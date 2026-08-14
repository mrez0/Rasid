# Rasid — Architecture

## Two kinds of memory

The app remembers things in two places, and they answer different questions:

- **The disk** — what exists *right now*. Files. Erasable, and the user can
  erase them behind the app's back.
- **The database** — what *happened*. History. Only the app writes it.

A `Video` row does not mean "a file I have". It means "a video I have seen and
dealt with". Whether its file exists right now is a separate question answered
by a separate field.

This matters because S-12 deletes files while S-07 forbids downloading twice.
Those two rules only coexist if the record outlives the file. Otherwise:
delete file → next check sees it as new → download → delete → forever.

## Make bad states impossible, not merely avoided

Duplicates could be prevented by checking before each download. But with 3
parallel downloads, two threads can both check, both see "not present", and
both proceed — a race that happens roughly never, until it does.

A unique constraint is enforced by SQLite itself: the second insert *fails*.
Not by our carefulness, but structurally.

> Prefer a wall to a warning sign.

---

## Project layout

```
Rasid/
├─ Rasid.sln
├─ PROGRESS.md
├─ docs/
└─ src/
   ├─ Rasid.Core/          ← no UI, no Avalonia reference
   │  ├─ Models/           Channel, Video, AppSettings
   │  ├─ Data/             RasidDbContext, migrations
   │  ├─ Abstractions/     IVideoSource, IDownloadService, INotifier, ...
   │  └─ Services/         RssVideoSource, YtDlpDownloader, ChannelChecker
   │
   ├─ Rasid.App/           ← Avalonia: Views, ViewModels, Program.cs
   │  ├─ ViewModels/
   │  ├─ Views/
   │  ├─ Converters/
   │  └─ Styles/
   │
   └─ Rasid.Tests/         ← tests against Core
```

```
   Rasid.App  ──────►  Rasid.Core  ◄──────  Rasid.Tests
        │                   │
        └── Avalonia        └── EF Core, Serilog
            CommunityToolkit
```

`Rasid.Core` must never reference Avalonia. That single constraint makes the
layering enforceable by the compiler rather than by discipline.

---

## Layers

Dependencies point inward. The View knows the ViewModel; the ViewModel does
not know the View exists.

```
View  →  ViewModel  →  Services  →  Model
```

| Layer | Question it answers | Example |
|---|---|---|
| Model | What is true? | This channel has ID `UC3Wzi...` and was last checked at 14:22 |
| ViewModel | What should be on screen, and what can the user do? | Show "Last checked: 2 minutes ago"; disable the Check button while a check runs |
| View | What does it look like? | Channel name bold, 16px, spinner to its right |

Rules of thumb:

- Model holds facts; ViewModel holds presentation state. `LastCheckedUtc` is a
  `DateTime` in the model; `"2 minutes ago"` is a string in the ViewModel.
  `IsChecking` is pure ViewModel — it exists only because a spinner spins.
- The ViewModel is the View written in C# instead of XAML. If a console UI
  could be written against it unchanged, the split is clean. If it mentions
  colours, pixels, or controls, it has leaked.
- Services do the work; ViewModels orchestrate. The ViewModel never calls
  `Process.Start` — it calls `IDownloadService`.

---

## Services

| Interface | Job | Notes |
|---|---|---|
| `IChannelResolver` | URL → channel ID, name, handle | yt-dlp, once at add time (D-007) |
| `IVideoSource` | Channel → recent videos | RSS + `/videos` (D-006) |
| `IDownloadService` | Video → file on disk | yt-dlp child process, reports `IProgress<T>` |
| `IExternalToolLocator` | Find yt-dlp and ffmpeg | Per-OS (D-013) |
| `IFileManager` | Delete old files safely | Enforces the D-002 rule |
| `INotifier` | Show a desktop notification | Per-OS (D-003) |
| `IDownloadQueue` | Hold the queue, cap concurrency | `SemaphoreSlim` |
| `ChannelCheckService` | The timer loop | `IHostedService` (D-011) |

`IExternalToolLocator`, `INotifier`, and tray behaviour are the OS-specific
seams from D-003. "Cross-platform" is not a property of the app — it is three
small classes.

---

## Flow, end to end

```
  PeriodicTimer ticks (every 30 min)
        │
        ▼
  ChannelCheckService ─── for each channel ───┐
        │                                     │
        ▼                                     │
  IVideoSource.GetRecentAsync()               │
   ├─ fetch RSS  ......... anything new?  ────┤ no → done
   └─ yes → yt-dlp /videos → authoritative list
        │
        ▼
  filter out IDs already in the DB  (S-07)
        │
        ▼
  IDownloadQueue.Enqueue(...)     ← SemaphoreSlim caps at 3
        │
        ▼
  IDownloadService.DownloadAsync(video, progress, token)
   ├─ spawn yt-dlp, parse stdout → IProgress<DownloadProgress>
   │        │
   │        └──► ViewModel updates ──► UI (marshal to UI thread!)
   ▼
  on success: write Video row, INotifier.Notify()
        │
        ▼
  IFileManager.EnforceRetention(channel)   ← keep newest N (S-12)
```

`IProgress<T>` is the only place data travels upward, from a service to a
ViewModel — and it does so through an interface the service was handed, so the
service still has no idea a UI exists.

---

## Data model

### Channel

One row per watched channel.

| Field | Type | Purpose |
|---|---|---|
| `Id` | `string` (PK) | The `UC...` ID from yt-dlp (D-007). Natural key, never changes. |
| `Handle` | `string?` | `@SummaryEgypt`. Display only; can change over time. |
| `Name` | `string` | Display name, e.g. "Summary". |
| `FolderName` | `string` | Sanitised name actually used on disk. Stored, not recomputed. |
| `AddedUtc` | `DateTime` | When the channel was added. |
| `LastCheckedUtc` | `DateTime?` | Null = never checked. Shown in the list (S-02). |
| `LastRssPublishedUtc` | `DateTime?` | Newest `<published>` seen. Enables the cheap RSS check. |
| `KeepCount` | `int?` | Null = use global default. Per-channel override (S-12). |
| `IsEnabled` | `bool` | Pause a channel without deleting it. |

**`FolderName` is stored, not computed.** Names can contain characters illegal
on disk (`:` on Windows), so they are sanitised. If it were recomputed and the
channel renamed itself, the app would look in a folder that no longer holds its
files. Storing it keeps disk and database in agreement.

**`LastRssPublishedUtc` is what makes polling cheap.** Fetch the feed, compare
the newest `<published>`. Same or older → stop, no yt-dlp call. Newer → fetch
the authoritative list. Most polls therefore cost one small XML request.
Uses `<published>`, never `<updated>` (see NOTES.md).

### Video

One row per video ever seen — not per file on disk.

| Field | Type | Purpose |
|---|---|---|
| `Id` | `string` (PK) | YouTube video ID, e.g. `dFlIToyEXow`. |
| `ChannelId` | `string` (FK) | Owning channel. |
| `Title` | `string` | For display. |
| `PublishedUtc` | `DateTime` | Ordering, and defining "newest N". |
| `DurationSeconds` | `double?` | Nullable — flat-playlist returns NA for some items. |
| `Status` | `enum` | `Pending`, `Downloading`, `Completed`, `Failed`, `Skipped`, `Deleted` |
| `FilePath` | `string?` | Where the file is. Null once deleted. |
| `FileSizeBytes` | `long?` | Powers the "how much will this free?" dialog (S-03). |
| `DownloadedUtc` | `DateTime?` | When it completed. |
| `DeletedUtc` | `DateTime?` | When retention removed the file. |
| `AttemptCount` | `int` | Retry logic (S-17). |
| `LastError` | `string?` | Shown in the UI when a download fails. |

The video ID as primary key **is** the anti-duplicate mechanism (D-015).
YouTube guarantees it is unique and permanent; a second insert cannot succeed.

`Status = Deleted` with `FilePath = null` is the row that says: *"downloaded
once, file removed to save space, do not fetch again."* That combination is
what breaks the download-delete-download loop (D-014).

### AppSettings

A single row, `Id = 1`.

| Field | Default |
|---|---|
| `DownloadRoot` | User's Videos folder |
| `CheckIntervalMinutes` | 30 |
| `KeepCountDefault` | 5 |
| `MaxParallelDownloads` | 3 |
| `MaxRetries` | 3 |
| `YtDlpPath` | null = auto-detect |
| `FfmpegPath` | null = auto-detect |
| `NotificationsEnabled` | true |
| `StartMinimised` | false |

Kept in the database rather than a JSON file: one storage mechanism instead of
two, transactional with everything else, and schema changes arrive through
migrations like any other table.

### Relationships

```
Channel  1 ────< many  Video
   │
   └── cascade delete: removing a channel removes its video rows (S-03)

AppSettings — standalone, exactly one row (Id = 1)
```

Cascade delete matters: without it, orphaned `Video` rows would accumulate
pointing at channels that no longer exist.

### Deferred

`DownloadAttempt` (one row per attempt, with raw yt-dlp output) is **not** in
v1. `AttemptCount` and `LastError` on `Video` cover S-17. The table would be
needed for S-25 (raw log viewer), which is v2 — see D-016.