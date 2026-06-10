using System.Windows.Threading;

namespace ClockWidget;

internal sealed class DisplayTickScheduler
{
    private readonly DispatcherTimer _timer = new();
    private readonly Action _tick;

    public DisplayTickScheduler(Action tick)
    {
        _tick = tick;
        _timer.Tick += (_, _) => _tick();
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void ScheduleNext(DisplayTickState state)
    {
        _timer.Interval = GetNextInterval(state, DateTime.Now);
    }

    internal static TimeSpan GetNextInterval(DisplayTickState state, DateTime now)
    {
        if (state.IsPomodoroDisplayVisible || state.IsPomodoroRunning)
        {
            return GetIntervalUntilNextSecond(now);
        }

        return state.ShowSeconds
            ? GetIntervalUntilNextSecond(now)
            : GetIntervalUntilNextMinute(now);
    }

    private static TimeSpan GetIntervalUntilNextSecond(DateTime now)
    {
        var nextSecond = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            now.Second,
            now.Kind).AddSeconds(1);

        return ClampTimerInterval(nextSecond - now);
    }

    private static TimeSpan GetIntervalUntilNextMinute(DateTime now)
    {
        var nextMinute = new DateTime(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            0,
            now.Kind).AddMinutes(1);

        return ClampTimerInterval(nextMinute - now);
    }

    private static TimeSpan ClampTimerInterval(TimeSpan interval)
    {
        var minimum = TimeSpan.FromMilliseconds(200);
        var maximum = TimeSpan.FromMinutes(1);
        return interval < minimum
            ? minimum
            : interval > maximum
                ? maximum
                : interval;
    }
}

internal sealed record DisplayTickState(
    bool ShowSeconds,
    bool IsPomodoroDisplayVisible,
    bool IsPomodoroRunning);
