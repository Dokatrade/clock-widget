# Chat Notes

## Conversation Summary

- User asked for a careful repository analysis and improvement plan for the clock widget.
- Initial review found a WPF/.NET 8 app with clock UI, settings, presets, Pomodoro work in progress, sounds, startup registry logic, and a dirty worktree.
- Early priorities were repository stability, settings validation, preserving the user's minimal Pomodoro UI, and reducing future risk before adding features.
- User clarified that Pomodoro bottom words (`Focus`, `Break`, `Paused`) were intentionally removed. Do not reintroduce them without asking.
- User removed the duplicate root `sound/` folder. Active sound assets live under `ClockWidget/Assets/Sounds`.
- `ClockWidget/app.manifest` was ignored by `*.manifest`; `.gitignore` now has an exception so it can be tracked.
- Settings were normalized in `WidgetSettings` and persistence was moved to `SettingsStore`.
- Tray icon was added using WinForms `NotifyIcon`, then extracted to `TrayIconController`.
- Important tray constraint: Show/Hide from tray must not call `SaveSettings()` and must not write to disk.
- Single-instance guard was added in `App.xaml.cs`.
- Adaptive timer was added to reduce background UI updates: seconds/Pomodoro update near each second; clock without seconds updates near the next minute.
- Snap-to-edge was added, then adjusted after user feedback:
  - initial `EnsureWindowOnScreen()` margin made it look like the widget was pushed away from edges;
  - margin was removed;
  - snap was changed to use current monitor working area and a larger threshold.
- Window placement logic was extracted to `WindowPlacementService`.
- User declined Pomodoro notifications for now.
- File-based project memory was introduced. Current layout: root `AGENTS.md` contains Codex instructions and is auto-discovered; `project-memory/` contains handoff, TODO, decisions, and chat notes.

## Build Issues Seen In Chat

- After enabling WinForms:
  - `Application` ambiguity fixed by inheriting `System.Windows.Application`.
  - `Color` ambiguity fixed with `MediaColor` alias.
  - `MessageBox` ambiguity fixed with `WpfMessageBox` alias.
  - WFAC010 DPI warning addressed by moving DPI mode to `ApplicationHighDpiMode` and simplifying manifest.
- Assistant environment lacks `dotnet`, so build verification must be done by user with `.\publish.ps1`.

## Refactors Completed

- `SettingsStore`
- `TrayIconController`
- `WindowPlacementService`
- `PomodoroController`

## Latest Update

- Pomodoro state machine was extracted from `MainWindow.xaml.cs` into `ClockWidget/PomodoroController.cs`.
- Display timer ownership and adaptive next-tick scheduling were extracted from `MainWindow.xaml.cs` into `ClockWidget/DisplayTickScheduler.cs`.
- Repeated display brush allocations were reduced: stable text/break-progress colors now use frozen reusable brushes; focus Pomodoro progress still creates a dynamic frozen brush because the color changes.
- `PomodoroController` now uses injectable `IClock` from `ClockWidget/IClock.cs` instead of direct `DateTime.Now`, preparing it for deterministic tests.
- `ClockWidget.Tests` was added with xUnit v3 (`xunit.v3.mtp-v2` 3.2.2), `InternalsVisibleTo("ClockWidget.Tests")`, initial `PomodoroController` tests, and `WidgetSettings.Normalize()` tests.
- `SettingsStore` now has a test-only internal constructor for a custom settings directory; tests cover same-JSON no-write behavior, changed-settings writes, and invalid JSON fallback.
- `DisplayTickScheduler.GetNextInterval()` tests now cover second/minute scheduling and min/max interval clamping.
- `WidgetDisplayFormatter` now owns clock/Pomodoro display text, progress ratio, and progress color formatting; tests cover duration, clock display, Pomodoro display, and focus color interpolation.
- `SettingsDialogController` now owns `SettingsWindow` creation and Apply/OK dispatch; `MainWindow` applies the returned settings via one `ApplyUpdatedSettings` method.
- `Reset position` was added to the widget context menu and tray menu. It ignores `Lock position`, moves the widget to the top-right of the current screen work area, and saves the new coordinates.
- `MainWindow` still owns applying display models to WPF controls, sounds, tray/menu synchronization, settings application, and window placement calls.
- Startup setting read/apply try/catch logic was extracted from `MainWindow.xaml.cs` into `ClockWidget/StartupSettingsService.cs`; `MainWindow` still shows the warning message if applying the startup setting fails.
- Settings preset `Save`, `Load`, and `Delete` no longer invoke `SettingsApplied`; they edit only the dialog draft. `Apply` or `OK` commits changes to the live widget, so `Cancel` remains a true cancel.
- `ClockWidget/PomodoroSession.cs` was added to own Pomodoro display mode, command handling, completion transitions, start/pause menu text, and display tick state. `MainWindow` now uses `_pomodoroSession` instead of direct `_pomodoro` and `_displayMode` fields.
- `ClockWidget.Tests/PomodoroSessionTests.cs` was added for disabled toggle, start/pause visibility, break completion reset, and tick-state behavior.
- Follow-up memory correction: `ClockWidget/PomodoroSession.cs` was added to the untracked/commit checklist, new decisions were moved to the correct project date `2026-06-11`, and the `MainWindow` decomposition decision list now includes the latest extracted services.
- Available checks passed in assistant environment: no old Pomodoro state fields remain, and `git diff --check` is clean.
- Build/publish still needs Windows verification because assistant environment lacks `dotnet`.
- Date text was moved closer to the time in `ClockWidget/MainWindow.xaml` by changing `DateText` margin from `0,2,0,0` to `0,-2,0,0`.
- Project memory layout was updated: root `AGENTS.md` is the active instruction file; `project-memory/AGENTS.md` was removed; remaining memory files now reference `project-memory/...` paths.
- User reported the previous Windows verification checkpoint completed.
- Latest implementation added built-in visual presets `Compact`, `Large`, `Minimal`, `Pomodoro` via `WidgetSettings.CreateBuiltInPresets()`.
- Settings window now shows user presets plus built-in presets. Built-ins can be loaded but cannot be deleted; saving with the same name creates a user override.
- Added `WidgetSettings` tests for preset apply semantics and built-in preset freshness/normalization.
- Settings window was reorganized into tabs: `Presets`, `Appearance`, `Pomodoro`, `System`.
- `MainWindow.xaml.cs` now shares helper methods for always-on-top, lock-position, and visible Pomodoro reset actions between context menu and tray adapters.
- README was updated for tabbed Settings and built-in presets.
- Available check passed: `git diff --check`. `dotnet test` could not run in assistant environment because `dotnet` is not installed.
- Latest block extracted preset behavior to `ClockWidget/SettingsPresetCatalog.cs`; Settings UI now labels built-in/custom/custom override presets, disables deleting built-ins, changes custom override delete to reset, and shows `Save override` for built-in names.
- `Apply` in Settings now uses exact in-memory draft comparison against normalized settings JSON, so it only enables when the draft differs from the last applied state.
- Settings System tab now has user-triggered `Import` / `Export`; import changes only the dialog draft until `Apply` or `OK`, export writes only the selected file.
- Second app launch now signals the existing process through a named event and brings the existing widget forward.
- Window placement math moved to `ClockWidget/WindowPlacementGeometry.cs`; tests were added for clamp, snap, and default top-right position.
- New tests added: `SettingsPresetCatalogTests`, `WindowPlacementGeometryTests`, and an extra `SettingsStore` JSON round-trip test.

## User Preferences

- Russian language by default.
- Avoid unnecessary SSD writes.
- Keep Pomodoro visually minimal.
- No Pomodoro notifications for now.
- No new production dependencies without explicit approval.
- Do not keep routine `publish.ps1` / app launch checks as TODO items; user does them after every change. Briefly remind only at the end of changes.

## Local Tooling Note
- Assistant environment currently lacks `dotnet`, so builds cannot be verified here. User can verify via PowerShell:
  `cd "E:\My apps\clock-widget"` then `.\publish.ps1`.
