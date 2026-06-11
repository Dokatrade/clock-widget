# TODO

## Now
- [ ] Include new files when committing: `ClockWidget/SettingsPresetCatalog.cs`, `ClockWidget/WindowPlacementGeometry.cs`, `ClockWidget.Tests/SettingsPresetCatalogTests.cs`, `ClockWidget.Tests/WindowPlacementGeometryTests.cs`.
- [ ] Verify latest Settings changes on Windows: tabs render correctly, built-in/custom/custom override labels are clear, built-in presets load, custom overrides can be reset, `Apply` enables/disables correctly, and Apply/OK/Cancel semantics still hold.
- [ ] Verify user-triggered Settings import/export on Windows.
- [ ] Verify launching a second app instance brings the existing widget forward instead of silently exiting.

## Next
- [ ] Run `dotnet test .\ClockWidget.sln` on Windows after the latest Settings/preset/import/export/activation changes.
- [ ] Keep `project-memory/HANDOFF.md`, `project-memory/TODO.md`, `project-memory/docs/decisions.md`, and `project-memory/docs/chat-notes.md` updated after substantial implementation.

## Later
- [ ] Consider extending tests for other window-independent services.
- [ ] Continue shrinking `MainWindow.xaml.cs` if another clear responsibility boundary appears.
- [ ] Consider extracting Settings draft/dirty-state logic further if `SettingsWindow.xaml.cs` keeps growing.
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
- [x] User reported previous Windows verification completed: tests/publish/manual checks from the prior checkpoint.
- [x] Confirmed `git ls-files --others --exclude-standard` is empty; old new-file commit checklist is no longer needed.
- [x] Added built-in visual presets: `Compact`, `Large`, `Minimal`, `Pomodoro`.
- [x] Added `WidgetSettings` tests for preset apply semantics and built-in preset freshness/normalization.
- [x] Reorganized Settings window into tabs: Presets, Appearance, Pomodoro, System.
- [x] Reduced duplicated `MainWindow` context-menu/tray toggle and Pomodoro reset code.
- [x] Extracted preset list/lookup/save/delete rules into `SettingsPresetCatalog`.
- [x] Added tests for built-in/custom/custom override preset behavior.
- [x] Added precise Settings `Apply` dirty-state based on in-memory settings JSON comparison.
- [x] Added user-triggered Settings import/export.
- [x] Added second-launch activation of the existing app instance.
- [x] Extracted testable `WindowPlacementGeometry` and added clamp/snap/default-position tests.
- [x] Fixed WinForms implicit using conflicts for `Application`, `Color`, and `MessageBox`.
- [x] Moved DPI mode to project property and simplified `app.manifest`.
- [x] Set up file-based project memory and reformatted TODO/decisions.
- [x] Moved project instructions to root `AGENTS.md` for Codex auto-discovery and project state files to `project-memory/`.
