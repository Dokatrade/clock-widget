namespace ClockWidget.Tests;

public sealed class DisplayTickSchedulerTests
{
    [Theory]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    public void GetNextInterval_WhenSecondPrecisionIsNeeded_ReturnsIntervalUntilNextSecond(
        bool showSeconds,
        bool isPomodoroDisplayVisible,
        bool isPomodoroRunning)
    {
        var state = new DisplayTickState(showSeconds, isPomodoroDisplayVisible, isPomodoroRunning);
        var now = new DateTime(2026, 6, 10, 12, 30, 15, 250);

        var interval = DisplayTickScheduler.GetNextInterval(state, now);

        Assert.Equal(TimeSpan.FromMilliseconds(750), interval);
    }

    [Fact]
    public void GetNextInterval_WhenSecondsAreHiddenAndPomodoroIsIdle_ReturnsIntervalUntilNextMinute()
    {
        var state = new DisplayTickState(
            ShowSeconds: false,
            IsPomodoroDisplayVisible: false,
            IsPomodoroRunning: false);
        var now = new DateTime(2026, 6, 10, 12, 30, 15, 250);

        var interval = DisplayTickScheduler.GetNextInterval(state, now);

        Assert.Equal(TimeSpan.FromMilliseconds(44750), interval);
    }

    [Fact]
    public void GetNextInterval_ClampsVerySmallIntervalToMinimum()
    {
        var state = new DisplayTickState(
            ShowSeconds: true,
            IsPomodoroDisplayVisible: false,
            IsPomodoroRunning: false);
        var now = new DateTime(2026, 6, 10, 12, 30, 15, 950);

        var interval = DisplayTickScheduler.GetNextInterval(state, now);

        Assert.Equal(TimeSpan.FromMilliseconds(200), interval);
    }

    [Fact]
    public void GetNextInterval_ClampsMinuteIntervalToMaximum()
    {
        var state = new DisplayTickState(
            ShowSeconds: false,
            IsPomodoroDisplayVisible: false,
            IsPomodoroRunning: false);
        var now = new DateTime(2026, 6, 10, 12, 30, 0, 0);

        var interval = DisplayTickScheduler.GetNextInterval(state, now);

        Assert.Equal(TimeSpan.FromMinutes(1), interval);
    }
}
