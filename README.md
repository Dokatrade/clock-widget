# ClockWidget

A small Windows 11 desktop clock widget built with WPF.

## Features

- Borderless transparent clock window.
- Always-on-top mode enabled by default.
- Drag the widget with the left mouse button.
- Right-click menu for always-on-top, seconds, position lock, settings, and exit.
- Settings window for background shade, opacity, border, weekday/date, precise padding, size, and clock font weight.
- Optional Pomodoro mode with configurable focus/break duration, sound, notification, and on-widget controls.
- `Fit window to content` mode makes padding values match the visible distance to the widget edge.
- Named presets can save and restore visual settings plus seconds/date visibility, like `small` or `big`.
- Defaults button for restoring visual settings.
- Position and preferences are saved in `%APPDATA%\ClockWidget\settings.json`.
- No periodic disk writes; settings are saved only after user changes or after pressing Apply/OK in settings.

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

## Usage

- Drag to place the clock anywhere.
- Use the right-click menu to toggle always-on-top.
- Ctrl-click the widget to switch clock/Pomodoro without opening the menu.
- Use the Pomodoro buttons on the widget to start, pause, or reset the timer.
- Right-click for settings.
- Enable `Lock position` after placing it.
