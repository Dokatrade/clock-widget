using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MediaColor = System.Windows.Media.Color;
using WpfMessageBox = System.Windows.MessageBox;

namespace ClockWidget;

public partial class MainWindow : Window
{
    private static readonly SolidColorBrush ClockTextBrush = CreateFrozenBrush(MediaColor.FromRgb(248, 250, 252));
    private static readonly SolidColorBrush DateTextBrush = CreateFrozenBrush(MediaColor.FromArgb(204, 248, 250, 252));
    private static readonly SolidColorBrush PomodoroBreakTextBrush = CreateFrozenBrush(MediaColor.FromRgb(187, 247, 208));
    private static readonly MediaColor PomodoroBreakProgressColor = MediaColor.FromRgb(34, 197, 94);
    private static readonly SolidColorBrush PomodoroBreakProgressBrush = CreateFrozenBrush(PomodoroBreakProgressColor);

    private readonly SettingsStore _settingsStore = new();
    private readonly StartupSettingsService _startupSettingsService = new();
    private readonly SettingsDialogController _settingsDialogController = new();
    private readonly WidgetDisplayFormatter _displayFormatter = new();
    private readonly DisplayTickScheduler _tickScheduler;
    private readonly PomodoroSession _pomodoroSession = new();
    private WidgetSettings _settings = new();
    private TrayIconController? _trayIcon;

    public MainWindow()
    {
        InitializeComponent();
        _tickScheduler = new DisplayTickScheduler(UpdateDisplayAndScheduleNextTick);
        Loaded += MainWindow_Loaded;
        Closing += (_, _) =>
        {
            _tickScheduler.Stop();
            _trayIcon?.Dispose();
            SaveSettings();
        };
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = _settingsStore.Load();
        _settings.StartWithWindows = _startupSettingsService.ReadEnabled();
        _settings.Normalize();
        ResetPomodoroDailyStatsIfNeeded(DateTime.Now);
        ResetPomodoroState(showClock: true);
        ApplySettings();

        InitializeTrayIcon();
        SizeChanged += (_, _) => ScheduleEnsureWindowOnScreen();
        UpdateDisplayAndScheduleNextTick();
        _tickScheduler.Start();
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Focus();

        if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            ToggleSideDateFromClockShortcut();
            return;
        }

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
            if (!WindowPlacementService.SnapToScreenEdges(
                this,
                _settings.Width,
                _settings.Height,
                _settings.SnapToScreenEdges))
            {
                EnsureWindowOnScreen();
            }

            SaveSettings();
        }
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Space
            || !_pomodoroSession.IsPomodoroDisplayVisible(_settings)
            || !Root.IsMouseOver)
        {
            return;
        }

        TogglePomodoroStartPause();
        e.Handled = true;
    }

    private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SetAlwaysOnTop(AlwaysOnTopMenuItem.IsChecked);
    }

    private void ShowSecondsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _settings.ShowSeconds = ShowSecondsMenuItem.IsChecked;
        UpdateDisplayAndScheduleNextTick();
        SaveSettings();
    }

    private void ShowSideDateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SetSideDateVisible(ShowSideDateMenuItem.IsChecked);
    }

    private void ToggleSideDateFromClockShortcut()
    {
        if (_pomodoroSession.IsPomodoroDisplayVisible(_settings))
        {
            return;
        }

        SetSideDateVisible(!_settings.ShowSideDate);
    }

    private void SetSideDateVisible(bool visible)
    {
        _settings.ShowSideDate = visible;
        ShowSideDateMenuItem.IsChecked = visible;
        UpdateDisplayPreservingRightEdge();
        SaveSettings();
    }

    private void SetAlwaysOnTop(bool enabled)
    {
        _settings.AlwaysOnTop = enabled;
        Topmost = enabled;
        AlwaysOnTopMenuItem.IsChecked = enabled;
        SaveSettings();
        UpdateTrayMenuState();
    }

    private void SetLockPosition(bool enabled)
    {
        _settings.LockPosition = enabled;
        LockPositionMenuItem.IsChecked = enabled;
        SaveSettings();
        UpdateTrayMenuState();
    }

    private void LockPositionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SetLockPosition(LockPositionMenuItem.IsChecked);
    }

    private void ResetPositionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ResetPosition();
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
        ResetVisiblePomodoroState();
    }

    private void PomodoroStatsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        OpenPomodoroStats();
    }

    private void PomodoroStartPauseButton_Click(object sender, RoutedEventArgs e)
    {
        TogglePomodoroStartPause();
    }

    private void PomodoroResetButton_Click(object sender, RoutedEventArgs e)
    {
        ResetVisiblePomodoroState();
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _settings.Left = Left;
        _settings.Top = Top;
        _settingsDialogController.Show(this, _settings, ApplyUpdatedSettings);
    }

    private void ApplyUpdatedSettings(WidgetSettings updatedSettings)
    {
        _settings = updatedSettings.Clone();
        ApplyStartupSetting();
        ApplySettings(reposition: false);
        ApplyPomodoroSettings();
        SaveSettings();
    }

    private void ApplySettings(bool reposition = true)
    {
        _settings.Normalize();

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
        ShowSideDateMenuItem.IsChecked = _settings.ShowSideDate;
        LockPositionMenuItem.IsChecked = _settings.LockPosition;
        UpdatePomodoroMenuState();
        ApplyAppearanceSettings();

        if (reposition)
        {
            if (WindowPlacementService.IsOnScreen(_settings.Left, _settings.Top))
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

        ScheduleEnsureWindowOnScreen();
    }

    private void ApplyAppearanceSettings()
    {
        Root.Padding = new Thickness(
            _settings.PaddingHorizontal,
            _settings.PaddingTop,
            _settings.PaddingHorizontal,
            _settings.PaddingBottom);

        Root.Background = new SolidColorBrush(MediaColor.FromArgb(
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
        var statsFontSize = Math.Clamp(_settings.ClockFontSize * 0.38, 16, 34);
        PomodoroDailyCountText.FontSize = statsFontSize;
        PomodoroDailyFocusMinutesText.FontSize = statsFontSize;
        var sideDateFontSize = Math.Clamp(_settings.ClockFontSize * 0.46, 20, 40);
        var sideDateLineHeight = Math.Min(_settings.ClockFontSize / 2, sideDateFontSize * 1.08);
        ClockSideDate.Height = _settings.ClockFontSize;
        ClockSideDateDayText.FontSize = sideDateFontSize;
        ClockSideDateMonthText.FontSize = sideDateFontSize;
        ClockSideDateDayText.LineHeight = sideDateLineHeight;
        ClockSideDateMonthText.LineHeight = sideDateLineHeight;
    }

    private void UpdateDisplay()
    {
        var displayModeBeforeUpdate = _pomodoroSession.DisplayMode;
        var right = Left + ActualWidth;
        var canPreserveRightEdge = IsLoaded
            && !double.IsNaN(Left)
            && !double.IsNaN(right)
            && ActualWidth > 0;

        ResetPomodoroDailyStatsIfNeeded(DateTime.Now);
        UpdatePomodoroState();

        if (_pomodoroSession.IsPomodoroDisplayVisible(_settings))
        {
            UpdatePomodoroDisplay();
        }
        else
        {
            UpdateClockDisplay();
        }

        UpdatePomodoroMenuState();

        if (displayModeBeforeUpdate != _pomodoroSession.DisplayMode && canPreserveRightEdge)
        {
            RestoreRightEdge(right);
        }
    }

    private void UpdateDisplayAndScheduleNextTick()
    {
        UpdateDisplay();
        _tickScheduler.ScheduleNext(GetDisplayTickState());
    }

    private void UpdateDisplayPreservingRightEdge()
    {
        var right = Left + ActualWidth;
        var canPreserveRightEdge = IsLoaded
            && !double.IsNaN(Left)
            && !double.IsNaN(right)
            && ActualWidth > 0;

        UpdateDisplayAndScheduleNextTick();

        if (!canPreserveRightEdge)
        {
            return;
        }

        RestoreRightEdge(right);
    }

    private void UpdateClockDisplay()
    {
        var display = _displayFormatter.BuildClockDisplay(
            DateTime.Now,
            _settings,
            _pomodoroSession.Controller,
            GetFocusDuration(),
            GetBreakDuration());

        PomodoroControls.Visibility = Visibility.Collapsed;
        PomodoroDailyStats.Visibility = Visibility.Collapsed;
        ClockSideDate.Visibility = display.ShowSideDate ? Visibility.Visible : Visibility.Collapsed;
        ClockSideDateDayText.Text = display.SideDateDayText;
        ClockSideDateMonthText.Text = display.SideDateMonthText;
        ApplyPomodoroProgress(display.Progress);
        TimeText.Text = display.TimeText;
        TimeText.Foreground = ClockTextBrush;
        DateText.Foreground = DateTextBrush;
        DateText.Visibility = display.ShowDate ? Visibility.Visible : Visibility.Collapsed;
        DateText.Text = display.DateText;
    }

    private void UpdatePomodoroDisplay()
    {
        var display = _displayFormatter.BuildPomodoroDisplay(
            _pomodoroSession.Controller,
            GetFocusDuration(),
            GetBreakDuration());

        PomodoroControls.Visibility = Visibility.Visible;
        ClockSideDate.Visibility = Visibility.Collapsed;
        UpdatePomodoroDailyStats();
        PomodoroStartPauseButton.Content = display.StartPauseText;
        ApplyPomodoroProgress(display.Progress);
        TimeText.Text = display.TimeText;
        TimeText.Foreground = display.UseBreakTextColor
            ? PomodoroBreakTextBrush
            : ClockTextBrush;
        DateText.Text = "";
        DateText.Visibility = Visibility.Collapsed;
    }

    private void ApplyPomodoroProgress(PomodoroProgressModel progress)
    {
        if (progress.IsVisible)
        {
            PomodoroProgressTrack.Visibility = Visibility.Visible;
            UpdatePomodoroProgress(progress);
        }
        else
        {
            PomodoroProgressTrack.Visibility = Visibility.Collapsed;
            PomodoroProgressFill.Width = 0;
        }
    }

    private void UpdatePomodoroProgress(PomodoroProgressModel progress)
    {
        var trackWidth = PomodoroProgressTrack.ActualWidth;
        if (trackWidth <= 0)
        {
            PomodoroProgressFill.Width = 0;
            return;
        }

        PomodoroProgressFill.Width = Math.Clamp(trackWidth * progress.Ratio, 0, trackWidth);
        PomodoroProgressFill.Background = progress.Color == PomodoroBreakProgressColor
            ? PomodoroBreakProgressBrush
            : CreateFrozenBrush(progress.Color);
    }

    private static SolidColorBrush CreateFrozenBrush(MediaColor color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private void UpdatePomodoroState()
    {
        var completion = _pomodoroSession.Update(GetBreakDurationForNextTransition(), _settings.PomodoroAutoStartBreak);
        if (completion == PomodoroPhaseCompletion.None)
        {
            return;
        }

        CompletePomodoroPhase(completion);
    }

    private void TogglePomodoroDisplay()
    {
        if (!_pomodoroSession.ToggleDisplay(_settings, GetFocusDuration(), GetBreakDuration()))
        {
            return;
        }

        UpdateDisplayPreservingRightEdge();
    }

    private void TogglePomodoroStartPause()
    {
        if (!_pomodoroSession.ToggleStartPause(_settings, GetFocusDuration(), GetBreakDuration()))
        {
            return;
        }

        UpdateDisplayPreservingRightEdge();
    }

    private void ResetVisiblePomodoroState()
    {
        ResetPomodoroState(showClock: false);
        UpdateDisplayPreservingRightEdge();
    }

    private void CompletePomodoroPhase(PomodoroPhaseCompletion completion)
    {
        if (completion == PomodoroPhaseCompletion.FocusCompleted)
        {
            RecordPomodoroFocusCompletion();
        }

        PlayPomodoroCompletionSound();

        _pomodoroSession.CompletePhase(
            completion,
            GetFocusDuration(),
            _settings.PomodoroReturnToClockAfterBreak);
    }

    private void ResetPomodoroState(bool showClock)
    {
        _pomodoroSession.Reset(GetFocusDuration(), showClock);
    }

    private void ApplyPomodoroSettings()
    {
        _settings.Normalize();
        ResetPomodoroDailyStatsIfNeeded(DateTime.Now);
        _pomodoroSession.ApplySettings(_settings, GetFocusDuration());
        UpdateDisplayAndScheduleNextTick();
    }

    private void UpdatePomodoroDailyStats()
    {
        if (!_settings.ShowPomodoroDailyStats)
        {
            PomodoroDailyStats.Visibility = Visibility.Collapsed;
            return;
        }

        PomodoroDailyCountText.Text = _settings.PomodoroDailyCount.ToString(CultureInfo.InvariantCulture);
        PomodoroDailyFocusMinutesText.Text = $"{_settings.PomodoroDailyFocusMinutes.ToString(CultureInfo.InvariantCulture)}m";
        PomodoroDailyStats.Visibility = Visibility.Visible;
    }

    private void RecordPomodoroFocusCompletion()
    {
        ResetPomodoroDailyStatsIfNeeded(DateTime.Now);
        _settings.PomodoroDailyCount++;
        _settings.PomodoroDailyFocusMinutes += (int)Math.Round(GetFocusDuration().TotalMinutes);
        _settings.UpsertPomodoroStatsHistoryEntry(
            _settings.PomodoroDailyStatsDate,
            _settings.PomodoroDailyCount,
            _settings.PomodoroDailyFocusMinutes);
        SaveSettings();
    }

    private void ResetPomodoroDailyStatsIfNeeded(DateTime now)
    {
        var today = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (string.Equals(_settings.PomodoroDailyStatsDate, today, StringComparison.Ordinal))
        {
            return;
        }

        _settings.UpsertPomodoroStatsHistoryEntry(
            _settings.PomodoroDailyStatsDate,
            _settings.PomodoroDailyCount,
            _settings.PomodoroDailyFocusMinutes);

        _settings.PomodoroDailyStatsDate = today;
        var todayEntry = _settings.PomodoroStatsHistory.FirstOrDefault(entry =>
            string.Equals(entry.Date, today, StringComparison.Ordinal));
        _settings.PomodoroDailyCount = todayEntry?.Count ?? 0;
        _settings.PomodoroDailyFocusMinutes = todayEntry?.FocusMinutes ?? 0;
    }

    private void OpenPomodoroStats()
    {
        ResetPomodoroDailyStatsIfNeeded(DateTime.Now);
        UpdatePomodoroState();
        UpdateDisplayAndScheduleNextTick();

        var statsWindow = new PomodoroStatsWindow(
            _settings,
            _pomodoroSession.Controller,
            ResetPomodoroStats)
        {
            Owner = this
        };

        statsWindow.ShowDialog();
    }

    private void ResetPomodoroStats()
    {
        _settings.ResetPomodoroStats(DateTime.Now);
        SaveSettings();
        UpdateDisplayAndScheduleNextTick();
    }

    private TimeSpan GetFocusDuration()
    {
        return TimeSpan.FromMinutes(_settings.PomodoroFocusMinutes);
    }

    private TimeSpan GetBreakDuration()
    {
        return TimeSpan.FromMinutes(_settings.GetBreakMinutesForCompletedPomodoros(_settings.PomodoroDailyCount));
    }

    private TimeSpan GetBreakDurationForNextTransition()
    {
        var completedPomodoros = _pomodoroSession.Controller.Phase == PomodoroPhase.Focus
            ? _settings.PomodoroDailyCount + 1
            : _settings.PomodoroDailyCount;
        return TimeSpan.FromMinutes(_settings.GetBreakMinutesForCompletedPomodoros(completedPomodoros));
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
        PomodoroModeMenuItem.IsChecked = _pomodoroSession.IsPomodoroDisplayVisible(_settings);
        PomodoroStartPauseMenuItem.Header = GetPomodoroStartPauseHeader();
    }

    private string GetPomodoroStartPauseHeader()
    {
        return _pomodoroSession.GetStartPauseHeader(GetFocusDuration(), GetBreakDuration());
    }

    private DisplayTickState GetDisplayTickState()
    {
        return _pomodoroSession.GetTickState(_settings);
    }

    private void InitializeTrayIcon()
    {
        if (_trayIcon is not null)
        {
            return;
        }

        _trayIcon = new TrayIconController(
            Dispatcher,
            new TrayIconActions(
                GetState: GetTrayIconState,
                ToggleWidgetVisibility: ToggleWidgetVisibilityWithoutSaving,
                OpenSettings: OpenSettingsFromTray,
                ToggleAlwaysOnTop: ToggleAlwaysOnTopFromTray,
                ToggleLockPosition: ToggleLockPositionFromTray,
                ResetPosition: ResetPositionFromTray,
                TogglePomodoroDisplay: TogglePomodoroDisplay,
                TogglePomodoroStartPause: TogglePomodoroStartPause,
                ResetPomodoro: ResetPomodoroFromTray,
                Exit: Close));
        UpdateTrayMenuState();
    }

    private void UpdateTrayMenuState()
    {
        _trayIcon?.UpdateState(GetTrayIconState());
    }

    private TrayIconState GetTrayIconState()
    {
        return new TrayIconState(
            IsWidgetVisible: IsVisible,
            AlwaysOnTop: _settings.AlwaysOnTop,
            LockPosition: _settings.LockPosition,
            PomodoroEnabled: _settings.PomodoroEnabled,
            IsPomodoroVisible: _pomodoroSession.IsPomodoroDisplayVisible(_settings),
            PomodoroStartPauseText: GetPomodoroStartPauseHeader());
    }

    private void ToggleWidgetVisibilityWithoutSaving()
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            ShowWidgetWithoutSaving();
        }

        UpdateTrayMenuState();
    }

    private void ShowWidgetWithoutSaving()
    {
        Show();
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        EnsureWindowOnScreen();
        Activate();
    }

    private void OpenSettingsFromTray()
    {
        ShowWidgetWithoutSaving();
        SettingsMenuItem_Click(this, new RoutedEventArgs());
    }

    private void ResetPomodoroFromTray()
    {
        ResetVisiblePomodoroState();
    }

    private void ResetPositionFromTray()
    {
        ShowWidgetWithoutSaving();
        ResetPosition();
    }

    private void ToggleAlwaysOnTopFromTray()
    {
        SetAlwaysOnTop(!_settings.AlwaysOnTop);
    }

    private void ToggleLockPositionFromTray()
    {
        SetLockPosition(!_settings.LockPosition);
    }

    private void RestoreRightEdge(double right)
    {
        Dispatcher.BeginInvoke(() =>
        {
            WindowPlacementService.RestoreRightEdge(this, right, _settings.Width, _settings.Height);
        }, DispatcherPriority.Loaded);
    }

    private void ScheduleEnsureWindowOnScreen()
    {
        if (!IsLoaded)
        {
            return;
        }

        Dispatcher.BeginInvoke(() =>
        {
            UpdateLayout();
            EnsureWindowOnScreen();
        }, DispatcherPriority.Loaded);
    }

    private bool EnsureWindowOnScreen()
    {
        return WindowPlacementService.EnsureOnScreen(this, _settings.Width, _settings.Height);
    }

    private void ResetPosition()
    {
        WindowPlacementService.MoveToDefaultPosition(this, _settings.Width, _settings.Height);
        SaveSettings();
    }

    private void ApplyStartupSetting()
    {
        var result = _startupSettingsService.ApplyEnabled(_settings.StartWithWindows);
        if (result.Succeeded)
        {
            return;
        }

        _settings.StartWithWindows = result.CurrentEnabled;
        WpfMessageBox.Show(
            this,
            $"Could not update Windows startup setting.\n\n{result.ErrorMessage}",
            "Clock Settings",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private void SaveSettings()
    {
        _settings.Left = Left;
        _settings.Top = Top;
        _settingsStore.Save(_settings);
    }
}
