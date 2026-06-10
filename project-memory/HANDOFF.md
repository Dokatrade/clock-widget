# Handoff

## Current State
- WPF/.NET 8 Windows desktop clock widget.
- Core UI: borderless transparent clock, date/weekday, settings window, visual presets, optional Pomodoro mode, sounds, tray icon, reset position command.
- Settings are stored in `%APPDATA%\ClockWidget\settings.json`.
- Settings load/save logic is in `ClockWidget/SettingsStore.cs`.
- `SettingsStore` has an internal constructor for test-only custom settings directories.
- Startup setting read/apply logic is in `ClockWidget/StartupSettingsService.cs`, backed by `ClockWidget/StartupManager.cs`.
- Settings dialog creation and Apply/OK dispatch logic is in `ClockWidget/SettingsDialogController.cs`.
- Display timer start/stop and adaptive next-tick scheduling logic is in `ClockWidget/DisplayTickScheduler.cs`.
- Display text/progress formatting logic is in `ClockWidget/WidgetDisplayFormatter.cs`.
- Tray menu logic is in `ClockWidget/TrayIconController.cs`.
- Window placement/snap/keep-on-screen logic is in `ClockWidget/WindowPlacementService.cs`.
- Pomodoro UI/session command logic is in `ClockWidget/PomodoroSession.cs`, backed by `PomodoroController`.
- `MainWindow.xaml.cs` still owns main UI orchestration, startup warning UI, applying display models to WPF controls, sounds, and calls into the helper services.
- Reused/frozen display brushes are defined in `MainWindow.xaml.cs`; only the focus Pomodoro progress brush remains dynamic because its color changes with progress.
- Pomodoro phase/running/remaining state is in `ClockWidget/PomodoroController.cs`.
- `PomodoroController` uses injectable `IClock` from `ClockWidget/IClock.cs`; production default is `SystemClock.Instance`.
- Minimal tests live in `ClockWidget.Tests/` and use xUnit v3 (`xunit.v3.mtp-v2` 3.2.2) as a test-only dependency.
- Root `AGENTS.md` is the Codex auto-discovered project instruction file.
- `project-memory/` stores handoff, task queue, decisions, and chat notes for future Codex sessions. Before substantial work, follow root `AGENTS.md` and read the listed `project-memory` files.

## Important Context
- At the end of each completed task, briefly include how to rebuild and run the app only if files were changed and the change requires rebuilding or restarting the app:
  `cd "E:\My apps\clock-widget"`, `.\publish.ps1`, then `.\dist\ClockWidget.exe`.
- Do not keep routine publish/run verification as TODO items; the user does this after each change. Only briefly remind at the end of changes.
- Answer the user in Russian unless they ask otherwise.
- User cares about avoiding unnecessary SSD writes. Do not add periodic/background disk writes.
- Show/hide from tray must remain temporary and must not call `SaveSettings()`.
- Pomodoro status text such as `Focus`, `Break`, `Paused` was intentionally removed by the user. Do not restore it without asking.
- User declined Pomodoro notifications for now.
- Do not add production dependencies without explicit approval.
- `ClockWidget/app.manifest` is intentionally tracked despite `.gitignore` ignoring `*.manifest`.
- `ClockWidget/SettingsStore.cs`, `ClockWidget/TrayIconController.cs`, `ClockWidget/WindowPlacementService.cs`, and `ClockWidget/app.manifest` may be untracked until the user commits/adds them.
- `ClockWidget/StartupSettingsService.cs` may be untracked until the user commits/adds it.
- `ClockWidget/SettingsDialogController.cs` may be untracked until the user commits/adds it.
- `ClockWidget/DisplayTickScheduler.cs` may be untracked until the user commits/adds it.
- `ClockWidget/WidgetDisplayFormatter.cs` may be untracked until the user commits/adds it.
- `ClockWidget/IClock.cs` may be untracked until the user commits/adds it.
- `ClockWidget/Properties/AssemblyInfo.cs` and `ClockWidget.Tests/` may be untracked until the user commits/adds them.
- The assistant environment has no `dotnet`; build/publish verification must be done by the user on Windows.
- Project memory layout: keep durable instructions in root `AGENTS.md`; keep project state in `project-memory/HANDOFF.md`, `project-memory/TODO.md`, `project-memory/docs/decisions.md`, and `project-memory/docs/chat-notes.md`.

## Recent Changes
- Added/kept `.gitignore` exception for `ClockWidget/app.manifest`.
- Added settings normalization in `WidgetSettings`.
- Added `SettingsStore` for JSON load/save with no write if JSON is unchanged.
- Added WinForms tray icon and moved tray code into `TrayIconController`.
- Added single-instance mutex in `App.xaml.cs`.
- Added adaptive timer: seconds/Pomodoro tick near next second; clock without seconds ticks near next minute.
- Added `WindowPlacementService` for keep-on-screen, snap-to-edge, current monitor work area, and right-edge restore.
- Added optional `SnapToScreenEdges` setting in Settings window.
- README was updated for tray, adaptive timer, settings normalization, and snap.
- Project memory layout was updated to root `AGENTS.md` plus `project-memory/`.
- `project-memory/TODO.md` uses `Now` / `Next` / `Later` / `Done`.
- `project-memory/docs/decisions.md` is a dated decision log.
- Extracted Pomodoro state transitions from `MainWindow.xaml.cs` into `PomodoroController`.
- Raised the date closer to the time in `MainWindow.xaml` by changing `DateText` top margin from `2` to `-2`.
- Extracted startup setting read/apply try/catch logic from `MainWindow.xaml.cs` into `StartupSettingsService`.
- Extracted display timer start/stop and adaptive next-tick scheduling from `MainWindow.xaml.cs` into `DisplayTickScheduler`.
- Replaced repeated display text/break-progress brush allocations with frozen reusable brushes.
- Replaced direct `DateTime.Now` usage in `PomodoroController` with injectable `IClock`.
- Added `ClockWidget.Tests` with initial tests for `PomodoroController` and `WidgetSettings.Normalize()`.
- Added `SettingsStore` tests for same-JSON no-write behavior, changed-settings write behavior, and invalid JSON fallback.
- Added `DisplayTickScheduler.GetNextInterval()` tests for second/minute scheduling and interval clamping.
- Extracted display text/progress model formatting from `MainWindow.xaml.cs` into `WidgetDisplayFormatter`.
- Added `WidgetDisplayFormatter` tests for duration text, clock display, Pomodoro display, progress ratio, and focus progress color.
- Extracted Settings window creation and Apply/OK dispatch from `MainWindow.xaml.cs` into `SettingsDialogController`.
- Added `Reset position` to the widget context menu and tray menu; it moves the widget to the top-right of the current screen work area and saves the new position.
- Changed Settings preset actions to edit only the settings-window draft. `Save`, `Load`, and `Delete` presets no longer apply to the live widget until `Apply` or `OK`, so `Cancel` remains a real cancel.
- Added `PomodoroSession` to own Pomodoro display mode, command handling, completion transitions, menu header text, and display tick state.
- Added `PomodoroSessionTests`.

## Next Focus
- Next candidate: run `dotnet test .\ClockWidget.sln` on Windows and manually verify Settings preset `Cancel` semantics.
- Continue shrinking `MainWindow.xaml.cs` around tray action adapters if useful.
- Keep `project-memory` files updated after substantial work.

## Links
- См. `project-memory/TODO.md` для задач.
- См. `project-memory/docs/decisions.md` для решений.
