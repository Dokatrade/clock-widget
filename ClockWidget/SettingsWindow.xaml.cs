using System.Windows;

namespace ClockWidget;

public partial class SettingsWindow : Window
{
    private static readonly IReadOnlyList<PomodoroSoundOption> PomodoroSoundOptions =
    [
        new("Notification chime", PomodoroSound.FreesoundsNotification),
        new("Harp", PomodoroSound.Harp),
        new("Ladder", PomodoroSound.Ladder),
        new("Music box", PomodoroSound.MusicBox),
        new("Message notification", PomodoroSound.MessageNotification),
        new("New notification 015", PomodoroSound.NewNotification015),
        new("New notification 036", PomodoroSound.NewNotification036),
        new("New notification 059", PomodoroSound.NewNotification059)
    ];

    public event EventHandler<WidgetSettings>? SettingsApplied;

    public WidgetSettings Settings { get; private set; }

    private bool _isLoading;

    public SettingsWindow(WidgetSettings settings)
    {
        InitializeComponent();
        PomodoroSoundComboBox.ItemsSource = PomodoroSoundOptions;
        PomodoroSoundComboBox.DisplayMemberPath = nameof(PomodoroSoundOption.Name);
        PomodoroSoundComboBox.SelectedValuePath = nameof(PomodoroSoundOption.Sound);
        Settings = settings.Clone();
        LoadSettingsToControls();
    }

    private void LoadSettingsToControls()
    {
        _isLoading = true;

        ShowBorderCheckBox.IsChecked = Settings.ShowBorder;
        ShowDateCheckBox.IsChecked = Settings.ShowDate;
        ShowWeekdayCheckBox.IsChecked = Settings.ShowWeekday;
        DateFontSizeSlider.Value = Settings.DateFontSize;
        BackgroundShadeSlider.Value = Settings.BackgroundShade;
        BackgroundOpacitySlider.Value = Settings.BackgroundOpacity * 100;
        PaddingHorizontalSlider.Value = Settings.PaddingHorizontal;
        PaddingTopSlider.Value = Settings.PaddingTop;
        PaddingBottomSlider.Value = Settings.PaddingBottom;
        FitToContentCheckBox.IsChecked = Settings.FitToContent;
        WidthSlider.Value = Settings.Width;
        HeightSlider.Value = Settings.Height;
        ClockFontSizeSlider.Value = Settings.ClockFontSize;
        ClockFontWeightSlider.Value = Settings.ClockFontWeight;
        StartWithWindowsCheckBox.IsChecked = Settings.StartWithWindows;
        PomodoroEnabledCheckBox.IsChecked = Settings.PomodoroEnabled;
        PomodoroFocusMinutesSlider.Value = Settings.PomodoroFocusMinutes;
        PomodoroBreakMinutesSlider.Value = Settings.PomodoroBreakMinutes;
        PomodoroAutoStartBreakCheckBox.IsChecked = Settings.PomodoroAutoStartBreak;
        PomodoroReturnToClockCheckBox.IsChecked = Settings.PomodoroReturnToClockAfterBreak;
        PomodoroPlaySoundCheckBox.IsChecked = Settings.PomodoroPlaySound;
        PomodoroSoundComboBox.SelectedValue = Settings.PomodoroSound;

        UpdatePresetList();
        _isLoading = false;
        UpdateLabels();
    }

    private void ApplyControlsToSettings()
    {
        Settings.ShowBorder = ShowBorderCheckBox.IsChecked == true;
        Settings.ShowDate = ShowDateCheckBox.IsChecked == true;
        Settings.ShowWeekday = ShowWeekdayCheckBox.IsChecked == true;
        Settings.DateFontSize = Math.Round(DateFontSizeSlider.Value);
        Settings.BackgroundShade = (byte)Math.Round(BackgroundShadeSlider.Value);
        Settings.BackgroundOpacity = Math.Round(BackgroundOpacitySlider.Value) / 100;
        Settings.PaddingHorizontal = Math.Round(PaddingHorizontalSlider.Value, 1);
        Settings.PaddingTop = Math.Round(PaddingTopSlider.Value, 1);
        Settings.PaddingBottom = Math.Round(PaddingBottomSlider.Value, 1);
        Settings.FitToContent = FitToContentCheckBox.IsChecked == true;
        Settings.Width = Math.Round(WidthSlider.Value);
        Settings.Height = Math.Round(HeightSlider.Value);
        Settings.ClockFontSize = Math.Round(ClockFontSizeSlider.Value);
        Settings.ClockFontWeight = (int)Math.Round(ClockFontWeightSlider.Value);
        Settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
        Settings.PomodoroEnabled = PomodoroEnabledCheckBox.IsChecked == true;
        Settings.PomodoroFocusMinutes = (int)Math.Round(PomodoroFocusMinutesSlider.Value);
        Settings.PomodoroBreakMinutes = (int)Math.Round(PomodoroBreakMinutesSlider.Value);
        Settings.PomodoroAutoStartBreak = PomodoroAutoStartBreakCheckBox.IsChecked == true;
        Settings.PomodoroReturnToClockAfterBreak = PomodoroReturnToClockCheckBox.IsChecked == true;
        Settings.PomodoroPlaySound = PomodoroPlaySoundCheckBox.IsChecked == true;
        Settings.PomodoroSound = GetSelectedPomodoroSound();
    }

    private void UpdateLabels()
    {
        BackgroundShadeLabel.Text = $"Black shade: {BackgroundShadeSlider.Value:0}";
        BackgroundOpacityLabel.Text = $"Background opacity: {BackgroundOpacitySlider.Value:0}%";
        DateFontSizeLabel.Text = $"Date font size: {DateFontSizeSlider.Value:0}px";
        PaddingHorizontalLabel.Text = $"Side padding: {PaddingHorizontalSlider.Value:0.#}px";
        PaddingTopLabel.Text = $"Top padding: {PaddingTopSlider.Value:0.#}px";
        PaddingBottomLabel.Text = $"Bottom padding: {PaddingBottomSlider.Value:0.#}px";
        WidthLabel.Text = $"Width: {WidthSlider.Value:0}px";
        HeightLabel.Text = $"Height: {HeightSlider.Value:0}px";
        ClockFontSizeLabel.Text = $"Clock font size: {ClockFontSizeSlider.Value:0}px";
        ClockFontWeightLabel.Text = $"Clock font weight: {GetFontWeightName(ClockFontWeightSlider.Value)}";
        PomodoroFocusMinutesLabel.Text = $"Focus minutes: {PomodoroFocusMinutesSlider.Value:0}";
        PomodoroBreakMinutesLabel.Text = $"Break minutes: {PomodoroBreakMinutesSlider.Value:0}";
        UpdatePomodoroControlState();
        UpdateDateControlState();
        UpdateSizeControlState();
    }

    private static string GetFontWeightName(double value)
    {
        return (int)Math.Round(value) switch
        {
            300 => "Light",
            400 => "Regular",
            500 => "Medium",
            600 => "SemiBold",
            700 => "Bold",
            800 => "ExtraBold",
            900 => "Black",
            var weight => weight.ToString()
        };
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdateLabels();
        }
    }

    private void FitToContentCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdateSizeControlState();
        }
    }

    private void ShowDateCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdateDateControlState();
        }
    }

    private void PomodoroEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdatePomodoroControlState();
        }
    }

    private void UpdateDateControlState()
    {
        var showDate = ShowDateCheckBox.IsChecked == true;
        ShowWeekdayCheckBox.IsEnabled = showDate;
        DateFontSizeSlider.IsEnabled = showDate;
    }

    private void UpdateSizeControlState()
    {
        var useManualSize = FitToContentCheckBox.IsChecked != true;
        WidthSlider.IsEnabled = useManualSize;
        HeightSlider.IsEnabled = useManualSize;
    }

    private void UpdatePomodoroControlState()
    {
        var enabled = PomodoroEnabledCheckBox.IsChecked == true;
        PomodoroFocusMinutesSlider.IsEnabled = enabled;
        PomodoroBreakMinutesSlider.IsEnabled = enabled;
        PomodoroAutoStartBreakCheckBox.IsEnabled = enabled;
        PomodoroReturnToClockCheckBox.IsEnabled = enabled;
        PomodoroPlaySoundCheckBox.IsEnabled = enabled;
        PomodoroSoundComboBox.IsEnabled = enabled;
        PomodoroSoundPreviewButton.IsEnabled = enabled;
    }

    private void PomodoroSoundPreviewButton_Click(object sender, RoutedEventArgs e)
    {
        PomodoroBell.Play(GetSelectedPomodoroSound());
    }

    private PomodoroSound GetSelectedPomodoroSound()
    {
        return PomodoroSoundComboBox.SelectedValue is PomodoroSound sound
            ? sound
            : PomodoroSound.FreesoundsNotification;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyControlsToSettings();
        DialogResult = true;
    }

    private sealed record PomodoroSoundOption(string Name, PomodoroSound Sound);

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyControlsToSettings();
        SettingsApplied?.Invoke(this, Settings.Clone());
    }

    private void DefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        Settings.ResetAppearanceToDefaults();
        LoadSettingsToControls();
    }

    private void PresetComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isLoading || PresetComboBox.SelectedItem is not string presetName)
        {
            return;
        }

        PresetNameTextBox.Text = presetName;
    }

    private void SavePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var presetName = PresetNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(presetName))
        {
            MessageBox.Show(this, "Enter preset name first.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        ApplyControlsToSettings();

        var existingPreset = Settings.Presets.FirstOrDefault(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));

        if (existingPreset is not null)
        {
            Settings.Presets.Remove(existingPreset);
        }

        Settings.Presets.Add(Settings.CreatePreset(presetName));
        Settings.Presets = Settings.Presets.OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase).ToList();

        UpdatePresetList(presetName);
        SettingsApplied?.Invoke(this, Settings.Clone());
    }

    private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
    {
        var preset = GetSelectedPreset();
        if (preset is null)
        {
            MessageBox.Show(this, "Select a preset to load.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Settings.ApplyPreset(preset);
        LoadSettingsToControls();
        PresetComboBox.SelectedItem = preset.Name;
        PresetNameTextBox.Text = preset.Name;
        SettingsApplied?.Invoke(this, Settings.Clone());
    }

    private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var preset = GetSelectedPreset();
        if (preset is null)
        {
            MessageBox.Show(this, "Select a preset to delete.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Settings.Presets.Remove(preset);
        PresetNameTextBox.Clear();
        UpdatePresetList();
        SettingsApplied?.Invoke(this, Settings.Clone());
    }

    private WidgetPreset? GetSelectedPreset()
    {
        var presetName = PresetComboBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(presetName))
        {
            presetName = PresetNameTextBox.Text.Trim();
        }

        return Settings.Presets.FirstOrDefault(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));
    }

    private void UpdatePresetList(string? selectedPresetName = null)
    {
        var presetNames = Settings.Presets
            .OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase)
            .Select(preset => preset.Name)
            .ToList();

        PresetComboBox.ItemsSource = presetNames;
        PresetComboBox.SelectedItem = selectedPresetName;
    }
}
