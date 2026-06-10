namespace ClockWidget;

internal interface IClock
{
    DateTime Now { get; }
}

internal sealed class SystemClock : IClock
{
    public static SystemClock Instance { get; } = new();

    private SystemClock()
    {
    }

    public DateTime Now => DateTime.Now;
}
