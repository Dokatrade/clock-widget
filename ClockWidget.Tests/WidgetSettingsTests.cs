namespace ClockWidget.Tests;

public sealed class WidgetSettingsTests
{
    [Fact]
    public void Normalize_ClampsInvalidNumericValues()
    {
        var settings = new WidgetSettings
        {
            Width = double.PositiveInfinity,
            Height = -10,
            ClockFontSize = 1000,
            ClockFontWeight = 777,
            PaddingHorizontal = -1,
            PaddingTop = 99,
            PaddingBottom = double.NaN,
            BackgroundShade = 255,
            BackgroundOpacity = -5,
            DateFontSize = 100,
            PomodoroFocusMinutes = 0,
            PomodoroBreakMinutes = 500,
            PomodoroDailyCount = -1,
            PomodoroDailyFocusMinutes = -20,
            PomodoroStatsHistory =
            [
                new PomodoroStatsEntry { Date = "2026-06-12", Count = -1, FocusMinutes = 25 },
                new PomodoroStatsEntry { Date = "not-a-date", Count = 1, FocusMinutes = 25 }
            ]
        };

        settings.Normalize();

        Assert.Equal(330d, settings.Width);
        Assert.Equal(WidgetSettings.MinHeight, settings.Height);
        Assert.Equal(WidgetSettings.MaxClockFontSize, settings.ClockFontSize);
        Assert.Equal(800, settings.ClockFontWeight);
        Assert.Equal(WidgetSettings.MinPadding, settings.PaddingHorizontal);
        Assert.Equal(WidgetSettings.MaxPadding, settings.PaddingTop);
        Assert.Equal(10d, settings.PaddingBottom);
        Assert.Equal(WidgetSettings.MaxBackgroundShade, settings.BackgroundShade);
        Assert.Equal(WidgetSettings.MinBackgroundOpacity, settings.BackgroundOpacity);
        Assert.Equal(WidgetSettings.MaxDateFontSize, settings.DateFontSize);
        Assert.Equal(WidgetSettings.MinPomodoroFocusMinutes, settings.PomodoroFocusMinutes);
        Assert.Equal(WidgetSettings.MaxPomodoroBreakMinutes, settings.PomodoroBreakMinutes);
        Assert.Equal(0, settings.PomodoroDailyCount);
        Assert.Equal(0, settings.PomodoroDailyFocusMinutes);
        Assert.Single(settings.PomodoroStatsHistory);
        Assert.Equal("2026-06-12", settings.PomodoroStatsHistory[0].Date);
        Assert.Equal(0, settings.PomodoroStatsHistory[0].Count);
        Assert.Equal(25, settings.PomodoroStatsHistory[0].FocusMinutes);
    }

    [Fact]
    public void Normalize_ReplacesInvalidPomodoroSound()
    {
        var settings = new WidgetSettings
        {
            PomodoroSound = (PomodoroSound)999
        };

        settings.Normalize();

        Assert.Equal(PomodoroSound.FreesoundsNotification, settings.PomodoroSound);
    }

    [Fact]
    public void Normalize_RemovesBlankPresetNames_DeduplicatesAndSorts()
    {
        var settings = new WidgetSettings
        {
            Presets =
            [
                new WidgetPreset { Name = " Beta ", ClockFontSize = 44 },
                new WidgetPreset { Name = "alpha", ClockFontSize = 50 },
                new WidgetPreset { Name = "BETA", ClockFontSize = 60 },
                new WidgetPreset { Name = " " }
            ]
        };

        settings.Normalize();

        Assert.Collection(
            settings.Presets,
            preset =>
            {
                Assert.Equal("alpha", preset.Name);
                Assert.Equal(50d, preset.ClockFontSize);
            },
            preset =>
            {
                Assert.Equal("Beta", preset.Name);
                Assert.Equal(60d, preset.ClockFontSize);
            });
    }

    [Fact]
    public void CreatePreset_StoresCurrentVisualSettingsOnly()
    {
        var settings = new WidgetSettings
        {
            Width = 420,
            Height = 140,
            ShowSeconds = false,
            ShowSideDate = true,
            ShowDate = false,
            LockPosition = true,
            StartWithWindows = true,
            PomodoroEnabled = false
        };

        var preset = settings.CreatePreset(" compact ");

        Assert.Equal("compact", preset.Name);
        Assert.Equal(420d, preset.Width);
        Assert.Equal(140d, preset.Height);
        Assert.False(preset.ShowSeconds);
        Assert.True(preset.ShowSideDate);
        Assert.False(preset.ShowDate);
    }

    [Fact]
    public void ApplyPreset_UpdatesVisualSettingsOnly()
    {
        var settings = new WidgetSettings
        {
            AlwaysOnTop = false,
            LockPosition = true,
            SnapToScreenEdges = false,
            StartWithWindows = true,
            PomodoroEnabled = false,
            PomodoroFocusMinutes = 45,
            PomodoroBreakMinutes = 12,
            PomodoroAutoStartBreak = false,
            PomodoroReturnToClockAfterBreak = false,
            PomodoroPlaySound = false,
            PomodoroSound = PomodoroSound.Harp
        };
        var preset = new WidgetPreset
        {
            Name = "Minimal",
            ClockFontSize = 44,
            BackgroundOpacity = 0.5,
            ShowSeconds = false,
            ShowSideDate = true,
            ShowDate = false
        };

        settings.ApplyPreset(preset);

        Assert.Equal(44d, settings.ClockFontSize);
        Assert.Equal(0.5d, settings.BackgroundOpacity);
        Assert.False(settings.ShowSeconds);
        Assert.True(settings.ShowSideDate);
        Assert.False(settings.ShowDate);
        Assert.False(settings.AlwaysOnTop);
        Assert.True(settings.LockPosition);
        Assert.False(settings.SnapToScreenEdges);
        Assert.True(settings.StartWithWindows);
        Assert.False(settings.PomodoroEnabled);
        Assert.Equal(45, settings.PomodoroFocusMinutes);
        Assert.Equal(12, settings.PomodoroBreakMinutes);
        Assert.False(settings.PomodoroAutoStartBreak);
        Assert.False(settings.PomodoroReturnToClockAfterBreak);
        Assert.False(settings.PomodoroPlaySound);
        Assert.Equal(PomodoroSound.Harp, settings.PomodoroSound);
    }

    [Fact]
    public void Clone_PreservesPomodoroDailyStats()
    {
        var settings = new WidgetSettings
        {
            ShowPomodoroDailyStats = true,
            PomodoroDailyStatsDate = "2026-06-12",
            PomodoroDailyCount = 4,
            PomodoroDailyFocusMinutes = 100,
            PomodoroStatsHistory =
            [
                new PomodoroStatsEntry { Date = "2026-06-11", Count = 2, FocusMinutes = 50 }
            ]
        };

        var clone = settings.Clone();

        Assert.True(clone.ShowPomodoroDailyStats);
        Assert.Equal("2026-06-12", clone.PomodoroDailyStatsDate);
        Assert.Equal(4, clone.PomodoroDailyCount);
        Assert.Equal(100, clone.PomodoroDailyFocusMinutes);
        Assert.Single(clone.PomodoroStatsHistory);
        Assert.Equal("2026-06-11", clone.PomodoroStatsHistory[0].Date);
        Assert.NotSame(settings.PomodoroStatsHistory[0], clone.PomodoroStatsHistory[0]);
    }

    [Fact]
    public void Normalize_UsesDailyStatsAsAuthoritativeHistoryEntry()
    {
        var settings = new WidgetSettings
        {
            PomodoroDailyStatsDate = "2026-06-12",
            PomodoroDailyCount = 3,
            PomodoroDailyFocusMinutes = 75,
            PomodoroStatsHistory =
            [
                new PomodoroStatsEntry { Date = "2026-06-12", Count = 1, FocusMinutes = 25 }
            ]
        };

        settings.Normalize();

        Assert.Single(settings.PomodoroStatsHistory);
        Assert.Equal(3, settings.PomodoroStatsHistory[0].Count);
        Assert.Equal(75, settings.PomodoroStatsHistory[0].FocusMinutes);
    }

    [Fact]
    public void ResetPomodoroStats_ClearsDailyStatsAndHistory()
    {
        var settings = new WidgetSettings
        {
            PomodoroDailyStatsDate = "2026-06-12",
            PomodoroDailyCount = 3,
            PomodoroDailyFocusMinutes = 75,
            PomodoroStatsHistory =
            [
                new PomodoroStatsEntry { Date = "2026-06-12", Count = 3, FocusMinutes = 75 },
                new PomodoroStatsEntry { Date = "2026-06-13", Count = 1, FocusMinutes = 25 }
            ]
        };

        settings.ResetPomodoroStats(new DateTime(2026, 6, 14));

        Assert.Equal("2026-06-14", settings.PomodoroDailyStatsDate);
        Assert.Equal(0, settings.PomodoroDailyCount);
        Assert.Equal(0, settings.PomodoroDailyFocusMinutes);
        Assert.Empty(settings.PomodoroStatsHistory);
    }

    [Fact]
    public void CreateBuiltInPresets_ReturnsFreshNormalizedVisualPresets()
    {
        var presets = WidgetSettings.CreateBuiltInPresets();

        Assert.Collection(
            presets,
            preset => Assert.Equal("Compact", preset.Name),
            preset => Assert.Equal("Large", preset.Name),
            preset => Assert.Equal("Minimal", preset.Name),
            preset => Assert.Equal("Pomodoro", preset.Name));

        foreach (var preset in presets)
        {
            Assert.InRange(preset.Width, WidgetSettings.MinWidth, WidgetSettings.MaxWidth);
            Assert.InRange(preset.Height, WidgetSettings.MinHeight, WidgetSettings.MaxHeight);
            Assert.InRange(preset.ClockFontSize, WidgetSettings.MinClockFontSize, WidgetSettings.MaxClockFontSize);
            Assert.InRange(preset.BackgroundOpacity, WidgetSettings.MinBackgroundOpacity, WidgetSettings.MaxBackgroundOpacity);
        }

        presets[0].Name = "Changed";

        Assert.Equal("Compact", WidgetSettings.CreateBuiltInPresets()[0].Name);
    }
}
