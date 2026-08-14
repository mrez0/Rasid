# Avalonia Notes

## YouTube data sources

- A channel URL is three playlists. Always target `/videos`, `/shorts`,
  or `/streams` explicitly.
- RSS: `youtube.com/feeds/videos.xml?channel_id=UC...` — needs the UC ID,
  returns the newest 15 items, mixes videos and live streams together.
- RSS feed-level `<yt:channelId>` omits the "UC" prefix. Entry-level includes
  it. Never trust the feed for the channel ID — get it from yt-dlp at add time.
- Use `<published>`, never `<updated>`. `updated` changes on metadata edits
  and would make old videos look new.
- Treat duration as optional (`double?`). Flat-playlist returns NA for Shorts.
- RSS answers "did anything change?". `/videos` is the authoritative list.
- Collections bound to the UI must only be modified on the UI thread.
  Use Dispatcher.UIThread.Post, or IProgress<T> created on the UI thread.
- 
## Indexes
- A composite index (A, B) stores rows grouped by A, sorted by B within
  each group. It serves "filter on A, sort by B" with no sort step.
- Only usable left-to-right: (A, B) helps queries starting with A, never
  ones starting with B.
- Indexes cost time on insert and space on disk. Add them for queries you
  actually run.

- A C# property initialiser (= true) is not a database default. EF writes
  every column on insert, so DB defaults rarely fire. Use HasDefaultValue()
  only if rows can be created outside your C# code.

## EF Core
- DbContext is NOT thread-safe. Inject IDbContextFactory<T>, create one per
  unit of work, dispose immediately with `await using`.
- Entities are plain classes. Attributes ([Key], [MaxLength]) handle
  per-property rules; the fluent API handles relationships and indexes.
- With <Nullable>enable</Nullable>, a non-nullable string is already
  NOT NULL. [Required] is unnecessary.
- A navigation property (Channel?) is null until you .Include() it.
  The FK (ChannelId) is what's always there.
- IDesignTimeDbContextFactory exists only so `dotnet ef` can build a
  context outside the app. The app uses DI instead.
- A C# initialiser (= true) is not a database default.
- Composite index (A, B): grouped by A, sorted by B within each group.
  Only usable left-to-right.

- DI registration only records a recipe. Nothing is created until
  something asks for it.
- A static property with an initialiser runs the first time anything
  touches it — that is when our AppData folder actually gets created.
- The message loop runs on the same thread as Main. Heavy work there
  freezes the window. This is why DispatcherTimer was rejected (D-011).
- ## Hosting and DI
 
- Registration only records a recipe. Nothing exists until something asks.
- Lifetimes: singleton (whole app), transient (new each time), scoped
  (avoid in desktop — there is no request boundary).
- A singleton must never depend on something shorter-lived — it captures
  one instance and holds it forever ("captive dependency").
- GetRequiredService<T> throws a clear error; GetService<T> returns null
  and gives you a blank window with no explanation.
- Log.CloseAndFlush() in a finally block, or the last lines never reach disk.
- Structured logging: Log.Information("path {Folder}", value) — not string
  interpolation. That is what makes the field searchable.