using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using WpfButton = System.Windows.Controls.Button;
using WpfMessageBox = System.Windows.MessageBox;

namespace ClockWidget;

public partial class PomodoroStatsWindow : Window
{
    private const int RecentActivityDays = 120;
    private const int ActivityScrollDays = 30;
    private const double ActivityCellSize = 20;
    private const double ActivityCellGap = 6;
    private const double ActivityColumnWidth = ActivityCellSize + ActivityCellGap;
    private const double ActivityMonthGap = 8;
    private const double MonthlyBarMaxHeight = 104;
    private const double MonthlyCountLabelReserve = 26;
    private const double HourlyBarMaxHeight = 150;
    private const double HourlyValueLabelReserve = 28;
    private const int TodayPomodoroIconGroupSize = 4;
    private static readonly MediaBrush ActivityEmptyBrush = CreateFrozenBrush(MediaColor.FromRgb(233, 238, 245));
    private static readonly MediaBrush ActivityLevel1Brush = CreateFrozenBrush(MediaColor.FromRgb(189, 236, 203));
    private static readonly MediaBrush ActivityLevel2Brush = CreateFrozenBrush(MediaColor.FromRgb(131, 217, 154));
    private static readonly MediaBrush ActivityLevel3Brush = CreateFrozenBrush(MediaColor.FromRgb(79, 188, 106));
    private static readonly MediaBrush ActivityLevel4Brush = CreateFrozenBrush(MediaColor.FromRgb(36, 139, 67));
    private static readonly MediaBrush ActivityTextBrush = CreateFrozenBrush(MediaColor.FromRgb(102, 112, 133));
    private static readonly MediaBrush ActivityBorderBrush = CreateFrozenBrush(MediaColor.FromRgb(215, 222, 232));
    private static readonly MediaBrush TomatoBrush = CreateFrozenBrush(MediaColor.FromRgb(194, 103, 96));
    private static readonly MediaBrush TomatoHighlightBrush = CreateFrozenBrush(MediaColor.FromRgb(226, 154, 148));
    private static readonly MediaBrush TomatoLeafBrush = CreateFrozenBrush(MediaColor.FromRgb(104, 151, 108));
    private readonly WidgetSettings _settings;
    private readonly Action<PomodoroStatsResetScope> _resetStats;
    private readonly Action<DateTime, int> _deleteFocusSession;
    private readonly Action _saveRhythmOptions;
    private PomodoroRhythmMetric _monthlyMetric = PomodoroRhythmMetric.Minutes;
    private PomodoroRhythmMetric _rhythmMetric = PomodoroRhythmMetric.Minutes;
    private PomodoroRhythmMode _rhythmMode = PomodoroRhythmMode.Total;
    private PomodoroRhythmRange _rhythmRange = PomodoroRhythmRange.AllTime;
    private DateTime? _recentActivityEndDate;

    internal PomodoroStatsWindow(
        WidgetSettings settings,
        Action<PomodoroStatsResetScope> resetStats,
        Action<DateTime, int> deleteFocusSession,
        Action saveRhythmOptions)
    {
        _settings = settings;
        _resetStats = resetStats;
        _deleteFocusSession = deleteFocusSession;
        _saveRhythmOptions = saveRhythmOptions;
        _monthlyMetric = _settings.PomodoroMonthlyMetric;
        _rhythmMetric = _settings.PomodoroRhythmMetric;
        _rhythmMode = _settings.PomodoroRhythmMode;
        _rhythmRange = _settings.PomodoroRhythmRange;
        InitializeComponent();
        UpdateMonthlyOptionButtons();
        UpdateRhythmOptionButtons();
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
        LoadRhythm(_settings, today);
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

        var latestEndDate = today.Date;
        if (_recentActivityEndDate is null || _recentActivityEndDate.Value > latestEndDate)
        {
            _recentActivityEndDate = latestEndDate;
        }

        LoadRecentActivity(values, _recentActivityEndDate.Value, latestEndDate);
        LoadMonthlyBars(values, today.Date);
    }

    private void LoadRhythm(WidgetSettings settings, DateTime today)
    {
        var hourlyValues = _rhythmMetric == PomodoroRhythmMetric.Pomodoros
            ? PomodoroStatsCalculator.BuildHourlyPomodoroCounts(settings, today, _rhythmRange, _rhythmMode)
            : PomodoroStatsCalculator.BuildHourlyFocusMinutes(settings, today, _rhythmRange, _rhythmMode);
        LoadHourlyRhythm(hourlyValues);
        LoadTodayFocusSessions(settings, today.Date);
    }

    private void LoadHourlyRhythm(IReadOnlyList<double> hourlyMinutes)
    {
        HourlyRhythmGrid.Children.Clear();
        HourlyRhythmGrid.ColumnDefinitions.Clear();
        HourlyRhythmGrid.RowDefinitions.Clear();

        HourlyRhythmGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HourlyBarMaxHeight + HourlyValueLabelReserve) });
        HourlyRhythmGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var maxValue = Math.Max(1, hourlyMinutes.Max());
        for (var hour = 0; hour < 24; hour++)
        {
            HourlyRhythmGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            AddHourlyBar(hour, hourlyMinutes[hour], maxValue);
        }
    }

    private void AddHourlyBar(int hour, double value, double maxValue)
    {
        var valueLabel = new TextBlock
        {
            Text = value > 0
                ? FormatRhythmValue(value)
                : "",
            Foreground = ActivityTextBrush,
            FontSize = 13.2,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6)
        };

        var barHeight = value == 0
            ? 6
            : Math.Max(12, HourlyBarMaxHeight * value / maxValue);
        var bar = new Border
        {
            Width = 18,
            Height = barHeight,
            CornerRadius = new CornerRadius(5, 5, 3, 3),
            Background = GetActivityBrush(value),
            BorderBrush = value > 0 ? null : ActivityBorderBrush,
            BorderThickness = value > 0 ? new Thickness(0) : new Thickness(1),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
            ToolTip = $"{hour:00}:00 - {FormatRhythmValue(value)} {GetRhythmUnitLabel()}",
            Margin = new Thickness(2, 0, 2, 0)
        };

        var barStack = new StackPanel
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom
        };
        barStack.Children.Add(valueLabel);
        barStack.Children.Add(bar);

        Grid.SetRow(barStack, 0);
        Grid.SetColumn(barStack, hour);
        HourlyRhythmGrid.Children.Add(barStack);

        var hourLabel = new TextBlock
        {
            Text = hour % 3 == 0 ? hour.ToString("00", CultureInfo.InvariantCulture) : "",
            Foreground = ActivityTextBrush,
            FontSize = 13.2,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        Grid.SetRow(hourLabel, 1);
        Grid.SetColumn(hourLabel, hour);
        HourlyRhythmGrid.Children.Add(hourLabel);
    }

    private string GetRhythmUnitLabel()
    {
        return (_rhythmMetric, _rhythmMode) switch
        {
            (PomodoroRhythmMetric.Pomodoros, PomodoroRhythmMode.Average) => "avg pomodoros",
            (PomodoroRhythmMetric.Pomodoros, _) => "pomodoros",
            (_, PomodoroRhythmMode.Average) => "avg focus minutes",
            _ => "focus minutes"
        };
    }

    private static string FormatRhythmValue(double value)
    {
        return value >= 10 || Math.Abs(value - Math.Round(value)) < 0.05
            ? Math.Round(value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private void SetRhythmMetric(PomodoroRhythmMetric metric)
    {
        if (_rhythmMetric == metric)
        {
            return;
        }

        _rhythmMetric = metric;
        _settings.PomodoroRhythmMetric = metric;
        UpdateRhythmOptionButtons();
        _saveRhythmOptions();
        LoadRhythm(_settings, DateTime.Now);
    }

    private void SetRhythmMode(PomodoroRhythmMode mode)
    {
        if (_rhythmMode == mode)
        {
            return;
        }

        _rhythmMode = mode;
        _settings.PomodoroRhythmMode = mode;
        UpdateRhythmOptionButtons();
        _saveRhythmOptions();
        LoadRhythm(_settings, DateTime.Now);
    }

    private void SetRhythmRange(PomodoroRhythmRange range)
    {
        if (_rhythmRange == range)
        {
            return;
        }

        _rhythmRange = range;
        _settings.PomodoroRhythmRange = range;
        UpdateRhythmOptionButtons();
        _saveRhythmOptions();
        LoadRhythm(_settings, DateTime.Now);
    }

    private void UpdateRhythmOptionButtons()
    {
        RhythmMinutesButton.IsEnabled = _rhythmMetric != PomodoroRhythmMetric.Minutes;
        RhythmPomodorosButton.IsEnabled = _rhythmMetric != PomodoroRhythmMetric.Pomodoros;
        RhythmAverageButton.IsEnabled = _rhythmMode != PomodoroRhythmMode.Average;
        RhythmTotalButton.IsEnabled = _rhythmMode != PomodoroRhythmMode.Total;
        RhythmAllTimeButton.IsEnabled = _rhythmRange != PomodoroRhythmRange.AllTime;
        RhythmMonthButton.IsEnabled = _rhythmRange != PomodoroRhythmRange.Month;
        RhythmWeekButton.IsEnabled = _rhythmRange != PomodoroRhythmRange.Week;
        RhythmDayButton.IsEnabled = _rhythmRange != PomodoroRhythmRange.Today;
    }

    private void RhythmMinutesButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmMetric(PomodoroRhythmMetric.Minutes);
    }

    private void RhythmPomodorosButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmMetric(PomodoroRhythmMetric.Pomodoros);
    }

    private void RhythmAverageButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmMode(PomodoroRhythmMode.Average);
    }

    private void RhythmTotalButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmMode(PomodoroRhythmMode.Total);
    }

    private void RhythmAllTimeButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmRange(PomodoroRhythmRange.AllTime);
    }

    private void RhythmMonthButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmRange(PomodoroRhythmRange.Month);
    }

    private void RhythmWeekButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmRange(PomodoroRhythmRange.Week);
    }

    private void RhythmDayButton_Click(object sender, RoutedEventArgs e)
    {
        SetRhythmRange(PomodoroRhythmRange.Today);
    }

    private void LoadTodayFocusSessions(WidgetSettings settings, DateTime today)
    {
        TodayFocusSessionsPanel.Children.Clear();

        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        var todaySessions = normalizedSettings.PomodoroFocusSessions
            .Select(entry => new
            {
                Entry = entry,
                CompletedAt = PomodoroStatsCalculator.TryParseStatsTimestamp(entry.CompletedAt, out var completedAt)
                    ? completedAt
                    : (DateTime?)null
            })
            .Where(item => item.CompletedAt is not null && item.CompletedAt.Value.Date == today)
            .OrderByDescending(item => item.CompletedAt)
            .ToList();

        LoadTodayPomodoroIcons(todaySessions.Count);

        if (todaySessions.Count == 0)
        {
            TodayFocusSessionsPanel.Children.Add(new TextBlock
            {
                Text = "No completed focus sessions today.",
                Style = (Style)FindResource("MutedTextStyle"),
                FontSize = 17
            });
            return;
        }

        foreach (var session in todaySessions)
        {
            AddTodayFocusSession(session.CompletedAt!.Value, session.Entry.FocusMinutes);
        }
    }

    private void LoadTodayPomodoroIcons(int count)
    {
        TodayPomodoroIconsPanel.Children.Clear();

        for (var i = 0; i < count; i++)
        {
            TodayPomodoroIconsPanel.Children.Add(CreateTomatoIcon((i + 1) % TodayPomodoroIconGroupSize == 0 && i + 1 < count));
        }
    }

    private static FrameworkElement CreateTomatoIcon(bool endsGroup)
    {
        var icon = new Grid
        {
            Width = 22,
            Height = 22,
            Margin = new Thickness(0, 0, endsGroup ? 16 : 4, 0),
            ToolTip = "Completed Pomodoro"
        };

        icon.Children.Add(new Ellipse
        {
            Width = 17.5,
            Height = 16.5,
            Fill = TomatoBrush,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 1)
        });

        icon.Children.Add(new Ellipse
        {
            Width = 5.5,
            Height = 4.5,
            Fill = TomatoHighlightBrush,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            VerticalAlignment = System.Windows.VerticalAlignment.Top,
            Margin = new Thickness(6, 7.2, 0, 0),
            Opacity = 0.75
        });

        icon.Children.Add(new Polygon
        {
            Fill = TomatoLeafBrush,
            Points = new PointCollection
            {
                new(11, 1.2),
                new(13.8, 7.2),
                new(11, 6),
                new(8.2, 8.2),
                new(8.9, 5.5),
                new(6, 6),
                new(8.9, 4)
            }
        });

        return icon;
    }

    private void AddTodayFocusSession(DateTime completedAt, int focusMinutes)
    {
        var startedAt = completedAt.AddMinutes(-focusMinutes);
        var row = new Border
        {
            Background = System.Windows.Media.Brushes.Transparent,
            BorderBrush = ActivityBorderBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 9, 0, 9)
        };

        var content = new DockPanel
        {
            LastChildFill = true
        };
        var deleteButton = new WpfButton
        {
            Content = "×",
            Style = (Style)FindResource("DeletePomodoroButtonStyle"),
            ToolTip = "Delete this Pomodoro"
        };
        deleteButton.Click += (_, _) => DeleteFocusSession(completedAt, focusMinutes);
        DockPanel.SetDock(deleteButton, Dock.Left);
        content.Children.Add(deleteButton);

        content.Children.Add(new TextBlock
        {
            Text = $"{startedAt:yyyy-MM-dd HH:mm} - {focusMinutes.ToString(CultureInfo.InvariantCulture)} minutes work",
            FontSize = 17,
            Foreground = (MediaBrush)FindResource("WindowTextBrush"),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        });

        row.Child = content;
        TodayFocusSessionsPanel.Children.Add(row);
    }

    private void DeleteFocusSession(DateTime completedAt, int focusMinutes)
    {
        _deleteFocusSession(completedAt, focusMinutes);
        LoadStats();
    }

    private void LoadRecentActivity(
        IReadOnlyDictionary<string, PomodoroStatsEntry> values,
        DateTime dataEndDate,
        DateTime latestEndDate)
    {
        RecentActivityGrid.Children.Clear();
        RecentActivityGrid.ColumnDefinitions.Clear();
        RecentActivityGrid.RowDefinitions.Clear();

        dataEndDate = dataEndDate.Date;
        latestEndDate = latestEndDate.Date;
        var isLatestRange = dataEndDate >= latestEndDate;
        var displayEndDate = isLatestRange
            ? new DateTime(dataEndDate.Year, dataEndDate.Month, 1).AddMonths(1).AddDays(-1)
            : dataEndDate;
        var startDate = dataEndDate.AddDays(-(RecentActivityDays - 1));
        ActivityRangeText.Text = FormatActivityRange(startDate, dataEndDate, isLatestRange);
        ActivityNextButton.IsEnabled = !isLatestRange;
        ActivityTodayButton.IsEnabled = !isLatestRange;

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

        MonthlyBarsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(MonthlyBarMaxHeight + MonthlyCountLabelReserve) });
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
        var maxValue = Math.Max(1, months.Max(GetMonthlyBarValue));

        for (var index = 0; index < months.Count; index++)
        {
            MonthlyBarsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            AddMonthlyBar(months[index], maxValue, index);
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

    private void AddMonthlyBar(MonthlyActivity month, int maxValue, int column)
    {
        var value = GetMonthlyBarValue(month);
        var countLabel = new TextBlock
        {
            Text = value > 0
                ? value.ToString(CultureInfo.InvariantCulture)
                : "",
            Foreground = ActivityTextBrush,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 7)
        };

        var barHeight = value == 0
            ? 6
            : Math.Max(12, MonthlyBarMaxHeight * value / maxValue);
        var bar = new Border
        {
            Width = 28,
            Height = barHeight,
            CornerRadius = new CornerRadius(6, 6, 3, 3),
            Background = GetActivityBrush(value),
            BorderBrush = value > 0 ? null : ActivityBorderBrush,
            BorderThickness = value > 0 ? new Thickness(0) : new Thickness(1),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
            ToolTip = $"{month.MonthStart:MMM yyyy}: {month.Count.ToString(CultureInfo.InvariantCulture)} Pomodoro, {month.FocusMinutes.ToString(CultureInfo.InvariantCulture)} focus minutes",
            Margin = new Thickness(4, 0, 4, 0)
        };

        var barStack = new StackPanel
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom
        };
        barStack.Children.Add(countLabel);
        barStack.Children.Add(bar);

        Grid.SetRow(barStack, 0);
        Grid.SetColumn(barStack, column);
        MonthlyBarsGrid.Children.Add(barStack);

        var label = new TextBlock
        {
            Text = month.MonthStart.ToString("MMM", CultureInfo.InvariantCulture),
            Foreground = ActivityTextBrush,
            FontSize = 12,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        Grid.SetRow(label, 1);
        Grid.SetColumn(label, column);
        MonthlyBarsGrid.Children.Add(label);
    }

    private int GetMonthlyBarValue(MonthlyActivity month)
    {
        return _monthlyMetric == PomodoroRhythmMetric.Pomodoros
            ? month.Count
            : month.FocusMinutes;
    }

    private void SetMonthlyMetric(PomodoroRhythmMetric metric)
    {
        if (_monthlyMetric == metric)
        {
            return;
        }

        _monthlyMetric = metric;
        _settings.PomodoroMonthlyMetric = metric;
        UpdateMonthlyOptionButtons();
        _saveRhythmOptions();
        LoadActivity(_settings, DateTime.Now);
    }

    private void UpdateMonthlyOptionButtons()
    {
        MonthlyMinutesButton.IsEnabled = _monthlyMetric != PomodoroRhythmMetric.Minutes;
        MonthlyPomodorosButton.IsEnabled = _monthlyMetric != PomodoroRhythmMetric.Pomodoros;
    }

    private void MonthlyMinutesButton_Click(object sender, RoutedEventArgs e)
    {
        SetMonthlyMetric(PomodoroRhythmMetric.Minutes);
    }

    private void MonthlyPomodorosButton_Click(object sender, RoutedEventArgs e)
    {
        SetMonthlyMetric(PomodoroRhythmMetric.Pomodoros);
    }

    private static bool IsDateInPeriod(string dateText, DateTime start, DateTime end)
    {
        var date = ParseStatsDate(dateText);
        return date is not null
            && date >= start
            && date < end;
    }

    private static DateTime? ParseStatsDate(string dateText)
    {
        return DateTime.TryParseExact(
            dateText,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? date
            : null;
    }

    private static MediaBrush GetActivityBrush(double focusMinutes)
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

    private static string FormatActivityRange(DateTime startDate, DateTime endDate, bool isLatestRange)
    {
        if (isLatestRange)
        {
            return "Last 120 days";
        }

        return startDate.Year == endDate.Year
            ? string.Format(CultureInfo.InvariantCulture, "{0:MMM d} - {1:MMM d, yyyy}", startDate, endDate)
            : string.Format(CultureInfo.InvariantCulture, "{0:MMM d, yyyy} - {1:MMM d, yyyy}", startDate, endDate);
    }

    private void ActivityPreviousButton_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Now.Date;
        _recentActivityEndDate = (_recentActivityEndDate ?? today).AddDays(-ActivityScrollDays);
        LoadActivity(_settings, today);
    }

    private void ActivityNextButton_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Now.Date;
        var nextEndDate = (_recentActivityEndDate ?? today).AddDays(ActivityScrollDays);
        _recentActivityEndDate = nextEndDate > today ? today : nextEndDate;
        LoadActivity(_settings, today);
    }

    private void ActivityTodayButton_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Now.Date;
        _recentActivityEndDate = today;
        LoadActivity(_settings, today);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResetStatsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
        {
            return;
        }

        var menu = new System.Windows.Controls.ContextMenu
        {
            PlacementTarget = button,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Top
        };

        AddResetMenuItem(menu, "All stats", PomodoroStatsResetScope.All);
        AddResetMenuItem(menu, "Today", PomodoroStatsResetScope.Today);
        AddResetMenuItem(menu, "This week", PomodoroStatsResetScope.Week);

        menu.IsOpen = true;
    }

    private void AddResetMenuItem(System.Windows.Controls.ContextMenu menu, string header, PomodoroStatsResetScope scope)
    {
        var item = new System.Windows.Controls.MenuItem
        {
            Header = header
        };
        item.Click += (_, _) => ConfirmAndResetStats(scope);
        menu.Items.Add(item);
    }

    private void ConfirmAndResetStats(PomodoroStatsResetScope scope)
    {
        var result = WpfMessageBox.Show(
            this,
            $"Reset {FormatResetScope(scope)} Pomodoro statistics? This cannot be undone.",
            "Reset Pomodoro Stats",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _resetStats(scope);
        LoadStats();
    }

    private static string FormatResetScope(PomodoroStatsResetScope scope)
    {
        return scope switch
        {
            PomodoroStatsResetScope.Today => "today's",
            PomodoroStatsResetScope.Week => "this week's",
            _ => "all"
        };
    }

    private sealed record MonthlyActivity(DateTime MonthStart, int Count, int FocusMinutes);
}
