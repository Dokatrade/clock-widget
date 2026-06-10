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
    public bool ShowDate { get; set; } = true;
    public bool ShowWeekday { get; set; } = true;
    public double DateFontSize { get; set; } = 13;
    public bool LockPosition { get; set; }
    public bool SnapToScreenEdges { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool PomodoroEnabled { get; set; } = true;
    public int PomodoroFocusMinutes { get; set; } = 25;
    public int PomodoroBreakMinutes { get; set; } = 5;
    public bool PomodoroAutoStartBreak { get; set; } = true;
    public bool PomodoroReturnToClockAfterBreak { get; set; } = true;
    public bool PomodoroPlaySound { get; set; } = true;
    public PomodoroSound PomodoroSound { get; set; } = PomodoroSound.FreesoundsNotification;
    public List<WidgetPreset> Presets { get; set; } = [];

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
            ShowDate = ShowDate,
            ShowWeekday = ShowWeekday,
            DateFontSize = DateFontSize,
            LockPosition = LockPosition,
            SnapToScreenEdges = SnapToScreenEdges,
            StartWithWindows = StartWithWindows,
            PomodoroEnabled = PomodoroEnabled,
            PomodoroFocusMinutes = PomodoroFocusMinutes,
            PomodoroBreakMinutes = PomodoroBreakMinutes,
            PomodoroAutoStartBreak = PomodoroAutoStartBreak,
            PomodoroReturnToClockAfterBreak = PomodoroReturnToClockAfterBreak,
            PomodoroPlaySound = PomodoroPlaySound,
            PomodoroSound = PomodoroSound,
            Presets = (Presets ?? []).Select(preset => preset.Clone()).ToList()
        };
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
