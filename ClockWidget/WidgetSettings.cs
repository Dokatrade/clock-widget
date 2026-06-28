using System.Globalization;

namespace ClockWidget;

public sealed class WidgetSettings
{
    public const double MinWidth = 120;
    public const double MaxWidth = 620;
    public const double MinHeight = 42;
    public const double MaxHeight = 260;
    public const double MinClockFontSize = 32;
    public const double MaxClockFontSize = 120;
    public const int MinClockFontWeight = 300;
    public const int MaxClockFontWeight = 900;
    public const double MinPadding = 0;
    public const double MaxPadding = 40;
    public const byte MinBackgroundShade = 0;
    public const byte MaxBackgroundShade = 80;
    public const double MinBackgroundOpacity = 0.2;
    public const double MaxBackgroundOpacity = 1;
    public const double MinDateFontSize = 8;
    public const double MaxDateFontSize = 42;
    public const int MinPomodoroFocusMinutes = 1;
    public const int MaxPomodoroFocusMinutes = 120;
    public const int MinPomodoroBreakMinutes = 1;
    public const int MaxPomodoroBreakMinutes = 60;
    public const int MinPomodoroLongBreakInterval = 1;
    public const int MaxPomodoroLongBreakInterval = 20;

    public double Left { get; set; } = double.NaN;
    public double Top { get; set; } = double.NaN;
    public double Width { get; set; } = 330;
    public double Height { get; set; } = 118;
    public bool FitToContent { get; set; } = true;
    public double ClockFontSize { get; set; } = 56;
    public int ClockFontWeight { get; set; } = 600;
    public double PaddingHorizontal { get; set; } = 18;
    public double PaddingTop { get; set; } = 10;
    public double PaddingBottom { get; set; } = 10;
    public byte BackgroundShade { get; set; } = 20;
    public double BackgroundOpacity { get; set; } = 0.85;
    public bool ShowBorder { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = true;
    public bool ShowSeconds { get; set; } = true;
    public bool ShowSideDate { get; set; }
    public bool ShowDate { get; set; } = true;
    public bool ShowWeekday { get; set; } = true;
    public double DateFontSize { get; set; } = 13;
    public bool LockPosition { get; set; }
    public bool SnapToScreenEdges { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool PomodoroEnabled { get; set; } = true;
    public int PomodoroFocusMinutes { get; set; } = 25;
    public int PomodoroBreakMinutes { get; set; } = 5;
    public int PomodoroLongBreakInterval { get; set; } = 4;
    public int PomodoroLongBreakMinutes { get; set; } = 15;
    public bool PomodoroAutoStartBreak { get; set; } = true;
    public bool PomodoroReturnToClockAfterBreak { get; set; } = true;
    public bool PomodoroPlaySound { get; set; } = true;
    public bool ShowPomodoroDailyStats { get; set; }
    public PomodoroSound PomodoroSound { get; set; } = PomodoroSound.FreesoundsNotification;
    public string PomodoroDailyStatsDate { get; set; } = "";
    public int PomodoroDailyCount { get; set; }
    public int PomodoroDailyFocusMinutes { get; set; }
    public List<PomodoroStatsEntry> PomodoroStatsHistory { get; set; } = [];
    public List<WidgetPreset> Presets { get; set; } = [];

    public static IReadOnlyList<WidgetPreset> CreateBuiltInPresets()
    {
        return
        [
            CreateBuiltInPreset(new WidgetPreset
            {
                Name = "Compact",
                Width = 260,
                Height = 86,
                FitToContent = true,
                ClockFontSize = 42,
                ClockFontWeight = 600,
                PaddingHorizontal = 12,
                PaddingTop = 7,
                PaddingBottom = 7,
                BackgroundShade = 18,
                BackgroundOpacity = 0.78,
                ShowBorder = true,
                ShowSeconds = false,
                ShowDate = true,
                ShowWeekday = false,
                DateFontSize = 11
            }),
            CreateBuiltInPreset(new WidgetPreset
            {
                Name = "Large",
                Width = 430,
                Height = 150,
                FitToContent = true,
                ClockFontSize = 76,
                ClockFontWeight = 600,
                PaddingHorizontal = 22,
                PaddingTop = 12,
                PaddingBottom = 12,
                BackgroundShade = 18,
                BackgroundOpacity = 0.85,
                ShowBorder = true,
                ShowSeconds = true,
                ShowDate = true,
                ShowWeekday = true,
                DateFontSize = 16
            }),
            CreateBuiltInPreset(new WidgetPreset
            {
                Name = "Minimal",
                Width = 300,
                Height = 82,
                FitToContent = true,
                ClockFontSize = 56,
                ClockFontWeight = 500,
                PaddingHorizontal = 10,
                PaddingTop = 6,
                PaddingBottom = 6,
                BackgroundShade = 0,
                BackgroundOpacity = 0.55,
                ShowBorder = false,
                ShowSeconds = false,
                ShowDate = false,
                ShowWeekday = false,
                DateFontSize = 12
            }),
            CreateBuiltInPreset(new WidgetPreset
            {
                Name = "Pomodoro",
                Width = 340,
                Height = 112,
                FitToContent = true,
                ClockFontSize = 60,
                ClockFontWeight = 600,
                PaddingHorizontal = 18,
                PaddingTop = 10,
                PaddingBottom = 12,
                BackgroundShade = 8,
                BackgroundOpacity = 0.9,
                ShowBorder = true,
                ShowSeconds = false,
                ShowDate = true,
                ShowWeekday = false,
                DateFontSize = 13
            })
        ];
    }

    public void Normalize()
    {
        Width = ClampFinite(Width, MinWidth, MaxWidth, 330);
        Height = ClampFinite(Height, MinHeight, MaxHeight, 118);
        ClockFontSize = ClampFinite(ClockFontSize, MinClockFontSize, MaxClockFontSize, 56);
        ClockFontWeight = SnapFontWeight(ClockFontWeight);
        PaddingHorizontal = ClampFinite(PaddingHorizontal, MinPadding, MaxPadding, 18);
        PaddingTop = ClampFinite(PaddingTop, MinPadding, MaxPadding, 10);
        PaddingBottom = ClampFinite(PaddingBottom, MinPadding, MaxPadding, 10);
        BackgroundShade = (byte)Math.Clamp(BackgroundShade, MinBackgroundShade, MaxBackgroundShade);
        BackgroundOpacity = ClampFinite(BackgroundOpacity, MinBackgroundOpacity, MaxBackgroundOpacity, 0.85);
        DateFontSize = ClampFinite(DateFontSize, MinDateFontSize, MaxDateFontSize, 13);
        PomodoroFocusMinutes = Math.Clamp(PomodoroFocusMinutes, MinPomodoroFocusMinutes, MaxPomodoroFocusMinutes);
        PomodoroBreakMinutes = Math.Clamp(PomodoroBreakMinutes, MinPomodoroBreakMinutes, MaxPomodoroBreakMinutes);
        PomodoroLongBreakInterval = Math.Clamp(PomodoroLongBreakInterval, MinPomodoroLongBreakInterval, MaxPomodoroLongBreakInterval);
        PomodoroLongBreakMinutes = Math.Clamp(PomodoroLongBreakMinutes, MinPomodoroBreakMinutes, MaxPomodoroBreakMinutes);
        PomodoroDailyCount = Math.Max(0, PomodoroDailyCount);
        PomodoroDailyFocusMinutes = Math.Max(0, PomodoroDailyFocusMinutes);
        PomodoroStatsHistory = NormalizePomodoroStatsHistory(PomodoroStatsHistory);
        UpsertPomodoroStatsHistoryEntry(
            PomodoroDailyStatsDate,
            PomodoroDailyCount,
            PomodoroDailyFocusMinutes);

        if (!Enum.IsDefined(typeof(PomodoroSound), PomodoroSound))
        {
            PomodoroSound = PomodoroSound.FreesoundsNotification;
        }

        Presets = (Presets ?? [])
            .Where(preset => !string.IsNullOrWhiteSpace(preset.Name))
            .GroupBy(preset => preset.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var preset = group.Last().Clone();
                preset.Name = group.Key;
                preset.Normalize();
                return preset;
            })
            .OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void ResetAppearanceToDefaults()
    {
        var defaults = new WidgetSettings();
        Width = defaults.Width;
        Height = defaults.Height;
        FitToContent = defaults.FitToContent;
        ClockFontSize = defaults.ClockFontSize;
        ClockFontWeight = defaults.ClockFontWeight;
        PaddingHorizontal = defaults.PaddingHorizontal;
        PaddingTop = defaults.PaddingTop;
        PaddingBottom = defaults.PaddingBottom;
        BackgroundShade = defaults.BackgroundShade;
        BackgroundOpacity = defaults.BackgroundOpacity;
        ShowBorder = defaults.ShowBorder;
        ShowSideDate = defaults.ShowSideDate;
        ShowDate = defaults.ShowDate;
        ShowWeekday = defaults.ShowWeekday;
        DateFontSize = defaults.DateFontSize;
        Normalize();
    }

    public WidgetSettings Clone()
    {
        return new WidgetSettings
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            FitToContent = FitToContent,
            ClockFontSize = ClockFontSize,
            ClockFontWeight = ClockFontWeight,
            PaddingHorizontal = PaddingHorizontal,
            PaddingTop = PaddingTop,
            PaddingBottom = PaddingBottom,
            BackgroundShade = BackgroundShade,
            BackgroundOpacity = BackgroundOpacity,
            ShowBorder = ShowBorder,
            AlwaysOnTop = AlwaysOnTop,
            ShowSeconds = ShowSeconds,
            ShowSideDate = ShowSideDate,
            ShowDate = ShowDate,
            ShowWeekday = ShowWeekday,
            DateFontSize = DateFontSize,
            LockPosition = LockPosition,
            SnapToScreenEdges = SnapToScreenEdges,
            StartWithWindows = StartWithWindows,
            PomodoroEnabled = PomodoroEnabled,
            PomodoroFocusMinutes = PomodoroFocusMinutes,
            PomodoroBreakMinutes = PomodoroBreakMinutes,
            PomodoroLongBreakInterval = PomodoroLongBreakInterval,
            PomodoroLongBreakMinutes = PomodoroLongBreakMinutes,
            PomodoroAutoStartBreak = PomodoroAutoStartBreak,
            PomodoroReturnToClockAfterBreak = PomodoroReturnToClockAfterBreak,
            PomodoroPlaySound = PomodoroPlaySound,
            ShowPomodoroDailyStats = ShowPomodoroDailyStats,
            PomodoroSound = PomodoroSound,
            PomodoroDailyStatsDate = PomodoroDailyStatsDate,
            PomodoroDailyCount = PomodoroDailyCount,
            PomodoroDailyFocusMinutes = PomodoroDailyFocusMinutes,
            PomodoroStatsHistory = (PomodoroStatsHistory ?? []).Select(entry => entry.Clone()).ToList(),
            Presets = (Presets ?? []).Select(preset => preset.Clone()).ToList()
        };
    }

    public int GetBreakMinutesForCompletedPomodoros(int completedPomodoros)
    {
        completedPomodoros = Math.Max(0, completedPomodoros);
        var longBreakInterval = Math.Clamp(PomodoroLongBreakInterval, MinPomodoroLongBreakInterval, MaxPomodoroLongBreakInterval);
        var shortBreakMinutes = Math.Clamp(PomodoroBreakMinutes, MinPomodoroBreakMinutes, MaxPomodoroBreakMinutes);
        var longBreakMinutes = Math.Clamp(PomodoroLongBreakMinutes, MinPomodoroBreakMinutes, MaxPomodoroBreakMinutes);
        return completedPomodoros > 0 && completedPomodoros % longBreakInterval == 0
            ? longBreakMinutes
            : shortBreakMinutes;
    }

    public void UpsertPomodoroStatsHistoryEntry(string date, int count, int focusMinutes)
    {
        date = NormalizeStatsDate(date);
        count = Math.Max(0, count);
        focusMinutes = Math.Max(0, focusMinutes);
        if (date.Length == 0 || count == 0 && focusMinutes == 0)
        {
            return;
        }

        var existing = PomodoroStatsHistory.FirstOrDefault(entry =>
            string.Equals(entry.Date, date, StringComparison.Ordinal));
        if (existing is null)
        {
            PomodoroStatsHistory.Add(new PomodoroStatsEntry
            {
                Date = date,
                Count = count,
                FocusMinutes = focusMinutes
            });
        }
        else
        {
            existing.Count = count;
            existing.FocusMinutes = focusMinutes;
        }

        PomodoroStatsHistory = NormalizePomodoroStatsHistory(PomodoroStatsHistory);
    }

    public void ResetPomodoroStats(DateTime now, PomodoroStatsResetScope scope = PomodoroStatsResetScope.All)
    {
        var today = now.Date;
        if (scope == PomodoroStatsResetScope.All)
        {
            PomodoroDailyStatsDate = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            PomodoroDailyCount = 0;
            PomodoroDailyFocusMinutes = 0;
            PomodoroStatsHistory = [];
            return;
        }

        PomodoroStatsHistory = NormalizePomodoroStatsHistory(PomodoroStatsHistory);
        UpsertPomodoroStatsHistoryEntry(
            PomodoroDailyStatsDate,
            PomodoroDailyCount,
            PomodoroDailyFocusMinutes);

        var startDate = scope == PomodoroStatsResetScope.Week
            ? GetWeekStart(today)
            : today;
        var endDate = today;

        PomodoroStatsHistory = PomodoroStatsHistory
            .Where(entry =>
            {
                var entryDate = ParseStatsDate(entry.Date);
                return entryDate is null || entryDate < startDate || entryDate > endDate;
            })
            .ToList();

        var dailyDate = ParseStatsDate(PomodoroDailyStatsDate);
        if (dailyDate is not null && dailyDate >= startDate && dailyDate <= endDate)
        {
            PomodoroDailyCount = 0;
            PomodoroDailyFocusMinutes = 0;
        }

        PomodoroDailyStatsDate = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public WidgetPreset CreatePreset(string name)
    {
        var preset = new WidgetPreset
        {
            Name = name.Trim(),
            Width = Width,
            Height = Height,
            FitToContent = FitToContent,
            ClockFontSize = ClockFontSize,
            ClockFontWeight = ClockFontWeight,
            PaddingHorizontal = PaddingHorizontal,
            PaddingTop = PaddingTop,
            PaddingBottom = PaddingBottom,
            BackgroundShade = BackgroundShade,
            BackgroundOpacity = BackgroundOpacity,
            ShowBorder = ShowBorder,
            ShowSeconds = ShowSeconds,
            ShowSideDate = ShowSideDate,
            ShowDate = ShowDate,
            ShowWeekday = ShowWeekday,
            DateFontSize = DateFontSize
        };

        preset.Normalize();
        return preset;
    }

    public void ApplyPreset(WidgetPreset preset)
    {
        preset.Normalize();
        Width = preset.Width;
        Height = preset.Height;
        FitToContent = preset.FitToContent;
        ClockFontSize = preset.ClockFontSize;
        ClockFontWeight = preset.ClockFontWeight;
        PaddingHorizontal = preset.PaddingHorizontal;
        PaddingTop = preset.PaddingTop;
        PaddingBottom = preset.PaddingBottom;
        BackgroundShade = preset.BackgroundShade;
        BackgroundOpacity = preset.BackgroundOpacity;
        ShowBorder = preset.ShowBorder;
        ShowSeconds = preset.ShowSeconds;
        ShowSideDate = preset.ShowSideDate;
        ShowDate = preset.ShowDate;
        ShowWeekday = preset.ShowWeekday;
        DateFontSize = preset.DateFontSize;
        Normalize();
    }

    private static double ClampFinite(double value, double min, double max, double fallback)
    {
        return double.IsFinite(value)
            ? Math.Clamp(value, min, max)
            : fallback;
    }

    private static int SnapFontWeight(int value)
    {
        var clamped = Math.Clamp(value, MinClockFontWeight, MaxClockFontWeight);
        return (int)Math.Round(clamped / 100d) * 100;
    }

    private static WidgetPreset CreateBuiltInPreset(WidgetPreset preset)
    {
        preset.Normalize();
        return preset;
    }

    private static List<PomodoroStatsEntry> NormalizePomodoroStatsHistory(IEnumerable<PomodoroStatsEntry>? entries)
    {
        return (entries ?? [])
            .Select(entry => entry.Clone())
            .Select(entry =>
            {
                entry.Date = NormalizeStatsDate(entry.Date);
                entry.Count = Math.Max(0, entry.Count);
                entry.FocusMinutes = Math.Max(0, entry.FocusMinutes);
                return entry;
            })
            .Where(entry => entry.Date.Length > 0 && (entry.Count > 0 || entry.FocusMinutes > 0))
            .GroupBy(entry => entry.Date, StringComparer.Ordinal)
            .Select(group => new PomodoroStatsEntry
            {
                Date = group.Key,
                Count = group.Sum(entry => entry.Count),
                FocusMinutes = group.Sum(entry => entry.FocusMinutes)
            })
            .OrderBy(entry => entry.Date, StringComparer.Ordinal)
            .ToList();
    }

    private static string NormalizeStatsDate(string? date)
    {
        return ParseStatsDate(date) is { } statsDate
            ? statsDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "";
    }

    private static DateTime? ParseStatsDate(string? date)
    {
        return DateTime.TryParseExact(
            date,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed.Date
            : null;
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysFromMonday);
    }
}

public enum PomodoroStatsResetScope
{
    All,
    Today,
    Week
}

public sealed class PomodoroStatsEntry
{
    public string Date { get; set; } = "";
    public int Count { get; set; }
    public int FocusMinutes { get; set; }

    public PomodoroStatsEntry Clone()
    {
        return new PomodoroStatsEntry
        {
            Date = Date,
            Count = Count,
            FocusMinutes = FocusMinutes
        };
    }
}

public sealed class WidgetPreset
{
    public string Name { get; set; } = "";
    public double Width { get; set; } = 330;
    public double Height { get; set; } = 118;
    public bool FitToContent { get; set; } = true;
    public double ClockFontSize { get; set; } = 56;
    public int ClockFontWeight { get; set; } = 600;
    public double PaddingHorizontal { get; set; } = 18;
    public double PaddingTop { get; set; } = 10;
    public double PaddingBottom { get; set; } = 10;
    public byte BackgroundShade { get; set; } = 20;
    public double BackgroundOpacity { get; set; } = 0.85;
    public bool ShowBorder { get; set; } = true;
    public bool ShowSeconds { get; set; } = true;
    public bool ShowSideDate { get; set; }
    public bool ShowDate { get; set; } = true;
    public bool ShowWeekday { get; set; } = true;
    public double DateFontSize { get; set; } = 13;

    public void Normalize()
    {
        Name = Name.Trim();
        Width = ClampFinite(Width, WidgetSettings.MinWidth, WidgetSettings.MaxWidth, 330);
        Height = ClampFinite(Height, WidgetSettings.MinHeight, WidgetSettings.MaxHeight, 118);
        ClockFontSize = ClampFinite(ClockFontSize, WidgetSettings.MinClockFontSize, WidgetSettings.MaxClockFontSize, 56);
        ClockFontWeight = SnapFontWeight(ClockFontWeight);
        PaddingHorizontal = ClampFinite(PaddingHorizontal, WidgetSettings.MinPadding, WidgetSettings.MaxPadding, 18);
        PaddingTop = ClampFinite(PaddingTop, WidgetSettings.MinPadding, WidgetSettings.MaxPadding, 10);
        PaddingBottom = ClampFinite(PaddingBottom, WidgetSettings.MinPadding, WidgetSettings.MaxPadding, 10);
        BackgroundShade = (byte)Math.Clamp(BackgroundShade, WidgetSettings.MinBackgroundShade, WidgetSettings.MaxBackgroundShade);
        BackgroundOpacity = ClampFinite(BackgroundOpacity, WidgetSettings.MinBackgroundOpacity, WidgetSettings.MaxBackgroundOpacity, 0.85);
        DateFontSize = ClampFinite(DateFontSize, WidgetSettings.MinDateFontSize, WidgetSettings.MaxDateFontSize, 13);
    }

    public WidgetPreset Clone()
    {
        return new WidgetPreset
        {
            Name = Name,
            Width = Width,
            Height = Height,
            FitToContent = FitToContent,
            ClockFontSize = ClockFontSize,
            ClockFontWeight = ClockFontWeight,
            PaddingHorizontal = PaddingHorizontal,
            PaddingTop = PaddingTop,
            PaddingBottom = PaddingBottom,
            BackgroundShade = BackgroundShade,
            BackgroundOpacity = BackgroundOpacity,
            ShowBorder = ShowBorder,
            ShowSeconds = ShowSeconds,
            ShowSideDate = ShowSideDate,
            ShowDate = ShowDate,
            ShowWeekday = ShowWeekday,
            DateFontSize = DateFontSize
        };
    }

    private static double ClampFinite(double value, double min, double max, double fallback)
    {
        return double.IsFinite(value)
            ? Math.Clamp(value, min, max)
            : fallback;
    }

    private static int SnapFontWeight(int value)
    {
        var clamped = Math.Clamp(value, WidgetSettings.MinClockFontWeight, WidgetSettings.MaxClockFontWeight);
        return (int)Math.Round(clamped / 100d) * 100;
    }
}
