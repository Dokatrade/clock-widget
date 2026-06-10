# ClockWidget

A small Windows 11 desktop clock widget built with WPF.

## Features

- Borderless transparent clock window.
- Always-on-top mode enabled by default.
- Keeps the widget inside the visible desktop area after moves and size changes.
- Optional snap-to-edge placement after dragging near screen edges.
- Prevents duplicate widget instances.
- Tray icon menu for show/hide, settings, lock, always-on-top, Pomodoro controls, and exit.
- Reset position command restores the widget to the top-right of the current screen work area.
- Show/hide from the tray is temporary and does not write settings to disk.
- Adaptive timer reduces background UI updates when seconds and Pomodoro are not active.
- Drag the widget with the left mouse button.
- Right-click menu for always-on-top, seconds, position lock, settings, and exit.
- Settings window for background shade, opacity, border, weekday/date, precise padding, size, and clock font weight.
- Optional Pomodoro mode with configurable focus/break duration, sound, and on-widget controls.
- `Fit window to content` mode makes padding values match the visible distance to the widget edge.
- Named presets can save and restore visual settings plus seconds/date visibility, like `small` or `big`.
- Defaults button for restoring visual settings.
- Position and preferences are saved in `%APPDATA%\ClockWidget\settings.json`.
- No periodic disk writes; settings are saved only after user changes or after pressing Apply/OK in settings.
- Settings JSON is normalized on load/save to recover from invalid manual edits.

## Requirements

- Windows 11 or Windows 10.
- .NET 8 SDK for building.

## Build

```powershell
dotnet build .\ClockWidget.sln -c Release
```

## Run

```powershell
dotnet run --project .\ClockWidget\ClockWidget.csproj -c Release
```

## Publish

```powershell
.\publish.ps1
```

The published executable will be in:

```text
dist\ClockWidget.exe
```

## Tests

```powershell
dotnet test .\ClockWidget.sln
```

## Usage

- Drag to place the clock anywhere.
- Use the right-click menu to toggle always-on-top.
- Use `Reset position` if the widget should return to the top-right corner.
- Ctrl-click the widget to switch clock/Pomodoro without opening the menu.
- Use the Pomodoro buttons on the widget to start, pause, or reset the timer.
- Right-click for settings.
- Enable `Lock position` after placing it.
