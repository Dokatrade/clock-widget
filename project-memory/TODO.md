# TODO

## Now
- [ ] Include new files when committing: `ClockWidget/SettingsPresetCatalog.cs`, `ClockWidget/WindowPlacementGeometry.cs`, `ClockWidget.Tests/SettingsPresetCatalogTests.cs`, `ClockWidget.Tests/WindowPlacementGeometryTests.cs`, `ClockWidget/PomodoroStatsWindow.xaml`, `ClockWidget/PomodoroStatsWindow.xaml.cs`, `ClockWidget/PomodoroStatsCalculator.cs`, `ClockWidget.Tests/PomodoroStatsCalculatorTests.cs`.
- [ ] Verify `Pomodoro Stats` context-menu dialog on Windows: opens centered over the widget, shows today/week/month/year count/minutes, current phase/remaining time, last-120-days heatmap, 12-month bar chart, and closes cleanly.
- [ ] Verify `Pomodoro Stats` reset on Windows: button asks for confirmation, `No` keeps data, `Yes` clears today/week/month/year/activity stats, updates widget daily stats if visible, and persists after restart.
- [ ] Verify `Pomodoro Stats` monthly focus chart on Windows: each month with data shows Pomodoro count above the bar and layout remains readable.
- [ ] Verify focused-widget `Space` shortcut on Windows: click/focus the widget while Pomodoro is visible, Space toggles start/pause only while the mouse is over the widget, and Space does nothing in normal clock mode.
- [ ] Verify Pomodoro long-break settings on Windows: Settings shows "use long break every N Pomodoro" and long-break duration controls, Apply/OK persists them, and every configured Nth completed Pomodoro starts the long break.
- [ ] Verify daily Pomodoro stats on Windows: disabled by default, Settings toggle works, stats show only in Pomodoro mode, hover tooltips are clear, focus completion increments count/minutes, and stats reset on a new local date.
- [ ] Verify side date on Windows: disabled by default, `Show side date` context-menu toggle works and persists, `Alt+left click` toggles it only in clock mode, day/month render as `dd` over `MM`, the block appears only in clock mode, and the right edge stays stable when toggled.
- [ ] Verify custom tray menu on Windows after outside-click fix: right-click tray icon opens the larger WPF menu near the cursor, checked/disabled states update, commands work, clicking empty Windows desktop/screen space closes it, and Show/Hide still does not save settings.
- [ ] Verify latest Settings changes on Windows: refreshed design renders correctly, tabs render correctly, built-in/custom/custom override labels are clear, built-in presets load, custom overrides can be reset, `Apply` enables/disables correctly, and Apply/OK/Cancel semantics still hold.
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
- [x] Added `Pomodoro Stats` context-menu item and dedicated Pomodoro stats dialog.
- [x] Extended `Pomodoro Stats` with today/week/month/year aggregates backed by daily history.
- [x] Replaced `Pomodoro Stats` cycle block with a hybrid activity block: larger last-120-days heatmap plus 12-month bar chart.
- [x] Added optional daily Pomodoro stats on the widget with two right-side green numbers and hover tooltips.
- [x] Added optional clock-mode side date with context-menu toggle and day/month numeric display.
- [x] Added larger custom WPF tray menu and enlarged the widget WPF context menu.
- [x] Hardened custom WPF tray menu dismissal by foreground-activating it and closing it on outside captured mouse clicks.
- [x] Added `Reset stats` button to `Pomodoro Stats` with confirmation, one explicit settings save, and immediate stats/widget refresh.
- [x] Added monthly Pomodoro count labels above the `Monthly focus` bars in `Pomodoro Stats`.
- [x] Added focused-widget `Space` shortcut for Pomodoro start/pause, active only while Pomodoro display is visible and the mouse is over the widget.
- [x] Added configurable Pomodoro long breaks: Settings controls for interval and long-break duration, persisted fields, normalization/tests, and runtime break selection on every configured Nth completed focus session.
- [x] Regenerated app icon assets from the user's supplied light rounded-square bright blue ring image.
- [x] Regenerated app icon assets from the user's supplied gray rounded-square blue ring image.
- [x] Regenerated app icon assets from the user's supplied white rounded-square clock image.
- [x] Regenerated app icon assets from the user's supplied orange rocket image.
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
