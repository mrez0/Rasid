# Rasid — Technical Notes

One-line rules learned along the way, grouped by topic.

---

## .NET structure

- One `.csproj` = one assembly. A `.sln` (or `.slnx`) is only a grouping.
- Reference cycles are forbidden, so dependency direction is enforced by the
  toolchain, not by discipline.
- NuGet references are transitive: App can see EF Core because Core does.

---

## MVVM with CommunityToolkit

- `[ObservableProperty]` on `public partial string X { get; set; }` generates
  the change notification. The class must be `partial`.
- Source generators run at compile time; the generated code is real and
  navigable in the IDE.
- `[ObservableProperty]` only works on **stored, settable** properties.
- A computed property notifies via `[NotifyPropertyChangedFor]` on its source,
  or a manual `OnPropertyChanged(nameof(X))` when the source lives outside
  the ViewModel.
- Binding works on any public property. `INotifyPropertyChanged` is only
  needed for *changes*, not for the initial read.
- [RelayCommand(CanExecute = nameof(X))] makes the command gate itself.
  Bind Command only — do not bind IsEnabled; the command drives the button.
- Add [NotifyCanExecuteChangedFor(nameof(SomeCommand))] to whatever the
  CanExecute depends on, or it evaluates once and never updates.

---

## Avalonia

- `x:DataType` enables compiled bindings: typos become build errors
  (AVLN2000) and rename refactoring updates XAML. **Set it on every view.**
- Without `x:DataType`, bindings fall back to reflection and typos go silent
  again. The safety comes from the attribute, not the framework version.
- The message loop runs on the same thread as `Main`. Heavy work there freezes
  the window — this is why `DispatcherTimer` was rejected (D-011).
- Collections bound to the UI must only be modified on the UI thread. Use
  `Dispatcher.UIThread.Post`, or `IProgress<T>` created on the UI thread.
- ObservableCollection announces adds/removes only. Changes inside an item
  are that item's own INotifyPropertyChanged job.
- A DataTemplate needs its own x:DataType — inside it the DataContext is
  one item, not the parent ViewModel.
- ItemsSource, not Items (WPF habit that breaks here).
- RowDefinitions="Auto,*" is Avalonia shorthand for a two-row Grid.
- Design.DataContext cannot work once a ViewModel needs constructor
  arguments — the previewer builds it with `new`.
- `{Binding !Something}` — inline boolean negation, no converter needed
  (WPF requires a converter class for this).
- TextBox has a `Watermark` property for placeholder text.
- [RelayCommand] on `DoThingAsync` generates `DoThingCommand`, and async
  commands expose `IsRunning` — bind IsEnabled to `!Command.IsRunning`
  instead of tracking your own busy flag.

---

## Hosting and DI

- Registration only records a recipe. Nothing is created until something asks.
- Lifetimes: **singleton** (whole app), **transient** (new each time),
  **scoped** (avoid in desktop — there is no request boundary).
- A singleton must never depend on something shorter-lived — it captures one
  instance and holds it forever ("captive dependency").
- `GetRequiredService<T>` throws a clear error; `GetService<T>` returns null
  and gives you a blank window with no explanation.
- A static property with an initialiser runs the first time anything touches
  it — that is when our AppData folder actually gets created.
- Structured logging: `Log.Information("path {Folder}", value)`, not string
  interpolation. That is what makes the field searchable.
- `Log.CloseAndFlush()` in a `finally` block, or the last lines never reach
  disk.

---

## EF Core

- `DbContext` is **not** thread-safe. Inject `IDbContextFactory<T>`, create one
  per unit of work, dispose immediately with `await using`.
- Entities are plain classes. Attributes (`[Key]`, `[MaxLength]`) handle
  per-property rules; the fluent API handles relationships and indexes.
- With `<Nullable>enable</Nullable>`, a non-nullable `string` is already
  NOT NULL. `[Required]` is unnecessary.
- A navigation property (`Channel?`) is null until you `.Include()` it. The FK
  (`ChannelId`) is what's always there.
- `IDesignTimeDbContextFactory` exists only so `dotnet ef` can build a context
  outside the app. The app uses DI instead.
- A C# property initialiser (`= true`) is **not** a database default. EF writes
  every column on insert, so DB defaults rarely fire. Use `HasDefaultValue()`
  only if rows can be created outside your C# code.
- Enum values are stored as integers, so pin the numbers explicitly.
- Deleting a parent without .Include on its children lets the database
  cascade in one statement. With .Include, EF tracks each child and
  deletes them individually. Do not Include what you do not need.




---

## Databases

- A composite index `(A, B)` stores rows grouped by A, sorted by B within each
  group. It serves "filter on A, sort by B" with no sort step.
- Only usable left-to-right: `(A, B)` helps queries starting with A, never
  ones starting with B.
- Indexes cost time on insert and space on disk. Add them for queries you
  actually run.

---

## Dates and times

- Store UTC, display local. `DateTime.UtcNow`, never `DateTime.Now`.
- Convert with `ToLocalTime()` in the ViewModel — the OS knows Cairo is +2 or
  +3 today, so we never hardcode an offset.
- SQLite stores dates as TEXT and may lose `DateTimeKind` on read. If
  `ToLocalTime()` appears to do nothing, that is the cause.

---

## Async and concurrency

- `SemaphoreSlim` = a fixed number of permits. Take one before the work,
  release it after. Caps how many things run at once.
- Always `Release()` in a `finally` — a leaked permit is gone forever and the
  app slowly deadlocks.
- `SemaphoreSlim(1)` is an async-safe lock. Plain `lock` cannot be used with
  `await`.
- `await using` awaits the *disposal*, not the creation. Two awaits on one
  line handle two different moments: opening and closing.
- Use `await using` for anything implementing IAsyncDisposable (DbContext,
  streams, HTTP responses).
- `_ = SomeAsync()` is a discard — it silences CS4014 and signals that
  not awaiting was deliberate.
- Fire-and-forget swallows exceptions. Only safe when the method catches
  and logs everything itself.---
- When running a child process, start reading stdout AND stderr before
  awaiting WaitForExit. A full pipe buffer blocks the child forever —
  a silent deadlock with no error message.
- Use ArgumentList, never a single Arguments string. Each item is escaped
  for you; hand-built strings break on spaces and invite injection.
- Exit code 0 means success. stderr usually explains any other code.
- Anything read before an `await` may have changed by the time it resumes.
  Capture what you need into a local first.



## YouTube data sources

- A channel URL is three playlists. Always target `/videos`, `/shorts`, or
  `/streams` explicitly — item ranges apply to each tab separately.
- RSS: `youtube.com/feeds/videos.xml?channel_id=UC...` — needs the UC ID,
  returns the newest 15 items, mixes videos and live streams together.
- Feed-level `<yt:channelId>` omits the `UC` prefix; entry-level includes it.
  Never trust the feed for the channel ID — get it from yt-dlp at add time.
- Use `<published>`, never `<updated>`. `updated` changes on metadata edits
  and would make old videos look new.
- Treat duration as optional (`double?`). Flat-playlist returns NA for Shorts.
- RSS answers "did anything change?". `/videos` is the authoritative list.

---

## Testing and evidence

- A test where two different causes produce the same result has told you
  nothing. Design the test so the outcomes differ.
- When a fact is expensive to nail down, ask first whether the design can stop
  caring about it.

## Cross-platform

- Path.GetInvalidFileNameChars() returns a different set per OS. Use it
  rather than hardcoding a list of forbidden characters.