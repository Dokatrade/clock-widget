# TODO

## Now
- [ ] Confirm new files are included when committing: `ClockWidget/SettingsStore.cs`, `ClockWidget/SettingsDialogController.cs`, `ClockWidget/TrayIconController.cs`, `ClockWidget/WindowPlacementService.cs`, `ClockWidget/PomodoroController.cs`, `ClockWidget/StartupSettingsService.cs`, `ClockWidget/DisplayTickScheduler.cs`, `ClockWidget/WidgetDisplayFormatter.cs`, `ClockWidget/IClock.cs`, `ClockWidget/Properties/AssemblyInfo.cs`, `ClockWidget.Tests/`, `ClockWidget/app.manifest`.

## Next
- [ ] Verify Settings preset `Save` / `Load` / `Delete` only commit after `Apply` or `OK`, and `Cancel` leaves the live widget unchanged.
- [ ] Verify tray Show/Hide does not rewrite `%APPDATA%\ClockWidget\settings.json`.
- [ ] Verify tray menu items: Settings, Always on top, Lock position, Pomodoro show/start/reset, Exit.
- [ ] Verify single-instance behavior by launching the app twice.
- [ ] Verify adaptive timer visually: seconds on/off and Pomodoro countdown.
- [ ] Verify Pomodoro behavior after controller extraction: start, pause, resume, reset, focus-to-break, break-to-clock.
- [ ] Verify snap-to-edge with `Snap to screen edges` enabled and disabled.
- [ ] Keep `project-memory/HANDOFF.md`, `project-memory/TODO.md`, `project-memory/docs/decisions.md`, and `project-memory/docs/chat-notes.md` updated after substantial implementation.

## Later
- [ ] Run `dotnet test .\ClockWidget.sln` on Windows.
- [ ] Consider extending tests for other window-independent services.
- [ ] Consider shrinking `MainWindow.xaml.cs` around tray action adapters.
- [ ] Consider built-in visual presets.
- [ ] Consider better Settings UI grouping/tabs if the window grows further.
- [ ] Pomodoro notifications: user declined for now.
- [ ] New production dependencies: require explicit user approval.
- [ ] Consider adding tests if project structure becomes more service-oriented.

## Done
- [x] Added settings normalization and `SettingsStore`.
- [x] Added tray icon and extracted `TrayIconController`.
- [x] Added single-instance guard.
- [x] Added adaptive timer.
- [x] Added optional snap-to-edge setting.
- [x] Extracted `WindowPlacementService`.
- [x] Extracted Pomodoro state machine into `PomodoroController`.
- [x] Extracted startup setting read/apply handling into `StartupSettingsService`.
- [x] Extracted display timer scheduling into `DisplayTickScheduler`.
- [x] Reduced repeated brush allocations in display updates.
- [x] Prepared `PomodoroController` for tests with injectable `IClock`.
- [x] Added minimal xUnit v3 tests for `PomodoroController` and `WidgetSettings.Normalize()`.
- [x] Added `SettingsStore` tests for no-write and load fallback behavior.
- [x] Added `DisplayTickScheduler.GetNextInterval()` tests.
- [x] Extracted display formatting into `WidgetDisplayFormatter` and added tests.
- [x] Extracted settings dialog orchestration into `SettingsDialogController`.
- [x] Added Reset position command to context menu and tray menu.
- [x] Changed preset actions in Settings to stay in the dialog draft until `Apply` or `OK`.
- [x] Extracted Pomodoro display/command session logic into `PomodoroSession` and added tests.
- [x] Fixed WinForms implicit using conflicts for `Application`, `Color`, and `MessageBox`.
- [x] Moved DPI mode to project property and simplified `app.manifest`.
- [x] Set up file-based project memory and reformatted TODO/decisions.
- [x] Moved project instructions to root `AGENTS.md` for Codex auto-discovery and project state files to `project-memory/`.
