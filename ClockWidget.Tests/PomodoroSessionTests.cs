namespace ClockWidget.Tests;

public sealed class PomodoroSessionTests
{
    [Fact]
    public void ToggleDisplay_WhenPomodoroDisabled_DoesNotChangeDisplayMode()
    {
        var session = new PomodoroSession();
        var settings = new WidgetSettings { PomodoroEnabled = false };

        var changed = session.ToggleDisplay(
            settings,
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5));

        Assert.False(changed);
        Assert.Equal(WidgetDisplayMode.Clock, session.DisplayMode);
    }

    [Fact]
    public void ToggleStartPause_WhenPomodoroEnabled_ShowsPomodoroAndStartsTimer()
    {
        var session = new PomodoroSession(new PomodoroController(new ManualClock()));
        var settings = new WidgetSettings { PomodoroEnabled = true };

        var changed = session.ToggleStartPause(
            settings,
            TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(5));

        Assert.True(changed);
        Assert.Equal(WidgetDisplayMode.Pomodoro, session.DisplayMode);
        Assert.True(session.Controller.IsRunning);
    }

    [Fact]
    public void CompletePhase_WhenBreakCompletesAndReturnToClock_ResetsToClock()
    {
        var clock = new ManualClock();
        var session = new PomodoroSession(new PomodoroController(clock));
        var settings = new WidgetSettings { PomodoroEnabled = true };
        var focusDuration = TimeSpan.FromMinutes(25);
        var breakDuration = TimeSpan.FromMinutes(5);

        session.ToggleStartPause(settings, focusDuration, breakDuration);
        clock.Advance(focusDuration);
        var focusCompletion = session.Update(breakDuration, autoStartBreak: true);
        session.CompletePhase(focusCompletion, focusDuration, returnToClockAfterBreak: true);
        clock.Advance(breakDuration);

        var breakCompletion = session.Update(breakDuration, autoStartBreak: true);
        session.CompletePhase(breakCompletion, focusDuration, returnToClockAfterBreak: true);

        Assert.Equal(WidgetDisplayMode.Clock, session.DisplayMode);
        Assert.Equal(PomodoroPhase.Focus, session.Controller.Phase);
        Assert.Equal(focusDuration, session.Controller.Remaining);
        Assert.False(session.Controller.IsRunning);
    }

    [Fact]
    public void GetTickState_WhenPomodoroIsRunningInClockMode_RequiresSecondPrecision()
    {
        var session = new PomodoroSession(new PomodoroController(new ManualClock()));
        var settings = new WidgetSettings
        {
            PomodoroEnabled = true,
            ShowSeconds = false
        };
        session.ToggleStartPause(settings, TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        session.ToggleDisplay(settings, TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));

        var tickState = session.GetTickState(settings);

        Assert.False(tickState.ShowSeconds);
        Assert.False(tickState.IsPomodoroDisplayVisible);
        Assert.True(tickState.IsPomodoroRunning);
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
