# Rasid — Decisions Log

A lightweight ADR (Architecture Decision Record) log.
Each entry: what we decided, why, and what it rules out.
Decisions are numbered and never deleted — if one changes, we add a new
entry that supersedes it, so the history of our thinking stays readable.

---

## D-001 — Target framework: net10.0
**Date:** Phase 0
**Decision:** Target `net10.0`, C# 14.
**Reason:** The user has the .NET 10 SDK installed, and the Avalonia 11
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