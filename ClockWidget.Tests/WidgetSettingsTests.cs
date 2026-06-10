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
            PomodoroBreakMinutes = 500
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
        Assert.False(preset.ShowDate);
    }
}
