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
}
