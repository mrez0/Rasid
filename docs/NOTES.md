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