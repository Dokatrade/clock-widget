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

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += (_, _) => SaveSettings();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = LoadSettings();
        _settings.StartWithWindows = ReadStartupSetting();
        ApplySettings();

        _timer.Tick += (_, _) => UpdateClock();
        _timer.Start();
        UpdateClock();
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_settings.LockPosition)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleAlwaysOnTop();
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
        UpdateClock();
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
            SaveSettings();
        };

        if (settingsWindow.ShowDialog() == true)
        {
            _settings = settingsWindow.Settings.Clone();
            ApplyStartupSetting();
            ApplySettings(reposition: false);
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

    private void ToggleAlwaysOnTop()
    {
        _settings.AlwaysOnTop = !_settings.AlwaysOnTop;
        Topmost = _settings.AlwaysOnTop;
        AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;
        SaveSettings();
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;
        TimeText.Text = now.ToString(_settings.ShowSeconds ? "HH:mm:ss" : "HH:mm", CultureInfo.CurrentCulture);
        if (_settings.ShowDate)
        {
            DateText.Text = now.ToString(_settings.ShowWeekday ? "dddd, d MMMM yyyy" : "d MMMM yyyy", CultureInfo.CurrentCulture);
        }
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
