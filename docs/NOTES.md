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
