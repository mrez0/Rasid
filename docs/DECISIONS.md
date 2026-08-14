# Rasid — Decisions Log

A lightweight ADR (Architecture Decision Record) log.
Each entry: what we decided, why, and what it rules out.
Decisions are numbered and never deleted — if one changes, we add a new
entry that supersedes it, so the history of our thinking stays readable.

---

## D-001 — Target framework: net10.0
**Date:** Phase 0
**Decision:** Target `net10.0`, C# 14.
**Reason:** The user has the .NET 10 SDK installed, and the Avalonia 12.1.1
MVVM template already requires it. .NET 10 is LTS.
**Rejected:** `net8.0` — only useful for shipping to machines with an older
runtime, which does not apply to a personal single-machine project.
**Consequence:** We can use the newest C# language features freely.

---

## D-002 — File deletion safety
**Date:** Phase 0
**Decision:** The app may only delete files it downloaded and still tracks in
the database. Whole-folder deletion (S-03) refuses to run if unknown files are
present in the folder, and warns the user instead.
**Reason:** File deletion is the only irreversible action in this app. Every
other bug can be fixed by restarting; a wrong delete cannot be undone.
**Consequence:** Every downloaded file's exact path is stored in the database.
Two different deletion paths exist with different rules — S-12 deletes specific
tracked files, S-03 deletes a folder and therefore needs the extra guard.

---

## D-003 — macOS is a v1 target
**Date:** Phase 0
**Decision:** Rasid must build and run on macOS as well as Windows (S-33).
No direct Windows API calls anywhere.
**Reason:** The user works across both machines and does not want to be locked
to one. Retrofitting cross-platform support later is far more expensive than
designing for it now.
**Consequence:** Three areas must sit behind interfaces with per-OS
implementations: notifications (S-15), tray icon behaviour (S-13), and locating
the yt-dlp binary (S-18). Paths must never be built with hardcoded separators.
Linux support is likely a free side effect but stays untested in v1 (S-34).

---

## D-004 — Planning lives in Markdown in the repo
**Date:** Phase 0
**Decision:** Stories, decisions, architecture, notes, and progress are
Markdown files under version control, not an external tool.
**Reason:** Works offline and behind a restricted network, no accounts, no
install, versioned alongside the code, and readable by AI coding assistants.
**Rejected:** Jira, Trello, Notion — overhead for a single developer.
GitHub Issues + Projects stays available later if a board is ever wanted.

---

## D-005 — App name: Rasid
**Date:** Phase 0
**Decision:** The app is called **Rasid** (راصد, "observer / watcher").
Solution `Rasid.sln`, root namespace `Rasid`.
**Reason:** Short, meaningful, unlikely to clash on GitHub or NuGet, and avoids
leading with "YouTube" or "YT" which sits awkwardly against Google's trademark
guidelines.
**Rejected:** `YtWatcher` (trademark grey area), `ChannelKeeper` (generic).

---

---

## D-006 — Video detection: RSS first, yt-dlp second
**Date:** Phase 0
**Decision:** Poll each channel's RSS feed on the schedule. When it shows
anything new, call `yt-dlp --flat-playlist` on the channel's `/videos` tab to
get the authoritative list, and download from that.
**Reason:** RSS is free, unlimited, and fast (~1s), so we can poll often at no
cost. yt-dlp is slow and can be throttled, so we call it rarely and only when
there is a reason to. Neither alone is enough: RSS mixes live streams in with
videos (verified — `MWZtC4Ns1LE` appeared in the feed), and yt-dlp is too
expensive to run every 30 minutes across many channels.
**Rejected:** YouTube Data API v3 — requires a Google Cloud project and API
key, and `search.list` costs 100 quota units per call against a 10,000/day
limit. Too much setup and too tight a budget for a personal app.
**Consequence:** Both go behind `IVideoSource`, so either half can be replaced
if YouTube changes. Shorts are excluded for free, since we only ever download
what `/videos` returns (S-06). Live streams (S-27, v2) become a second URL
suffix rather than a filter.

**Verified findings that shaped this:**
- A channel URL expands to three playlists (Videos / Shorts / Streams). Item
  ranges apply to each separately — `--playlist-items 1:15` returned 45 rows.
  Always target the tab explicitly.
- RSS returns the newest 15 items and mixes videos with live streams.
- Feed-level `<yt:channelId>` omits the `UC` prefix; entry-level includes it.
- `<updated>` changes on metadata edits, so only `<published>` is safe for
  detecting new items.

---

## D-007 — Channel resolution via yt-dlp at add time
**Date:** Phase 0
**Decision:** When a channel is added, run yt-dlp once against the pasted URL
and read `playlist_channel_id` (the UC ID), `playlist_channel` (display name),
and `playlist_uploader_id` (the @handle). Store the UC ID as the primary key.
**Reason:** RSS requires the UC ID, but users paste `@handle` URLs. yt-dlp
resolves every URL shape reliably. Handles can change; the UC ID never does.
**Rejected:** Scraping the channel page for `externalId` — fragile against
YouTube redesigns. The Data API — rejected in D-006.
**Consequence:** Adding a channel takes a few seconds and needs a progress
indicator (S-01). Costs nothing afterwards, since it happens once per channel.

---

## D-008 — Persistence: EF Core + SQLite
**Date:** Phase 0
**Decision:** EF Core with a SQLite file for all app data.
**Reason:** S-07 requires checking "have I seen this video ID?" against a
growing history on every check — a database query, not a file scan. SQLite
needs no server or install and behaves identically on Windows and macOS.
EF Core's migrations and change tracking map closely onto Eloquent, which the
developer already knows.
**Rejected:** Plain JSON — a full load-and-rewrite per change, and a crash
mid-write corrupts everything. Dapper — hand-written SQL and manual schema
management, for performance we will never need.
**Consequence:** `DbContext` is not thread-safe, and this app has background
timers plus parallel downloads. We must use `IDbContextFactory` and create a
short-lived context per unit of work. This is the most likely source of a
confusing runtime crash in the whole app.

---

## D-009 — MVVM library: CommunityToolkit.Mvvm
**Date:** Phase 0
**Decision:** CommunityToolkit.Mvvm, using `[ObservableProperty]` and
`[RelayCommand]` source generators.
**Reason:** The learning goal is MVVM itself, not a second framework. The
Toolkit keeps the pattern visible with minimal ceremony, and the
`SimpleToDoList` sample already studied uses it. Maintained by Microsoft.
**Rejected:** ReactiveUI — genuinely powerful for complex async coordination,
but adds a whole reactive paradigm on top of C#, XAML, and MVVM all being new
at once. Can be revisited later.
**Consequence:** Some Avalonia documentation and samples assume ReactiveUI; we
translate rather than copy.

---

## D-010 — DI and lifetime: Microsoft.Extensions.Hosting
**Date:** Phase 0
**Decision:** Build a generic `Host` at startup. Services registered in its DI
container; background work implemented as `IHostedService`.
**Reason:** This app is largely a background service with a UI attached — it
runs timers, spawns child processes, and must shut down without corrupting
files (S-14). The host provides the container, ordered startup, and graceful
shutdown as one package. The DI concept transfers directly from Laravel's
service container.
**Rejected:** A bare `ServiceCollection` plus hand-managed threads — less to
learn, but shutdown ordering is exactly the part that is easy to get wrong.
**Consequence:** ~20 lines of unfamiliar setup in `Program.cs` before any UI
work. Avalonia's own lifetime must be integrated with the host's.

---

## D-011 — Scheduling: PeriodicTimer inside an IHostedService
**Date:** Phase 0
**Decision:** A `PeriodicTimer` in an `await` loop, hosted by an
`IHostedService`, driven by a `CancellationToken`.
**Reason:** The await loop makes re-entrancy structurally impossible — the next
tick cannot fire while the previous check is still running. Cancellation is
built in, so app shutdown stops the loop cleanly.
**Rejected:** `DispatcherTimer` — runs on the UI thread and would freeze the
window during a check. This is the WPF habit most likely to be carried over
by mistake. `System.Timers.Timer` — fires regardless of whether the previous
run finished, causing overlapping checks and duplicate downloads; avoidable
only with a lock we would have to write correctly. Quartz.NET — cron
expressions, persistence, and clustering, none of which we need.
**Consequence:** A long-running check delays the next one rather than
overlapping it. Each check gets its own timeout via `CancellationToken` so one
hanging channel cannot stall the loop indefinitely.

---

## D-012 — Logging: Serilog
**Date:** Phase 0
**Decision:** Serilog, writing to a rolling file plus the debug output.
**Reason:** Failures happen in the background with nobody watching. The log is
the only witness. Structured logging gives searchable fields rather than flat
strings.
**Consequence:** Registered through the host (D-010), so every service can take
an `ILogger<T>` by injection.

---

## D-013 — yt-dlp and ffmpeg: required, not bundled
**Date:** Phase 0
**Decision:** The user installs yt-dlp and ffmpeg themselves. The app locates
them via `IExternalToolLocator`: a configured path first, then PATH, then
known per-OS install locations. A clear message with the install command for
the current OS if either is missing (S-18).
**Reason:** yt-dlp updates roughly weekly because YouTube keeps changing. A
bundled copy goes stale and silently stops working. Bundling also means a
separate binary per OS, conflicting with D-003.
**Rejected:** Shipping the binaries — instant setup, but ~30 MB of
platform-specific files in the repo that break within weeks.
**Consequence:** First-run needs a clear setup message. ffmpeg matters as much
as yt-dlp: yt-dlp downloads video and audio as separate streams and needs
ffmpeg to merge them. Missing ffmpeg produces a confusing partial failure, so
we check for both at startup.

## D-014 — Video rows outlive their files
**Decision:** A `Video` row is never deleted when its file is removed by
retention. Status becomes `Deleted` and `FilePath` becomes null.
**Reason:** S-07 (never download twice) and S-12 (delete old files) are only
compatible if history survives deletion. Otherwise a deleted file looks new on
the next check and downloads forever.
**Consequence:** The `Video` table grows without bound — acceptable, since a
row is tiny and even years of history stays under a few MB.

## D-015 — Dedup enforced by primary key
**Decision:** The YouTube video ID is the primary key of `Video`.
**Reason:** With up to 3 parallel downloads, a check-then-insert pattern has a
race window where two threads both see "not present" and both insert. A PK
makes the second insert fail at the database level.

## D-016 — DownloadAttempt table deferred
**Decision:** No attempt-history table in v1. `AttemptCount` and `LastError`
on `Video` cover S-17.
**Reason:** S-25 (raw log viewer) is v2. Adding the table later is a migration.

---

## D-017 — Partial-property syntax for [ObservableProperty]
**Date:** Step 2
**Decision:** Use the partial-property form:
`[ObservableProperty] public partial string Greeting { get; set; }`
rather than the older private-field form (`private string _greeting;`).
**Reason:** Requires CommunityToolkit.Mvvm 8.4+ and C# 13, both available on
net10.0, and the Avalonia 12 template already generates this form. Removes the
`_field` → `Property` naming translation, which was the most common source of
confusion with the generator. The property is visible in the source file rather
than only in generated code.
**Consequence:** The class must still be `partial`. Property initialisers are
allowed (`{ get; set; } = "..."`), contrary to an early assumption.

---

## D-018 — Compiled bindings
**Date:** Step 2
**Decision:** Rely on Avalonia 12's default, which compiles `{Binding}`
expressions when `x:DataType` is declared on the view. No csproj setting added.
**Verified:** A deliberate typo (`{Binding Greetng}`) produced build error
AVLN2000 with file, line, column, and the type checked against — not a silent
runtime failure as it would be in WPF.
**Reason:** Binding typos become build errors, and rename refactoring updates
XAML. This is one of Avalonia's clearest improvements over WPF.
**Consequence:** **Every view must declare `x:DataType`.** Without it, bindings
fall back to reflection and typos become silent again. The safety comes from
`x:DataType`, not from the framework version.

## D-019 — Times stored in UTC, converted for display only
**Date:** Step 3
**Decision:** All DateTime fields store UTC. Always use `DateTime.UtcNow`,
never `DateTime.Now`. Conversion to local time happens in ViewModels via
`ToLocalTime()`, never in the model or services.
**Reason:** UTC has no DST ambiguity. Cairo is +2 in winter and +3 in summer;
the OS knows the current rule, so we never hardcode an offset. This also keeps
the Model-holds-facts / ViewModel-holds-presentation split intact.
**Rejected:** DateTimeOffset — more correct in general, but every value would
carry +00:00, so it adds ceremony without adding information.
**Risk:** SQLite stores dates as TEXT and may lose DateTimeKind on read. If
`ToLocalTime()` appears to do nothing, that is the cause — fix with a
HasConversion that stamps DateTimeKind.Utc on read.

## D-020 — Output layout targets Jellyfin
**Date:** Step 4
**Decision:** Downloads are organised for a Jellyfin media server, which the
user runs at home to watch the library from any device.
**Consequences for step 11 (download service):**
- Folder per channel remains correct (S-11); Jellyfin reads each channel
  folder as a "show".
- Filenames include the upload date so Jellyfin sorts chronologically:
  `{channel} - {YYYY-MM-DD} - {title}.{ext}`
- Pass `--write-info-json` so Jellyfin's YouTube metadata plugin can read
  real titles, descriptions, and upload dates instead of guessing.
- Pass `--write-thumbnail` so videos show real thumbnails.
- Sanitised filenames matter more than before: Jellyfin scans the folder,
  so a broken name means a broken library entry.
  **Cost:** Two extra yt-dlp flags and a filename template. Nothing structural.
  **Note:** Watch-progress tracking is Jellyfin's job, not ours — this is why
  watched/unwatched stayed out of scope.