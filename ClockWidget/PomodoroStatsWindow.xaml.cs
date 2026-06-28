using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using WpfMessageBox = System.Windows.MessageBox;

namespace ClockWidget;

public partial class PomodoroStatsWindow : Window
{
    private const int RecentActivityDays = 120;
    private const double ActivityCellSize = 20;
    private const double ActivityCellGap = 6;
    private const double ActivityColumnWidth = ActivityCellSize + ActivityCellGap;
    private const double ActivityMonthGap = 8;
    private const double MonthlyBarMaxHeight = 104;
    private static readonly MediaBrush ActivityEmptyBrush = CreateFrozenBrush(MediaColor.FromRgb(233, 238, 245));
    private static readonly MediaBrush ActivityLevel1Brush = CreateFrozenBrush(MediaColor.FromRgb(189, 236, 203));
    private static readonly MediaBrush ActivityLevel2Brush = CreateFrozenBrush(MediaColor.FromRgb(131, 217, 154));
    private static readonly MediaBrush ActivityLevel3Brush = CreateFrozenBrush(MediaColor.FromRgb(79, 188, 106));
    private static readonly MediaBrush ActivityLevel4Brush = CreateFrozenBrush(MediaColor.FromRgb(36, 139, 67));
    private static readonly MediaBrush ActivityTextBrush = CreateFrozenBrush(MediaColor.FromRgb(102, 112, 133));
    private static readonly MediaBrush ActivityBorderBrush = CreateFrozenBrush(MediaColor.FromRgb(215, 222, 232));
    private readonly WidgetSettings _settings;
    private readonly PomodoroController _controller;
    private readonly Action _resetStats;

    internal PomodoroStatsWindow(
        WidgetSettings settings,
        PomodoroController controller,
        Action resetStats)
    {
        InitializeComponent();
        _settings = settings;
        _controller = controller;
        _resetStats = resetStats;
        LoadStats();
    }

    private void LoadStats()
    {
        var today = DateTime.Now;
        var stats = PomodoroStatsCalculator.BuildSummary(_settings, today);
        TodayTitleText.Text = $"Stats, {today:MMM d}";
        LoadPeriod(TodayPomodoroCountText, TodayFocusMinutesText, stats.Today);
        LoadPeriod(WeekPomodoroCountText, WeekFocusMinutesText, stats.Week);
        LoadPeriod(MonthPomodoroCountText, MonthFocusMinutesText, stats.Month);
        LoadPeriod(YearPomodoroCountText, YearFocusMinutesText, stats.Year);
        LoadActivity(_settings, today);
        PhaseText.Text = _controller.Phase == PomodoroPhase.Focus ? "Focus" : "Break";
        RemainingText.Text = FormatDuration(_controller.Remaining);
        RunningStateText.Text = _controller.IsRunning ? "Timer is running." : "Timer is paused.";
    }

    private static void LoadPeriod(
        TextBlock countText,
        TextBlock focusMinutesText,
        PomodoroStatsPeriod period)
    {
        countText.Text = period.Count.ToString(CultureInfo.InvariantCulture);
        focusMinutesText.Text = $"{period.FocusMinutes.ToString(CultureInfo.InvariantCulture)}m";
    }

    private void LoadActivity(WidgetSettings settings, DateTime today)
    {
        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        var values = normalizedSettings.PomodoroStatsHistory
            .ToDictionary(entry => entry.Date, entry => entry, StringComparer.Ordinal);

        LoadRecentActivity(values, today.Date);
        LoadMonthlyBars(values, today.Date);
    }

    private void LoadRecentActivity(IReadOnlyDictionary<string, PomodoroStatsEntry> values, DateTime today)
    {
        RecentActivityGrid.Children.Clear();
        RecentActivityGrid.ColumnDefinitions.Clear();
        RecentActivityGrid.RowDefinitions.Clear();

        var dataEndDate = today.Date;
        var displayEndDate = new DateTime(dataEndDate.Year, dataEndDate.Month, 1).AddMonths(1).AddDays(-1);
        var startDate = dataEndDate.AddDays(-(RecentActivityDays - 1));

        RecentActivityGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var day = 0; day < 7; day++)
        {
            RecentActivityGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ActivityColumnWidth) });
        }

        RecentActivityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
        AddWeekdayLabel("Mon", 1);
        AddWeekdayLabel("Wed", 3);
        AddWeekdayLabel("Fri", 5);

        var column = 1;
        var monthStart = new DateTime(startDate.Year, startDate.Month, 1);
        while (monthStart <= displayEndDate)
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var visibleStart = startDate > monthStart ? startDate : monthStart;
            var visibleEnd = displayEndDate < monthEnd ? displayEndDate : monthEnd;
            var monthGridStart = GetWeekStart(visibleStart);
            var monthGridEnd = GetWeekStart(visibleEnd);
            var monthWeekCount = (int)((monthGridEnd - monthGridStart).TotalDays / 7) + 1;
            var firstMonthColumn = column;

            AddMonthLabel(visibleStart, firstMonthColumn);

            for (var week = 0; week < monthWeekCount; week++)
            {
                RecentActivityGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(ActivityColumnWidth + (week == 0 && column > 1 ? ActivityMonthGap : 0))
                });
                var weekStart = monthGridStart.AddDays(week * 7);
                for (var day = 0; day < 7; day++)
                {
                    var date = weekStart.AddDays(day);
                    if (date < visibleStart || date > visibleEnd)
                    {
                        continue;
                    }

                    var focusMinutes = 0;
                    if (date <= dataEndDate)
                    {
                        var dateKey = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        focusMinutes = values.TryGetValue(dateKey, out var entry)
                            ? Math.Max(0, entry.FocusMinutes)
                            : 0;
                    }

                    AddActivityCell(
                        date,
                        focusMinutes,
                        column,
                        day + 1,
                        week == 0 && column > 1);
                }

                column++;
            }

            monthStart = monthStart.AddMonths(1);
        }
    }

    private void LoadMonthlyBars(IReadOnlyDictionary<string, PomodoroStatsEntry> values, DateTime today)
    {
        MonthlyBarsGrid.Children.Clear();
        MonthlyBarsGrid.ColumnDefinitions.Clear();
        MonthlyBarsGrid.RowDefinitions.Clear();

        MonthlyBarsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        MonthlyBarsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MonthlyBarMaxHeight) });
        MonthlyBarsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var months = Enumerable.Range(0, 12)
            .Select(offset => new DateTime(today.Year, today.Month, 1).AddMonths(offset - 11))
            .Select(monthStart =>
            {
                var nextMonth = monthStart.AddMonths(1);
                var entries = values
                    .Where(pair => IsDateInPeriod(pair.Key, monthStart, nextMonth))
                    .Select(pair => pair.Value)
                    .ToList();
                var count = entries.Sum(entry => Math.Max(0, entry.Count));
                var focusMinutes = entries.Sum(entry => Math.Max(0, entry.FocusMinutes));
                return new MonthlyActivity(monthStart, count, focusMinutes);
            })
            .ToList();
        var maxMinutes = Math.Max(1, months.Max(month => month.FocusMinutes));

        for (var index = 0; index < months.Count; index++)
        {
            MonthlyBarsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            AddMonthlyBar(months[index], maxMinutes, index);
        }
    }

    private void AddMonthLabel(DateTime date, int column)
    {
        var label = new TextBlock
        {
            Text = date.ToString("MMM", CultureInfo.InvariantCulture),
            Foreground = ActivityTextBrush,
            FontSize = 13,
            Margin = new Thickness(ActivityMonthGap, 0, 0, 7)
        };

        Grid.SetRow(label, 0);
        Grid.SetColumn(label, column);
        Grid.SetColumnSpan(label, 4);
        RecentActivityGrid.Children.Add(label);
    }

    private void AddWeekdayLabel(string text, int row)
    {
        var label = new TextBlock
        {
            Text = text,
            Foreground = ActivityTextBrush,
            FontSize = 13,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, ActivityCellGap)
        };

        Grid.SetRow(label, row);
        Grid.SetColumn(label, 0);
        RecentActivityGrid.Children.Add(label);
    }

    private void AddActivityCell(DateTime date, int focusMinutes, int column, int row, bool addMonthGap)
    {
        var cell = new Border
        {
            Width = ActivityCellSize,
            Height = ActivityCellSize,
            CornerRadius = new CornerRadius(5),
            Background = GetActivityBrush(focusMinutes),
            BorderBrush = focusMinutes > 0 ? null : ActivityBorderBrush,
            BorderThickness = focusMinutes > 0 ? new Thickness(0) : new Thickness(1),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            ToolTip = $"{date:MMM d, yyyy}: {focusMinutes.ToString(CultureInfo.InvariantCulture)} focus minutes",
            Margin = new Thickness(addMonthGap ? ActivityMonthGap : 0, 0, ActivityCellGap, ActivityCellGap)
        };

        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, column);
        RecentActivityGrid.Children.Add(cell);
    }

    private void AddMonthlyBar(MonthlyActivity month, int maxMinutes, int column)
    {
        var countLabel = new TextBlock
        {
            Text = month.Count > 0
                ? month.Count.ToString(CultureInfo.InvariantCulture)
                : "",
            Foreground = ActivityTextBrush,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 7)
        };

        Grid.SetRow(countLabel, 0);
        Grid.SetColumn(countLabel, column);
        MonthlyBarsGrid.Children.Add(countLabel);

        var barHeight = month.FocusMinutes == 0
            ? 6
            : Math.Max(12, MonthlyBarMaxHeight * month.FocusMinutes / maxMinutes);
        var bar = new Border
        {
            Width = 28,
            Height = barHeight,
            CornerRadius = new CornerRadius(6, 6, 3, 3),
            Background = GetActivityBrush(month.FocusMinutes),
            BorderBrush = month.FocusMinutes > 0 ? null : ActivityBorderBrush,
            BorderThickness = month.FocusMinutes > 0 ? new Thickness(0) : new Thickness(1),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
            ToolTip = $"{month.MonthStart:MMM yyyy}: {month.Count.ToString(CultureInfo.InvariantCulture)} Pomodoro, {month.FocusMinutes.ToString(CultureInfo.InvariantCulture)} focus minutes",
            Margin = new Thickness(4, 0, 4, 0)
        };

        Grid.SetRow(bar, 1);
        Grid.SetColumn(bar, column);
        MonthlyBarsGrid.Children.Add(bar);

        var label = new TextBlock
        {
            Text = month.MonthStart.ToString("MMM", CultureInfo.InvariantCulture),
            Foreground = ActivityTextBrush,
            FontSize = 12,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        Grid.SetRow(label, 2);
        Grid.SetColumn(label, column);
        MonthlyBarsGrid.Children.Add(label);
    }

    private static bool IsDateInPeriod(string dateText, DateTime start, DateTime end)
    {
        return DateTime.TryParseExact(
            dateText,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            && date >= start
            && date < end;
    }

    private static MediaBrush GetActivityBrush(int focusMinutes)
    {
        return focusMinutes switch
        {
            <= 0 => ActivityEmptyBrush,
            < 30 => ActivityLevel1Brush,
            < 60 => ActivityLevel2Brush,
            < 120 => ActivityLevel3Brush,
            _ => ActivityLevel4Brush
        };
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysFromMonday);
    }

    private static SolidColorBrush CreateFrozenBrush(MediaColor color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        return duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours:0}:{duration.Minutes:00}:{duration.Seconds:00}"
            : $"{duration.Minutes:0}:{duration.Seconds:00}";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResetStatsButton_Click(object sender, RoutedEventArgs e)
    {
        var result = WpfMessageBox.Show(
            this,
            "Reset all Pomodoro statistics? This cannot be undone.",
            "Reset Pomodoro Stats",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _resetStats();
        LoadStats();
    }

    private sealed record MonthlyActivity(DateTime MonthStart, int Count, int FocusMinutes);
}
