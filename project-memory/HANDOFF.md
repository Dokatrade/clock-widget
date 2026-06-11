# Handoff

## Current State
- WPF/.NET 8 Windows desktop clock widget.
- Core UI: borderless transparent clock, date/weekday, tabbed settings window, visual presets, optional Pomodoro mode, sounds, tray icon, reset position command.
- Settings are stored in `%APPDATA%\ClockWidget\settings.json`.
- Settings load/save logic is in `ClockWidget/SettingsStore.cs`.
- `SettingsStore` has an internal constructor for test-only custom settings directories.
- Startup setting read/apply logic is in `ClockWidget/StartupSettingsService.cs`, backed by `ClockWidget/StartupManager.cs`.
- Settings dialog creation and Apply/OK dispatch logic is in `ClockWidget/SettingsDialogController.cs`.
- Display timer start/stop and adaptive next-tick scheduling logic is in `ClockWidget/DisplayTickScheduler.cs`.
- Display text/progress formatting logic is in `ClockWidget/WidgetDisplayFormatter.cs`.
- Tray menu logic is in `ClockWidget/TrayIconController.cs`.
- Window placement/snap/keep-on-screen logic is in `ClockWidget/WindowPlacementService.cs`.
- Testable placement math is in `ClockWidget/WindowPlacementGeometry.cs`.
- Pomodoro UI/session command logic is in `ClockWidget/PomodoroSession.cs`, backed by `PomodoroController`.
- Built-in visual presets are created by `WidgetSettings.CreateBuiltInPresets()`.
- Preset list/lookup/save/delete rules are in `ClockWidget/SettingsPresetCatalog.cs`; Settings UI shows built-in/custom/custom override labels, disables deleting built-ins, and resets custom overrides through Delete/Reset.
- Settings import/export is user-triggered from the Settings window and uses `SettingsStore.Serialize()` / `SettingsStore.Deserialize()`.
- `Apply` in Settings is enabled only when the current dialog draft differs from the last applied draft.
- Single-instance behavior now signals the existing app instance to show/activate the existing widget when a second process starts.
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
- As of 2026-06-11, `git ls-files --others --exclude-standard` is empty; the old new-file commit checklist is no longer needed.
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
- User reported the previous Windows verification checkpoint completed.
- Added built-in visual presets: `Compact`, `Large`, `Minimal`, `Pomodoro`.
- Settings presets now combine user presets with built-in presets; built-ins can be loaded but cannot be deleted, and user presets with the same name act as custom overrides.
- Added `WidgetSettings` tests for preset apply semantics and built-in preset freshness/normalization.
- Reorganized `SettingsWindow` into tabs: Presets, Appearance, Pomodoro, System.
- Reduced duplicated `MainWindow` context-menu/tray toggle and Pomodoro reset code with shared helper methods.
- Updated README for tabbed Settings and built-in presets.
- Extracted preset catalog logic to `SettingsPresetCatalog` and added tests for built-in/custom/custom override behavior.
- Settings preset UI now labels built-ins/customs/overrides, disables deleting built-ins, changes `Delete` to `Reset` for custom overrides, and shows `Save override` for built-in names.
- Settings `Apply` now tracks an exact in-memory draft diff against the last applied settings JSON.
- Added user-triggered Settings import/export.
- Added second-launch activation: duplicate process signals the first instance to show/activate the existing widget.
- Extracted `WindowPlacementGeometry` and added tests for clamp/snap/default-position math.

## Next Focus
- Next candidate: run `dotnet test .\ClockWidget.sln` on Windows after the latest Settings/preset/import/export/activation changes.
- Manually verify Settings tabs, preset labels/override reset, Apply dirty-state, import/export, second-launch activation, and snap/reset positioning after the next publish/run.
- Continue shrinking `MainWindow.xaml.cs` only when another clear responsibility boundary appears.
- Keep `project-memory` files updated after substantial work.

## Links
- См. `project-memory/TODO.md` для задач.
- См. `project-memory/docs/decisions.md` для решений.
