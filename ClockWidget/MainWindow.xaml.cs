using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ClockWidget;

public partial class MainWindow : Window
{
    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClockWidget");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private WidgetSettings _settings = new();
    private string? _lastSavedJson;
    private WidgetDisplayMode _displayMode = WidgetDisplayMode.Clock;
    private PomodoroPhase _pomodoroPhase = PomodoroPhase.Focus;
    private TimeSpan _pomodoroRemaining = TimeSpan.Zero;
    private DateTime _pomodoroEndsAt;
    private bool _pomodoroRunning;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += (_, _) =>
        {
            SaveSettings();
        };
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = LoadSettings();
        _settings.StartWithWindows = ReadStartupSetting();
        NormalizePomodoroSettings();
        ResetPomodoroState(showClock: true);
        ApplySettings();

        _timer.Tick += (_, _) => UpdateDisplay();
        _timer.Start();
        UpdateDisplay();
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_settings.PomodoroEnabled && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            TogglePomodoroDisplay();
            return;
        }

        if (_settings.LockPosition)
        {
            return;
        }

        try
        {
            DragMove();
        }
        finally
        {
            SaveSettings();
        }
    }

    private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _settings.AlwaysOnTop = AlwaysOnTopMenuItem.IsChecked;
        Topmost = _settings.AlwaysOnTop;
        SaveSettings();
    }

    private void ShowSecondsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _settings.ShowSeconds = ShowSecondsMenuItem.IsChecked;
        UpdateDisplay();
        SaveSettings();
    }

    private void LockPositionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _settings.LockPosition = LockPositionMenuItem.IsChecked;
        SaveSettings();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void PomodoroModeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        TogglePomodoroDisplay();
    }

    private void PomodoroStartPauseMenuItem_Click(object sender, RoutedEventArgs e)
    {
        TogglePomodoroStartPause();
    }

    private void PomodoroResetMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ResetPomodoroState(showClock: false);
        UpdateDisplayPreservingRightEdge();
    }

    private void PomodoroStartPauseButton_Click(object sender, RoutedEventArgs e)
    {
        TogglePomodoroStartPause();
    }

    private void PomodoroResetButton_Click(object sender, RoutedEventArgs e)
    {
        ResetPomodoroState(showClock: false);
        UpdateDisplayPreservingRightEdge();
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _settings.Left = Left;
        _settings.Top = Top;

        var settingsWindow = new SettingsWindow(_settings)
        {
            Owner = this
        };

        settingsWindow.SettingsApplied += (_, updatedSettings) =>
        {
            _settings = updatedSettings.Clone();
            ApplyStartupSetting();
            ApplySettings(reposition: false);
            ApplyPomodoroSettings();
            SaveSettings();
        };

        if (settingsWindow.ShowDialog() == true)
        {
            _settings = settingsWindow.Settings.Clone();
            ApplyStartupSetting();
            ApplySettings(reposition: false);
            ApplyPomodoroSettings();
            SaveSettings();
        }
    }

    private void ApplySettings(bool reposition = true)
    {
        if (_settings.FitToContent)
        {
            SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }
        else
        {
            SizeToContent = System.Windows.SizeToContent.Manual;
            Width = _settings.Width;
            Height = _settings.Height;
        }

        Topmost = _settings.AlwaysOnTop;
        AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;
        ShowSecondsMenuItem.IsChecked = _settings.ShowSeconds;
        LockPositionMenuItem.IsChecked = _settings.LockPosition;
        UpdatePomodoroMenuState();
        ApplyAppearanceSettings();

        if (!reposition)
        {
            return;
        }

        if (IsOnScreen(_settings.Left, _settings.Top))
        {
            Left = _settings.Left;
            Top = _settings.Top;
        }
        else
        {
            Left = SystemParameters.WorkArea.Right - Width - 32;
            Top = SystemParameters.WorkArea.Top + 32;
        }
    }

    private void ApplyAppearanceSettings()
    {
        Root.Padding = new Thickness(
            _settings.PaddingHorizontal,
            _settings.PaddingTop,
            _settings.PaddingHorizontal,
            _settings.PaddingBottom);

        Root.Background = new SolidColorBrush(Color.FromArgb(
            (byte)Math.Clamp((int)Math.Round(_settings.BackgroundOpacity * 255), 0, 255),
            _settings.BackgroundShade,
            _settings.BackgroundShade,
            _settings.BackgroundShade));

        Root.BorderThickness = _settings.ShowBorder ? new Thickness(1) : new Thickness(0);

        TimeText.FontSize = _settings.ClockFontSize;
        TimeText.FontWeight = FontWeight.FromOpenTypeWeight(_settings.ClockFontWeight);
        TimeText.LineHeight = _settings.ClockFontSize;
        DateText.FontSize = _settings.DateFontSize;
        DateText.Visibility = _settings.ShowDate ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateDisplay()
    {
        var displayModeBeforeUpdate = _displayMode;
        var right = Left + ActualWidth;
        var canPreserveRightEdge = IsLoaded
            && !double.IsNaN(Left)
            && !double.IsNaN(right)
            && ActualWidth > 0;

        UpdatePomodoroState();

        if (_displayMode == WidgetDisplayMode.Pomodoro && _settings.PomodoroEnabled)
        {
            UpdatePomodoroDisplay();
        }
        else
        {
            UpdateClockDisplay();
        }

        UpdatePomodoroMenuState();

        if (displayModeBeforeUpdate != _displayMode && canPreserveRightEdge)
        {
            Dispatcher.BeginInvoke(() =>
            {
                UpdateLayout();
                Left = Math.Max(SystemParameters.WorkArea.Left + 12, right - ActualWidth);
            }, DispatcherPriority.Loaded);
        }
    }

    private void UpdateDisplayPreservingRightEdge()
    {
        var right = Left + ActualWidth;
        var canPreserveRightEdge = IsLoaded
            && !double.IsNaN(Left)
            && !double.IsNaN(right)
            && ActualWidth > 0;

        UpdateDisplay();

        if (!canPreserveRightEdge)
        {
            return;
        }

        Dispatcher.BeginInvoke(() =>
        {
            UpdateLayout();
            Left = Math.Max(SystemParameters.WorkArea.Left + 12, right - ActualWidth);
        }, DispatcherPriority.Loaded);
    }

    private void UpdateClockDisplay()
    {
        var now = DateTime.Now;
        PomodoroControls.Visibility = Visibility.Collapsed;
        if (_settings.PomodoroEnabled && _pomodoroRunning)
        {
            PomodoroProgressTrack.Visibility = Visibility.Visible;
            UpdatePomodoroProgress();
        }
        else
        {
            PomodoroProgressTrack.Visibility = Visibility.Collapsed;
            PomodoroProgressFill.Width = 0;
        }

        TimeText.Text = now.ToString(_settings.ShowSeconds ? "HH:mm:ss" : "HH:mm", CultureInfo.CurrentCulture);
        TimeText.Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252));
        DateText.Foreground = new SolidColorBrush(Color.FromArgb(204, 248, 250, 252));
        DateText.Visibility = _settings.ShowDate ? Visibility.Visible : Visibility.Collapsed;

        if (_settings.ShowDate)
        {
            DateText.Text = now.ToString(_settings.ShowWeekday ? "dddd, d MMMM yyyy" : "d MMMM yyyy", CultureInfo.CurrentCulture);
        }
    }

    private void UpdatePomodoroDisplay()
    {
        PomodoroControls.Visibility = Visibility.Visible;
        PomodoroProgressTrack.Visibility = Visibility.Visible;
        PomodoroStartPauseButton.Content = _pomodoroRunning ? "Ⅱ" : "▶";
        UpdatePomodoroProgress();
        TimeText.Text = FormatDuration(_pomodoroRemaining);
        TimeText.Foreground = _pomodoroPhase == PomodoroPhase.Focus
            ? new SolidColorBrush(Color.FromRgb(248, 250, 252))
            : new SolidColorBrush(Color.FromRgb(187, 247, 208));
        DateText.Foreground = _pomodoroRunning
            ? new SolidColorBrush(Color.FromArgb(220, 34, 211, 238))
            : new SolidColorBrush(Color.FromArgb(220, 251, 191, 36));
        var statusText = GetPomodoroStatusText();
        DateText.Text = statusText;
        DateText.Visibility = string.IsNullOrEmpty(statusText) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdatePomodoroProgress()
    {
        var duration = _pomodoroPhase == PomodoroPhase.Focus ? GetFocusDuration() : GetBreakDuration();
        var durationSeconds = duration.TotalSeconds;
        var trackWidth = PomodoroProgressTrack.ActualWidth;
        if (durationSeconds <= 0 || trackWidth <= 0)
        {
            PomodoroProgressFill.Width = 0;
            return;
        }

        var remainingSeconds = Math.Clamp(_pomodoroRemaining.TotalSeconds, 0, durationSeconds);
        var progress = 1 - remainingSeconds / durationSeconds;
        PomodoroProgressFill.Width = Math.Clamp(trackWidth * progress, 0, trackWidth);
        PomodoroProgressFill.Background = new SolidColorBrush(GetPomodoroProgressColor(progress, _pomodoroPhase));
    }

    private static Color GetPomodoroProgressColor(double progress, PomodoroPhase phase)
    {
        var green = Color.FromRgb(34, 197, 94);
        if (phase == PomodoroPhase.Break)
        {
            return green;
        }

        var clampedProgress = Math.Clamp(progress, 0, 1);
        var blue = Color.FromRgb(34, 211, 238);
        var red = Color.FromRgb(248, 64, 64);

        if (clampedProgress < 0.5)
        {
            return LerpColor(green, blue, clampedProgress * 2);
        }

        return LerpColor(blue, red, (clampedProgress - 0.5) * 2);
    }

    private static Color LerpColor(Color start, Color end, double amount)
    {
        return Color.FromRgb(
            Lerp(start.R, end.R, amount),
            Lerp(start.G, end.G, amount),
            Lerp(start.B, end.B, amount));
    }

    private static byte Lerp(byte start, byte end, double amount)
    {
        return (byte)Math.Round(start + (end - start) * amount);
    }

    private void UpdatePomodoroState()
    {
        if (!_pomodoroRunning)
        {
            return;
        }

        var remaining = _pomodoroEndsAt - DateTime.Now;
        if (remaining > TimeSpan.Zero)
        {
            _pomodoroRemaining = remaining;
            return;
        }

        _pomodoroRemaining = TimeSpan.Zero;
        CompletePomodoroPhase();
    }

    private void TogglePomodoroDisplay()
    {
        if (!_settings.PomodoroEnabled)
        {
            return;
        }

        EnsurePomodoroRemaining();
        _displayMode = _displayMode == WidgetDisplayMode.Pomodoro
            ? WidgetDisplayMode.Clock
            : WidgetDisplayMode.Pomodoro;
        UpdateDisplayPreservingRightEdge();
    }

    private void TogglePomodoroStartPause()
    {
        if (!_settings.PomodoroEnabled)
        {
            return;
        }

        EnsurePomodoroRemaining();
        _displayMode = WidgetDisplayMode.Pomodoro;

        if (_pomodoroRunning)
        {
            _pomodoroRemaining = _pomodoroEndsAt - DateTime.Now;
            if (_pomodoroRemaining < TimeSpan.Zero)
            {
                _pomodoroRemaining = TimeSpan.Zero;
            }

            _pomodoroRunning = false;
        }
        else
        {
            _pomodoroEndsAt = DateTime.Now + _pomodoroRemaining;
            _pomodoroRunning = true;
        }

        UpdateDisplayPreservingRightEdge();
    }

    private void CompletePomodoroPhase()
    {
        if (_pomodoroPhase == PomodoroPhase.Focus)
        {
            PlayPomodoroCompletionSound();
            _pomodoroPhase = PomodoroPhase.Break;
            _pomodoroRemaining = GetBreakDuration();
            _pomodoroRunning = _settings.PomodoroAutoStartBreak;
            if (_pomodoroRunning)
            {
                _pomodoroEndsAt = DateTime.Now + _pomodoroRemaining;
            }

            _displayMode = WidgetDisplayMode.Pomodoro;
            return;
        }

        PlayPomodoroCompletionSound();
        ResetPomodoroState(showClock: _settings.PomodoroReturnToClockAfterBreak);
    }

    private void ResetPomodoroState(bool showClock)
    {
        _pomodoroPhase = PomodoroPhase.Focus;
        _pomodoroRemaining = GetFocusDuration();
        _pomodoroRunning = false;
        _displayMode = showClock ? WidgetDisplayMode.Clock : WidgetDisplayMode.Pomodoro;
    }

    private void ApplyPomodoroSettings()
    {
        NormalizePomodoroSettings();

        if (!_settings.PomodoroEnabled)
        {
            ResetPomodoroState(showClock: true);
            UpdateDisplay();
            return;
        }

        if (!_pomodoroRunning && _pomodoroPhase == PomodoroPhase.Focus)
        {
            _pomodoroRemaining = GetFocusDuration();
        }

        UpdateDisplay();
    }

    private void NormalizePomodoroSettings()
    {
        _settings.PomodoroFocusMinutes = Math.Clamp(_settings.PomodoroFocusMinutes, 1, 120);
        _settings.PomodoroBreakMinutes = Math.Clamp(_settings.PomodoroBreakMinutes, 1, 60);
    }

    private void EnsurePomodoroRemaining()
    {
        if (_pomodoroRemaining <= TimeSpan.Zero)
        {
            _pomodoroRemaining = _pomodoroPhase == PomodoroPhase.Focus
                ? GetFocusDuration()
                : GetBreakDuration();
        }
    }

    private TimeSpan GetFocusDuration()
    {
        return TimeSpan.FromMinutes(_settings.PomodoroFocusMinutes);
    }

    private TimeSpan GetBreakDuration()
    {
        return TimeSpan.FromMinutes(_settings.PomodoroBreakMinutes);
    }

    private string GetPomodoroStatusText()
    {
        return "";
    }

    private void PlayPomodoroCompletionSound()
    {
        if (_settings.PomodoroPlaySound)
        {
            PomodoroBell.Play(_settings.PomodoroSound);
        }
    }

    private void UpdatePomodoroMenuState()
    {
        PomodoroModeMenuItem.IsEnabled = _settings.PomodoroEnabled;
        PomodoroStartPauseMenuItem.IsEnabled = _settings.PomodoroEnabled;
        PomodoroResetMenuItem.IsEnabled = _settings.PomodoroEnabled;
        PomodoroModeMenuItem.IsChecked = _displayMode == WidgetDisplayMode.Pomodoro;
        PomodoroStartPauseMenuItem.Header = GetPomodoroStartPauseHeader();
    }

    private string GetPomodoroStartPauseHeader()
    {
        if (_pomodoroRunning)
        {
            return "Pause Pomodoro";
        }

        var phase = _pomodoroPhase == PomodoroPhase.Focus ? "Pomodoro" : "Break";
        var fullDuration = _pomodoroPhase == PomodoroPhase.Focus ? GetFocusDuration() : GetBreakDuration();
        return _pomodoroRemaining < fullDuration ? $"Resume {phase}" : $"Start {phase}";
    }

    private static string FormatDuration(TimeSpan duration)
    {
        var totalSeconds = Math.Max(0, (int)Math.Ceiling(duration.TotalSeconds));
        var normalized = TimeSpan.FromSeconds(totalSeconds);
        return normalized.TotalHours >= 1
            ? normalized.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : normalized.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    private static bool IsOnScreen(double left, double top)
    {
        const double margin = 40;
        return left >= SystemParameters.VirtualScreenLeft - margin
            && top >= SystemParameters.VirtualScreenTop - margin
            && left <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - margin
            && top <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - margin;
    }

    private WidgetSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new WidgetSettings();
            }

            _lastSavedJson = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<WidgetSettings>(_lastSavedJson) ?? new WidgetSettings();
        }
        catch
        {
            _lastSavedJson = null;
            return new WidgetSettings();
        }
    }

    private static bool ReadStartupSetting()
    {
        try
        {
            return StartupManager.IsEnabled();
        }
        catch
        {
            return false;
        }
    }

    private void ApplyStartupSetting()
    {
        try
        {
            StartupManager.SetEnabled(_settings.StartWithWindows);
        }
        catch (Exception ex)
        {
            _settings.StartWithWindows = ReadStartupSetting();
            MessageBox.Show(
                this,
                $"Could not update Windows startup setting.\n\n{ex.Message}",
                "Clock Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void SaveSettings()
    {
        _settings.Left = Left;
        _settings.Top = Top;

        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        if (json == _lastSavedJson)
        {
            return;
        }

        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsPath, json);
        _lastSavedJson = json;
    }
}
