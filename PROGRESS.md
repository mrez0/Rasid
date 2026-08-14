# Rasid — Progress

## Phase 0 — Planning
- [x] 0.1 Requirements gathering
- [x] 0.2 User stories (docs/STORIES.md)
- [x] 0.3 Technology decisions (D-001 to D-016)
- [x] 0.4 Architecture (docs/ARCHITECTURE.md)
- [x] 0.5 Data model
- [x] 0.6 Roadmap

## Part 1 — Foundations

- [x] **1.** Solution and projects — create Rasid.Core, Rasid.App, Rasid.Tests;
  wire references; first run of the empty Avalonia window.
- [ ] **2.** MVVM basics — ViewModelBase, CommunityToolkit
  `[ObservableProperty]`, compiled bindings with `x:DataType`, why
  reflection bindings are worse.
- [ ] **3.** Database — EF Core, `RasidDbContext`, the Channel and Video
  entities, first migration, `IDbContextFactory` and why it is mandatory.
- [ ] **4.** Hosting and DI — `Microsoft.Extensions.Hosting`, wiring Avalonia's
  lifetime to the host, Serilog, first injected service.

## Part 2 — Channels

- [ ] **5.** Channel list UI — `ObservableCollection`, `ItemsControl` vs
  `ListBox`, `DataTemplate`, ViewModel-first views. Reads real rows.
- [ ] **6.** Adding a channel — `IChannelResolver` running yt-dlp, async command
  with a busy state, validation and error display (S-01).
- [ ] **7.** Removing a channel — confirmation dialogs done the MVVM way,
  cascade delete, the file-safety guard from D-002 (S-03).

## Part 3 — Detection

- [ ] **8.** RSS reading — `IVideoSource`, `HttpClient`, XML parsing, the
  `published` vs `updated` trap, `LastRssPublishedUtc` short-circuit.
- [ ] **9.** yt-dlp listing — `IExternalToolLocator`, spawning a process and
  reading its output, the `/videos` tab rule, JSON parsing (S-18).
- [ ] **10.** Detecting what is new — comparing against the DB, the primary-key
  dedup rule in practice, "Check now" button (S-05, S-07).

## Part 4 — Downloading

- [ ] **11.** The download service — `IDownloadService`, spawning yt-dlp,
  `CancellationToken` plumbing, writing to the right folder (S-06, S-11).
- [ ] **12.** Progress reporting — parsing yt-dlp's stdout, `IProgress<T>`,
  marshalling to the UI thread, `ProgressBar` binding (S-09).
- [ ] **13.** The queue — `IDownloadQueue`, `SemaphoreSlim`, concurrency cap,
  cancel a running download (S-08, S-10).
- [ ] **14.** Retry and failure — `AttemptCount`, backoff, manual retry, showing
  `LastError` in the UI (S-17).

## Part 5 — Running unattended

- [ ] **15.** The scheduler — `IHostedService` + `PeriodicTimer`, per-check
  timeout, graceful shutdown (S-04).
- [ ] **16.** Retention — `IFileManager`, keep newest N, the D-002 safety rule,
  `Status = Deleted` (S-12).
- [ ] **17.** Tray icon — Avalonia `TrayIcon`, close-to-tray vs quit, macOS
  differences (S-13).
- [ ] **18.** Notifications — `INotifier` with per-OS implementations (S-15).
- [ ] **19.** Clean restart — detecting and removing partial files on
  startup (S-14).

## Part 6 — Settings and polish

- [ ] **20.** Settings screen — the AppSettings row, folder picker, live
  re-read of the interval (S-16).
- [ ] **21.** Styling I — Avalonia selectors vs WPF `Style`/`TargetType`,
  pseudo-classes (`:pointerover`, `:pressed`) as trigger replacements.
- [ ] **22.** Styling II — `ControlTheme`, retemplating a control, `ItemsPanel`,
  transitions and animations.
- [ ] **23.** Converters — `IValueConverter`, `MultiBinding`, `StringFormat`,
  and when a ViewModel property is the better answer.
- [ ] **24.** Testing — unit tests against Core, faking `IVideoSource` and
  `IDownloadService`, why the interfaces made this possible.
- [ ] **25.** macOS run — building and running on Mac, fixing what breaks (S-33).
- [ ] **26.** Packaging — publishing a runnable app on both platforms.

## Notes
- DevTools (F12) is introduced in step 5 and used throughout.
- `NOTES.md` gets an entry whenever a new Avalonia concept appears.
- `DECISIONS.md` gets an entry whenever we choose between real alternatives.