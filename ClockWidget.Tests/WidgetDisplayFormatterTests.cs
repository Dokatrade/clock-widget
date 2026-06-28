using MediaColor = System.Windows.Media.Color;

namespace ClockWidget.Tests;

public sealed class WidgetDisplayFormatterTests
{
    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(65, "01:05")]
    [InlineData(3661, "1:01:01")]
    public void FormatDuration_FormatsMinutesAndHours(int totalSeconds, string expected)
    {
        var text = WidgetDisplayFormatter.FormatDuration(TimeSpan.FromSeconds(totalSeconds));

        Assert.Equal(expected, text);
    }

    [Fact]
    public void FormatDuration_RoundsPartialSecondsUp()
    {
        var text = WidgetDisplayFormatter.FormatDuration(TimeSpan.FromSeconds(1.2));

        Assert.Equal("00:02", text);
    }

    [Fact]
    public void BuildClockDisplay_WhenDateIsHidden_FormatsTimeAndHidesDate()
    {
        var formatter = new WidgetDisplayFormatter();
        var settings = new WidgetSettings
        {
            ShowSeconds = false,
            ShowDate = false
        };
        var pomodoro = new PomodoroController(new ManualClock());
        pomodoro.Reset(TimeSpan.FromMinutes(25));

        var display = formatter.BuildClockDisplay(
            new DateTime(2026, 6, 10, 9, 8, 7),
            settings,
            pomodoro,
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5));

        Assert.Equal("09:08", display.TimeText);
        Assert.Equal("", display.DateText);
        Assert.False(display.ShowDate);
        Assert.False(display.Progress.IsVisible);
    }

    [Fact]
    public void BuildClockDisplay_WhenSideDateIsEnabled_FormatsDayAndMonth()
    {
        var formatter = new WidgetDisplayFormatter();
        var settings = new WidgetSettings
        {
            ShowSideDate = true
        };
        var pomodoro = new PomodoroController(new ManualClock());
        pomodoro.Reset(TimeSpan.FromMinutes(25));

        var display = formatter.BuildClockDisplay(
            new DateTime(2026, 6, 28, 9, 8, 7),
            settings,
            pomodoro,
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5));

        Assert.True(display.ShowSideDate);
        Assert.Equal("28", display.SideDateDayText);
        Assert.Equal("06", display.SideDateMonthText);
    }

    [Fact]
    public void BuildClockDisplay_WhenPomodoroRuns_ShowsProgress()
    {
        var formatter = new WidgetDisplayFormatter();
        var clock = new ManualClock();
        var pomodoro = new PomodoroController(clock);
        pomodoro.Reset(TimeSpan.FromMinutes(25));
        pomodoro.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(5));
        pomodoro.Update(TimeSpan.FromMinutes(5), autoStartBreak: true);

        var display = formatter.BuildClockDisplay(
            clock.Now,
            new WidgetSettings { PomodoroEnabled = true },
            pomodoro,
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5));

        Assert.True(display.Progress.IsVisible);
        Assert.InRange(display.Progress.Ratio, 0.199999, 0.200001);
    }

    [Fact]
    public void BuildPomodoroDisplay_ForBreak_UsesBreakTextColorAndGreenProgress()
    {
        var formatter = new WidgetDisplayFormatter();
        var clock = new ManualClock();
        var pomodoro = new PomodoroController(clock);
        pomodoro.Reset(TimeSpan.FromMinutes(25));
        pomodoro.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(25));
        pomodoro.Update(TimeSpan.FromMinutes(5), autoStartBreak: false);

        var display = formatter.BuildPomodoroDisplay(
            pomodoro,
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5));

        Assert.Equal("05:00", display.TimeText);
        Assert.True(display.UseBreakTextColor);
        Assert.Equal("▶", display.StartPauseText);
        Assert.True(display.Progress.IsVisible);
        Assert.Equal(0, display.Progress.Ratio);
        Assert.Equal(MediaColor.FromRgb(34, 197, 94), display.Progress.Color);
    }

    [Fact]
    public void GetPomodoroProgressColor_ForFocusInterpolatesThroughBlue()
    {
        var color = WidgetDisplayFormatter.GetPomodoroProgressColor(0.5, PomodoroPhase.Focus);

        Assert.Equal(MediaColor.FromRgb(34, 211, 238), color);
    }

    private sealed class ManualClock : IClock
    {
        public DateTime Now { get; private set; } = new(2026, 6, 10, 12, 0, 0);

        public void Advance(TimeSpan duration)
        {
            Now += duration;
        }
    }
}
