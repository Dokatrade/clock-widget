namespace ClockWidget.Tests;

public sealed class PomodoroControllerTests
{
    [Fact]
    public void Reset_SetsFocusIdleWithFullDuration()
    {
        var clock = new ManualClock();
        var controller = new PomodoroController(clock);

        controller.Reset(TimeSpan.FromMinutes(25));

        Assert.Equal(PomodoroPhase.Focus, controller.Phase);
        Assert.Equal(TimeSpan.FromMinutes(25), controller.Remaining);
        Assert.False(controller.IsRunning);
    }

    [Fact]
    public void ToggleStartPause_StartsThenPausesWithUpdatedRemaining()
    {
        var clock = new ManualClock();
        var controller = new PomodoroController(clock);
        controller.Reset(TimeSpan.FromMinutes(25));

        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(3));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));

        Assert.False(controller.IsRunning);
        Assert.Equal(TimeSpan.FromMinutes(22), controller.Remaining);
    }

    [Fact]
    public void ToggleStartPause_ResumesFromPausedRemaining()
    {
        var clock = new ManualClock();
        var controller = new PomodoroController(clock);
        controller.Reset(TimeSpan.FromMinutes(25));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(3));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(10));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(2));
        var completion = controller.Update(TimeSpan.FromMinutes(5), autoStartBreak: true);

        Assert.Equal(PomodoroPhaseCompletion.None, completion);
        Assert.True(controller.IsRunning);
        Assert.Equal(TimeSpan.FromMinutes(20), controller.Remaining);
    }

    [Fact]
    public void Update_WhenFocusCompletesWithAutoStart_StartsBreak()
    {
        var clock = new ManualClock();
        var controller = new PomodoroController(clock);
        controller.Reset(TimeSpan.FromMinutes(25));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(25));
        var completion = controller.Update(TimeSpan.FromMinutes(5), autoStartBreak: true);

        Assert.Equal(PomodoroPhaseCompletion.FocusCompleted, completion);
        Assert.Equal(PomodoroPhase.Break, controller.Phase);
        Assert.Equal(TimeSpan.FromMinutes(5), controller.Remaining);
        Assert.True(controller.IsRunning);
    }

    [Fact]
    public void Update_WhenFocusCompletesWithoutAutoStart_PausesAtBreak()
    {
        var clock = new ManualClock();
        var controller = new PomodoroController(clock);
        controller.Reset(TimeSpan.FromMinutes(25));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));

        clock.Advance(TimeSpan.FromMinutes(25));
        var completion = controller.Update(TimeSpan.FromMinutes(5), autoStartBreak: false);

        Assert.Equal(PomodoroPhaseCompletion.FocusCompleted, completion);
        Assert.Equal(PomodoroPhase.Break, controller.Phase);
        Assert.Equal(TimeSpan.FromMinutes(5), controller.Remaining);
        Assert.False(controller.IsRunning);
    }

    [Fact]
    public void Update_WhenBreakCompletes_StopsTimer()
    {
        var clock = new ManualClock();
        var controller = new PomodoroController(clock);
        controller.Reset(TimeSpan.FromMinutes(25));
        controller.ToggleStartPause(TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(5));
        clock.Advance(TimeSpan.FromMinutes(25));
        controller.Update(TimeSpan.FromMinutes(5), autoStartBreak: true);

        clock.Advance(TimeSpan.FromMinutes(5));
        var completion = controller.Update(TimeSpan.FromMinutes(5), autoStartBreak: true);

        Assert.Equal(PomodoroPhaseCompletion.BreakCompleted, completion);
        Assert.Equal(PomodoroPhase.Break, controller.Phase);
        Assert.Equal(TimeSpan.Zero, controller.Remaining);
        Assert.False(controller.IsRunning);
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
