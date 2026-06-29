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

    public static IReadOnlyList<double> BuildHourlyFocusMinutes(
        WidgetSettings settings,
        DateTime today,
        PomodoroRhythmRange range = PomodoroRhythmRange.AllTime,
        PomodoroRhythmMode mode = PomodoroRhythmMode.Total)
    {
        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        var hours = new double[24];
        var todayDate = today.Date;
        var sessions = normalizedSettings.PomodoroFocusSessions
            .Select(entry => new PomodoroFocusSession(
                PomodoroStatsCalculator.TryParseStatsTimestamp(entry.CompletedAt, out var completedAt)
                    ? completedAt
                    : (DateTime?)null,
                Math.Max(0, entry.FocusMinutes)))
            .Where(session => session.CompletedAt is not null && session.FocusMinutes > 0)
            .ToList();

        var rangeStart = GetRhythmRangeStart(sessions, todayDate, range);
        var rangeEnd = todayDate;
        var activeDates = new HashSet<DateTime>();

        foreach (var session in sessions)
        {
            var completedAt = session.CompletedAt!.Value;
            if (completedAt.Date < rangeStart || completedAt.Date > rangeEnd)
            {
                continue;
            }

            for (var minute = 0; minute < session.FocusMinutes; minute++)
            {
                var minuteStart = completedAt.AddMinutes(-session.FocusMinutes + minute);
                if (minuteStart.Date < rangeStart || minuteStart.Date > rangeEnd)
                {
                    continue;
                }

                hours[minuteStart.Hour]++;
                activeDates.Add(minuteStart.Date);
            }
        }

        if (mode == PomodoroRhythmMode.Average)
        {
            var dayCount = Math.Max(1, activeDates.Count);
            for (var hour = 0; hour < hours.Length; hour++)
            {
                hours[hour] /= dayCount;
            }
        }

        return hours;
    }

    public static IReadOnlyList<double> BuildHourlyPomodoroCounts(
        WidgetSettings settings,
        DateTime today,
        PomodoroRhythmRange range = PomodoroRhythmRange.AllTime,
        PomodoroRhythmMode mode = PomodoroRhythmMode.Total)
    {
        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        var hours = new double[24];
        var todayDate = today.Date;
        var sessions = normalizedSettings.PomodoroFocusSessions
            .Select(entry => new PomodoroFocusSession(
                PomodoroStatsCalculator.TryParseStatsTimestamp(entry.CompletedAt, out var completedAt)
                    ? completedAt
                    : (DateTime?)null,
                Math.Max(0, entry.FocusMinutes)))
            .Where(session => session.CompletedAt is not null && session.FocusMinutes > 0)
            .ToList();

        var rangeStart = GetRhythmRangeStart(sessions, todayDate, range);
        var rangeEnd = todayDate;
        var activeDates = new HashSet<DateTime>();

        foreach (var session in sessions)
        {
            var completedAt = session.CompletedAt!.Value;
            if (completedAt.Date < rangeStart || completedAt.Date > rangeEnd)
            {
                continue;
            }

            hours[completedAt.Hour]++;
            activeDates.Add(completedAt.Date);
        }

        if (mode == PomodoroRhythmMode.Average)
        {
            var dayCount = Math.Max(1, activeDates.Count);
            for (var hour = 0; hour < hours.Length; hour++)
            {
                hours[hour] /= dayCount;
            }
        }

        return hours;
    }

    private static DateTime GetRhythmRangeStart(
        IReadOnlyList<PomodoroFocusSession> sessions,
        DateTime today,
        PomodoroRhythmRange range)
    {
        return range switch
        {
            PomodoroRhythmRange.Today => today,
            PomodoroRhythmRange.Week => GetWeekStart(today),
            PomodoroRhythmRange.Month => new DateTime(today.Year, today.Month, 1),
            _ => sessions.Count == 0
                ? today
                : sessions.Min(session => session.CompletedAt!.Value.AddMinutes(-session.FocusMinutes).Date)
        };
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

    internal static bool TryParseStatsTimestamp(string? timestamp, out DateTime parsed)
    {
        return DateTime.TryParseExact(
            timestamp,
            "yyyy-MM-ddTHH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsed);
    }
}

internal sealed record PomodoroStatsSummary(
    PomodoroStatsPeriod Today,
    PomodoroStatsPeriod Week,
    PomodoroStatsPeriod Month,
    PomodoroStatsPeriod Year);

internal sealed record PomodoroStatsPeriod(int Count, int FocusMinutes);

public enum PomodoroRhythmMode
{
    Total,
    Average
}

public enum PomodoroRhythmMetric
{
    Minutes,
    Pomodoros
}

public enum PomodoroRhythmRange
{
    AllTime,
    Month,
    Week,
    Today
}

internal sealed record PomodoroFocusSession(DateTime? CompletedAt, int FocusMinutes);
