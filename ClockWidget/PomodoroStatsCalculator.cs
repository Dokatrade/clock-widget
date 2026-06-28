using System.Globalization;

namespace ClockWidget;

internal static class PomodoroStatsCalculator
{
    public static PomodoroStatsSummary BuildSummary(WidgetSettings settings, DateTime today)
    {
        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        var todayDate = today.Date;

        return new PomodoroStatsSummary(
            Today: SumPeriod(normalizedSettings, todayDate, todayDate),
            Week: SumPeriod(normalizedSettings, GetWeekStart(todayDate), todayDate),
            Month: SumPeriod(normalizedSettings, new DateTime(todayDate.Year, todayDate.Month, 1), todayDate),
            Year: SumPeriod(normalizedSettings, new DateTime(todayDate.Year, 1, 1), todayDate));
    }

    private static PomodoroStatsPeriod SumPeriod(WidgetSettings settings, DateTime start, DateTime end)
    {
        var count = 0;
        var focusMinutes = 0;

        foreach (var entry in settings.PomodoroStatsHistory)
        {
            if (!DateTime.TryParseExact(
                entry.Date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var entryDate))
            {
                continue;
            }

            entryDate = entryDate.Date;
            if (entryDate < start || entryDate > end)
            {
                continue;
            }

            count += Math.Max(0, entry.Count);
            focusMinutes += Math.Max(0, entry.FocusMinutes);
        }

        return new PomodoroStatsPeriod(count, focusMinutes);
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysFromMonday);
    }
}

internal sealed record PomodoroStatsSummary(
    PomodoroStatsPeriod Today,
    PomodoroStatsPeriod Week,
    PomodoroStatsPeriod Month,
    PomodoroStatsPeriod Year);

internal sealed record PomodoroStatsPeriod(int Count, int FocusMinutes);
