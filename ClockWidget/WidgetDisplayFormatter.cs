using System.Globalization;
using MediaColor = System.Windows.Media.Color;

namespace ClockWidget;

internal sealed class WidgetDisplayFormatter
{
    public ClockDisplayModel BuildClockDisplay(
        DateTime now,
        WidgetSettings settings,
        PomodoroController pomodoro,
        TimeSpan focusDuration,
        TimeSpan breakDuration)
    {
        return new ClockDisplayModel(
            TimeText: now.ToString(settings.ShowSeconds ? "HH:mm:ss" : "HH:mm", CultureInfo.CurrentCulture),
            DateText: settings.ShowDate
                ? now.ToString(settings.ShowWeekday ? "dddd, d MMMM yyyy" : "d MMMM yyyy", CultureInfo.CurrentCulture)
                : "",
            ShowDate: settings.ShowDate,
            Progress: settings.PomodoroEnabled && pomodoro.IsRunning
                ? BuildProgress(pomodoro, focusDuration, breakDuration)
                : PomodoroProgressModel.Hidden);
    }

    public PomodoroDisplayModel BuildPomodoroDisplay(
        PomodoroController pomodoro,
        TimeSpan focusDuration,
        TimeSpan breakDuration)
    {
        return new PomodoroDisplayModel(
            TimeText: FormatDuration(pomodoro.Remaining),
            UseBreakTextColor: pomodoro.Phase == PomodoroPhase.Break,
            StartPauseText: pomodoro.IsRunning ? "Ⅱ" : "▶",
            Progress: BuildProgress(pomodoro, focusDuration, breakDuration));
    }

    private static PomodoroProgressModel BuildProgress(
        PomodoroController pomodoro,
        TimeSpan focusDuration,
        TimeSpan breakDuration)
    {
        var duration = pomodoro.Phase == PomodoroPhase.Focus ? focusDuration : breakDuration;
        var durationSeconds = duration.TotalSeconds;
        if (durationSeconds <= 0)
        {
            return PomodoroProgressModel.Visible(0, GetPomodoroProgressColor(0, pomodoro.Phase));
        }

        var remainingSeconds = Math.Clamp(pomodoro.Remaining.TotalSeconds, 0, durationSeconds);
        var ratio = 1 - remainingSeconds / durationSeconds;
        return PomodoroProgressModel.Visible(ratio, GetPomodoroProgressColor(ratio, pomodoro.Phase));
    }

    internal static MediaColor GetPomodoroProgressColor(double progress, PomodoroPhase phase)
    {
        var green = MediaColor.FromRgb(34, 197, 94);
        if (phase == PomodoroPhase.Break)
        {
            return green;
        }

        var clampedProgress = Math.Clamp(progress, 0, 1);
        var blue = MediaColor.FromRgb(34, 211, 238);
        var red = MediaColor.FromRgb(248, 64, 64);

        if (clampedProgress < 0.5)
        {
            return LerpColor(green, blue, clampedProgress * 2);
        }

        return LerpColor(blue, red, (clampedProgress - 0.5) * 2);
    }

    internal static string FormatDuration(TimeSpan duration)
    {
        var totalSeconds = Math.Max(0, (int)Math.Ceiling(duration.TotalSeconds));
        var normalized = TimeSpan.FromSeconds(totalSeconds);
        return normalized.TotalHours >= 1
            ? normalized.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : normalized.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    private static MediaColor LerpColor(MediaColor start, MediaColor end, double amount)
    {
        return MediaColor.FromRgb(
            Lerp(start.R, end.R, amount),
            Lerp(start.G, end.G, amount),
            Lerp(start.B, end.B, amount));
    }

    private static byte Lerp(byte start, byte end, double amount)
    {
        return (byte)Math.Round(start + (end - start) * amount);
    }
}

internal sealed record ClockDisplayModel(
    string TimeText,
    string DateText,
    bool ShowDate,
    PomodoroProgressModel Progress);

internal sealed record PomodoroDisplayModel(
    string TimeText,
    bool UseBreakTextColor,
    string StartPauseText,
    PomodoroProgressModel Progress);

internal sealed record PomodoroProgressModel(
    bool IsVisible,
    double Ratio,
    MediaColor Color)
{
    public static PomodoroProgressModel Hidden { get; } = new(
        IsVisible: false,
        Ratio: 0,
        Color: MediaColor.FromRgb(34, 197, 94));

    public static PomodoroProgressModel Visible(double ratio, MediaColor color)
    {
        return new PomodoroProgressModel(
            IsVisible: true,
            Ratio: Math.Clamp(ratio, 0, 1),
            Color: color);
    }
}
