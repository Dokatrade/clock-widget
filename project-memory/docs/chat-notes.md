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
- Settings window design was refreshed in `ClockWidget/SettingsWindow.xaml` only: larger dialog, local resource styles, pill-like tabs, section panels, and styled dialog/action buttons. The extra top `Clock Settings` header block was removed after user feedback. No code-behind behavior was changed.
- App icon assets `ClockWidget/Assets/AppIcon.ico` and `ClockWidget/Assets/AppIconSource.png` were regenerated from the user's supplied light rounded-square image with a bright blue ring. Black outside the rounded square was made transparent; ICO frames include 256/128/64/48/32/16.
- User asked to add optional Pomodoro daily stats to the widget. Implementation shows two green right-side numbers in Pomodoro mode only when enabled: completed Pomodoros today and focus minutes today. Each number has a WPF tooltip. The Settings Pomodoro tab has a `Show daily Pomodoro stats` checkbox. Stats are stored in `WidgetSettings`, reset by local `yyyy-MM-dd`, increment only when a focus phase completes, and are saved on focus completion or normal settings saves, not through periodic/background writes.
- User asked to add a `Pomodoro Stats` context-menu item that opens a dedicated window like Settings but focused on stats. Added `ClockWidget/PomodoroStatsWindow.xaml` / `.xaml.cs`; the dialog shows today's Pomodoro count, today's focus minutes, current phase, remaining time, and configured focus/break cycle. `MainWindow` opens it from the widget context menu.
- User asked to add an optional date block to the right of the clock: day number above month number, toggled from the widget context menu, and hidden in Pomodoro mode. Added persisted `ShowSideDate`, context-menu `Show side date`, side date formatting as `dd` / `MM`, and tests for formatting/preset persistence.
- Side date shortcut: `Alt+left click` on the widget toggles side date only while the clock display is visible. In Pomodoro display mode it intentionally does nothing and does not save settings.
- User asked whether context menu text could be made larger. Native Windows tray menus cannot be reliably resized per app, so the tray menu was replaced with a custom WPF `TrayMenuWindow` opened from `NotifyIcon.MouseUp` on right-click. The custom menu uses larger 16px text/42px rows, keeps the existing tray actions, updates checked/disabled states from `TrayIconState`, hides on deactivation, and positions itself near the cursor within the current screen work area. The widget's existing WPF context menu also received local larger `ContextMenu` / `MenuItem` styles.
- User reported that the custom tray menu sometimes cannot be closed by clicking empty Windows desktop/screen space. `TrayMenuWindow` now explicitly calls `SetForegroundWindow`/`Activate` when shown and uses WPF mouse capture with `PreviewMouseDownOutsideCapturedElement` to hide on outside clicks. This still needs Windows publish/run verification.
- User asked for `Pomodoro Stats` to include stats for today, week, month, and year. Added `PomodoroStatsHistory` daily entries to `WidgetSettings`, migrated current-day legacy fields into history during normalization, added `PomodoroStatsCalculator`, and updated `PomodoroStatsWindow` to show four period cards. The week starts on Monday; month/year are calendar periods. Stats still save only on focus completion or normal settings saves, not periodic/background writes.
- User then asked to remove the `Cycle` block in `Pomodoro Stats` and replace it with a visual activity section. The first year heatmap was too small, so it was changed to a hybrid: a larger recent-days heatmap with Mon/Wed/Fri labels and per-day tooltips plus a 12-month focus-minutes bar chart. After feedback that Activity looked cramped, the dark panel was replaced with a light panel matching the stats window, daily cells were increased to 20px, spacing was increased, green intensity colors were softened, and the range was set to last 120 days with subtle month gaps. The heatmap now builds each month as its own segment, so month labels start at the actual visible days of that month instead of a shared cross-month week column. The current month is drawn through its calendar end with future days as empty cells. Both charts use `PomodoroStatsHistory`.
- User asked to add a reset statistics button to `Pomodoro Stats`. Added `Reset stats` with a confirmation dialog; accepting clears `PomodoroDailyCount`, `PomodoroDailyFocusMinutes`, and `PomodoroStatsHistory`, sets the daily stats date to today, saves settings once, refreshes the stats window, and updates the widget daily stats display.
- User later asked for `Reset stats` to show a choice: all stats, today, or week. The button now opens a small menu with `All stats`, `Today`, and `This week`; each option then asks for confirmation. The model supports scoped reset through `PomodoroStatsResetScope`; current week starts on Monday and clears through the current day.
- User asked to show Pomodoro counts above the bars in `Monthly focus`. The monthly chart now aggregates both `Count` and `FocusMinutes`, renders the count above each monthly bar, and keeps focus minutes as the bar height/color basis.
- User asked to add forward/back buttons near `Last 120 days` in the Pomodoro Stats Activity calendar, then noted the earlier idea of a quick return button. Added previous/next and `Today` buttons in the Activity header; previous/next scroll only the heatmap by 30 days, `Today` returns to the latest 120-day range. The latest window stays labeled `Last 120 days`, older windows show explicit date ranges, forward/Today are disabled at the latest range, and back can show empty older ranges. The navigation state is in-memory only for the open stats window.
- User considered using Space to start/pause Pomodoro when the cursor is over the widget. We decided focus-based behavior is safer than hover/global input. Added `Focusable=True` and `PreviewKeyDown` on `MainWindow`; left-click focuses the widget, and `Space` toggles Pomodoro start/pause only while Pomodoro display is visible and the mouse is still over the widget.
- User asked to add Settings controls for changing break duration after a configured number of Pomodoro sessions. Added `PomodoroLongBreakInterval` and `PomodoroLongBreakMinutes` to `WidgetSettings` with defaults `4` and `15`, normalization, clone/test coverage, Settings Pomodoro-tab sliders, and runtime break selection so every configured Nth completed focus starts the long break.
- User agreed that `Current Timer` in `Pomodoro Stats` is not very important because it duplicates the main widget timer. Removed that block from the stats dialog and dropped the now-unused timer field updates from `PomodoroStatsWindow`.
- User reported that the widget often drifts slightly left from its fixed position, then clarified that switching Pomodoro/clock while `Lock position` is enabled anchored differently than unlocked switching. The attempted generic `SizeChanged` right-edge preservation was too broad and conflicted with existing explicit right-edge restore. Final fix: while locked, `SizeChanged` does not schedule keep-on-screen movement, but explicit Clock/Pomodoro display transitions still preserve the right edge exactly like unlocked switching.
- User reported that Pomodoro timer width changes as digits change, for example around one minute. `TimeText` now uses tabular numerals and a calculated `MinWidth` based on clock font size, seconds visibility, and configured Pomodoro durations, so `FitToContent` does not resize the widget every tick.
- User reported that closing the widget while Pomodoro is visible makes the next launch open farther left than before. `SaveSettings` now accepts `updatePosition`; app closing, focus-completion stats saves, and stats reset use `updatePosition: false`, so temporary Pomodoro-mode geometry does not overwrite saved `Left`/`Top`.
- User reported that the bottom date was too far from the clock and the clock felt too wide. `DateText` top margin was changed from `-2` to `-8`, and `GetStableTimeTextMinWidth()` factors were reduced from `2.85/3.55/4.15` to `2.65/3.25/3.9` to tighten the layout while preserving stable width.

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
