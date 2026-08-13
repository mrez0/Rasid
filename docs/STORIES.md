# Rasid — User Stories

Version: draft 2
Status: awaiting sign-off

---

## v1 — Must have

### Watch list

**S-01 — Add a channel**
As a user, I want to paste a YouTube channel URL and add it to my watch list,
so that the app knows which channel to monitor.
- Accepts common URL shapes (`/@handle`, `/channel/UC...`, `/c/name`)
- Resolves and stores the channel's real name and stable ID
- Rejects a URL that is not a channel, with a clear message
- Rejects a channel already in the list

**S-02 — See my channels**
As a user, I want to see all watched channels in a list with their name and
last-checked time, so that I know the app is working.

**S-03 — Remove a channel**
As a user, I want to remove a channel and its downloaded files,
so that it stops being checked and frees my disk.
- Confirmation dialog names the channel and shows how many files
  and how much disk space will be deleted
- Deletes the channel's folder and its contents
- **Refuses to delete if the folder contains files the app did not download**
  — warns instead and lets the user choose
- Video history rows are removed too, since the channel is gone

### Checking and downloading

**S-04 — Automatic checking**
As a user, I want the app to check every channel on a schedule,
so that I do not have to remember to do it.
- One global interval, default 30 minutes
- Checks run in the background without freezing the UI

**S-05 — Check now**
As a user, I want a "Check now" button,
so that I do not have to wait for the timer.
- Works for one channel or all channels

**S-06 — Download recent videos**
As a user, I want the app to download the 5 most recent videos of a channel,
so that I always have the latest content offline.
- Best available quality, video + audio
- Shorts are excluded
- Number "5" is a setting

**S-07 — Never download twice**
As a user, I want the app to remember every video it downloaded,
so that it never downloads the same video again — even if I deleted the file.

**S-08 — Limited parallel downloads**
As a user, I want no more than 3 downloads running at once,
so that my network and CPU stay usable.
- Cap is a setting

**S-09 — Live progress**
As a user, I want to see what is downloading now with percentage, speed,
and estimated time left, so that I know the app is alive and how long to wait.

**S-10 — Cancel a download**
As a user, I want to cancel a running download,
so that I can stop something I do not want.
- Partial file is cleaned up

### Files on disk

**S-11 — Folder per channel**
As a user, I want each channel's videos in its own folder,
so that my library stays organised.
- I choose the root download folder
- Folder name is derived safely from the channel name

**S-12 — Keep only the newest N**
As a user, I want the app to delete older downloads so only the newest N
videos remain per channel, so that my disk does not fill up.
- Default N = 5, adjustable globally and per channel
- Can be switched off per channel ("keep everything")
- **Only deletes files this app downloaded and still tracks. Never touches
  any other file in the folder.**
- Deletion is recorded; the video stays in history so it is not re-fetched

### Running in the background

**S-13 — System tray**
As a user, I want the app to keep running in the tray when I close the window,
so that checking continues in the background.
- Tray menu: Open, Check now, Quit
- Quit is the only thing that really exits

**S-14 — Clean restart**
As a user, I want the app to recover after being closed mid-download,
so that I never end up with half-broken files.
- On start: partial files are removed, those videos are simply re-checked

**S-15 — Notifications**
As a user, I want a desktop notification when a download finishes,
so that I know without watching the app.

### Settings and reliability

**S-16 — Settings screen**
As a user, I want to change interval, download folder, videos-to-keep,
and parallel limit, so that the app fits my machine.

**S-17 — Retry on failure**
As a user, I want failed downloads retried automatically a few times,
so that a short network problem does not lose a video.
- Default 3 attempts with increasing delay, then marked Failed
- I can retry a failed item by hand

**S-18 — Find yt-dlp**
As a user, I want the app to tell me clearly if yt-dlp is missing,
so that I know how to fix it instead of seeing a silent failure.

### Portability

**S-33 — Runs on macOS**
As a developer, I want to build and run Rasid on macOS,
so that I am not locked to one machine.
- No Windows-only APIs used directly
- Anything OS-specific sits behind an interface with a per-OS implementation
- Paths, folder names, and file separators handled portably
- Known OS-specific areas: notifications (S-15), tray icon (S-13),
  locating the yt-dlp binary (S-18)

---

## v2 — Nice to have

- **S-19** Playlists as well as channels, saved to a subfolder
- **S-20** Audio-only mode (per channel)
- **S-21** Subtitle options: none / Arabic / English / both, embedded or .srt
- **S-22** Dubbed audio track selection (original / preferred language / all),
  merged into MKV
- **S-23** Quality cap per channel (e.g. max 1080p)
- **S-24** Download history view with search and filter
- **S-25** Raw yt-dlp log viewer for troubleshooting
- **S-26** Per-channel check interval
- **S-27** Include or exclude live streams and premieres
- **S-28** Pause and resume a download
- **S-29** Import/export the channel list (OPML or JSON)
- **S-30** Light/dark theme switch
- **S-31** Auto-update yt-dlp
- **S-32** Bandwidth limit
- **S-34** Linux support (should mostly come free from S-33, but untested)

---

## Out of scope

Decided against, on purpose. Not "later" — v2 is for later.

- **Any account system, login, or cloud sync** — one user, one machine, so no
  UserId anywhere in the data model
- **Multi-user or multi-machine sharing** — no server, no API layer
- **Built-in video player** — the OS already has one; huge effort, no gain
- **Mobile version** — Avalonia can do it, but the whole design assumes a
  desktop filesystem and a background process
- **Re-uploading or sharing downloaded content** — personal offline use only
- **Anything commercial or distributed to other people** — keeps us out of
  trademark and licensing questions entirely