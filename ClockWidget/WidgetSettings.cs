namespace ClockWidget;

public sealed class WidgetSettings
{
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
    public bool StartWithWindows { get; set; }
    public bool PomodoroEnabled { get; set; } = true;
    public int PomodoroFocusMinutes { get; set; } = 25;
    public int PomodoroBreakMinutes { get; set; } = 5;
    public bool PomodoroAutoStartBreak { get; set; } = true;
    public bool PomodoroReturnToClockAfterBreak { get; set; } = true;
    public bool PomodoroPlaySound { get; set; } = true;
    public PomodoroSound PomodoroSound { get; set; } = PomodoroSound.FreesoundsNotification;
    public List<WidgetPreset> Presets { get; set; } = [];

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
        return new WidgetPreset
        {
            Name = name,
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

    public void ApplyPreset(WidgetPreset preset)
    {
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
}
