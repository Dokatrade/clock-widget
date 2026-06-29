namespace ClockWidget.Tests;

public sealed class PomodoroStatsCalculatorTests
{
    [Fact]
    public void BuildSummary_ReturnsTodayWeekMonthAndYearTotals()
    {
        var settings = new WidgetSettings
        {
            PomodoroStatsHistory =
            [
                new PomodoroStatsEntry { Date = "2025-12-31", Count = 100, FocusMinutes = 2500 },
                new PomodoroStatsEntry { Date = "2026-05-31", Count = 8, FocusMinutes = 200 },
                new PomodoroStatsEntry { Date = "2026-06-21", Count = 5, FocusMinutes = 125 },
                new PomodoroStatsEntry { Date = "2026-06-22", Count = 1, FocusMinutes = 25 },
                new PomodoroStatsEntry { Date = "2026-06-27", Count = 2, FocusMinutes = 50 },
                new PomodoroStatsEntry { Date = "2026-06-28", Count = 3, FocusMinutes = 75 }
            ]
        };

        var summary = PomodoroStatsCalculator.BuildSummary(settings, new DateTime(2026, 6, 28, 14, 30, 0));

        Assert.Equal(new PomodoroStatsPeriod(3, 75), summary.Today);
        Assert.Equal(new PomodoroStatsPeriod(6, 150), summary.Week);
        Assert.Equal(new PomodoroStatsPeriod(11, 275), summary.Month);
        Assert.Equal(new PomodoroStatsPeriod(19, 475), summary.Year);
    }

    [Fact]
    public void BuildSummary_IncludesLegacyDailyStatsWithoutHistory()
    {
        var settings = new WidgetSettings
        {
            PomodoroDailyStatsDate = "2026-06-28",
            PomodoroDailyCount = 4,
            PomodoroDailyFocusMinutes = 100
        };

        var summary = PomodoroStatsCalculator.BuildSummary(settings, new DateTime(2026, 6, 28));

        Assert.Equal(new PomodoroStatsPeriod(4, 100), summary.Today);
        Assert.Equal(new PomodoroStatsPeriod(4, 100), summary.Week);
        Assert.Equal(new PomodoroStatsPeriod(4, 100), summary.Month);
        Assert.Equal(new PomodoroStatsPeriod(4, 100), summary.Year);
    }

    [Fact]
    public void BuildHourlyFocusMinutes_DistributesSessionsAcrossHours()
    {
        var settings = new WidgetSettings
        {
            PomodoroFocusSessions =
            [
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T10:10:00", FocusMinutes = 25 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T11:05:00", FocusMinutes = 10 }
            ]
        };

        var hours = PomodoroStatsCalculator.BuildHourlyFocusMinutes(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0));

        Assert.Equal(15.0, hours[9]);
        Assert.Equal(25.0, hours[10]);
        Assert.Equal(5.0, hours[11]);
        Assert.Equal(45.0, hours.Sum());
    }

    [Fact]
    public void BuildHourlyFocusMinutes_FiltersBySelectedRange()
    {
        var settings = new WidgetSettings
        {
            PomodoroFocusSessions =
            [
                new PomodoroFocusSessionEntry { CompletedAt = "2026-05-28T10:30:00", FocusMinutes = 25 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-21T10:30:00", FocusMinutes = 25 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T10:30:00", FocusMinutes = 25 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T11:30:00", FocusMinutes = 25 }
            ]
        };

        var monthHours = PomodoroStatsCalculator.BuildHourlyFocusMinutes(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0),
            PomodoroRhythmRange.Month);
        var weekHours = PomodoroStatsCalculator.BuildHourlyFocusMinutes(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0),
            PomodoroRhythmRange.Week);
        var todayHours = PomodoroStatsCalculator.BuildHourlyFocusMinutes(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0),
            PomodoroRhythmRange.Today);

        Assert.Equal(50.0, monthHours[10]);
        Assert.Equal(25.0, weekHours[10]);
        Assert.Equal(25.0, todayHours[10]);
        Assert.Equal(25.0, todayHours[11]);
    }

    [Fact]
    public void BuildHourlyFocusMinutes_AverageDividesByActiveDaysInRange()
    {
        var settings = new WidgetSettings
        {
            PomodoroFocusSessions =
            [
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-27T10:30:00", FocusMinutes = 30 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T10:30:00", FocusMinutes = 30 }
            ]
        };

        var hours = PomodoroStatsCalculator.BuildHourlyFocusMinutes(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0),
            PomodoroRhythmRange.Week,
            PomodoroRhythmMode.Average);

        Assert.Equal(30.0, hours[10]);
        Assert.Equal(30.0, hours.Sum());
    }

    [Fact]
    public void BuildHourlyPomodoroCounts_CountsSessionsByCompletionHour()
    {
        var settings = new WidgetSettings
        {
            PomodoroFocusSessions =
            [
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T10:10:00", FocusMinutes = 25 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T10:45:00", FocusMinutes = 25 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T11:05:00", FocusMinutes = 10 }
            ]
        };

        var hours = PomodoroStatsCalculator.BuildHourlyPomodoroCounts(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0));

        Assert.Equal(2.0, hours[10]);
        Assert.Equal(1.0, hours[11]);
        Assert.Equal(3.0, hours.Sum());
    }

    [Fact]
    public void BuildHourlyPomodoroCounts_AverageDividesByActiveDaysInRange()
    {
        var settings = new WidgetSettings
        {
            PomodoroFocusSessions =
            [
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-27T10:30:00", FocusMinutes = 30 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T10:30:00", FocusMinutes = 30 },
                new PomodoroFocusSessionEntry { CompletedAt = "2026-06-28T11:30:00", FocusMinutes = 30 }
            ]
        };

        var hours = PomodoroStatsCalculator.BuildHourlyPomodoroCounts(
            settings,
            new DateTime(2026, 6, 28, 14, 30, 0),
            PomodoroRhythmRange.Week,
            PomodoroRhythmMode.Average);

        Assert.Equal(1.0, hours[10]);
        Assert.Equal(0.5, hours[11]);
        Assert.Equal(1.5, hours.Sum());
    }
}
