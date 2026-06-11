using System.IO;
using System.Windows;
using OpenSettingsFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveSettingsFileDialog = Microsoft.Win32.SaveFileDialog;
using WpfMessageBox = System.Windows.MessageBox;

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

    private readonly SettingsPresetCatalog _presetCatalog = new();
    private bool _isLoading;
    private bool _hasPendingChanges;
    private string _lastAppliedSettingsJson = "";

    public SettingsWindow(WidgetSettings settings)
    {
        InitializeComponent();
        PresetComboBox.DisplayMemberPath = nameof(SettingsPresetListItem.DisplayName);
        PresetComboBox.SelectedValuePath = nameof(SettingsPresetListItem.Name);
        PomodoroSoundComboBox.ItemsSource = PomodoroSoundOptions;
        PomodoroSoundComboBox.DisplayMemberPath = nameof(PomodoroSoundOption.Name);
        PomodoroSoundComboBox.SelectedValuePath = nameof(PomodoroSoundOption.Sound);
        Settings = settings.Clone();
        Settings.Normalize();
        LoadSettingsToControls();
        MarkClean();
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
        SnapToScreenEdgesCheckBox.IsChecked = Settings.SnapToScreenEdges;
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
        UpdateCommandState();
    }

    private void ApplyControlsToSettings()
    {
        Settings = BuildSettingsFromControls();
    }

    private WidgetSettings BuildSettingsFromControls()
    {
        var settings = Settings.Clone();
        settings.ShowBorder = ShowBorderCheckBox.IsChecked == true;
        settings.ShowDate = ShowDateCheckBox.IsChecked == true;
        settings.ShowWeekday = ShowWeekdayCheckBox.IsChecked == true;
        settings.DateFontSize = Math.Round(DateFontSizeSlider.Value);
        settings.BackgroundShade = (byte)Math.Round(BackgroundShadeSlider.Value);
        settings.BackgroundOpacity = Math.Round(BackgroundOpacitySlider.Value) / 100;
        settings.PaddingHorizontal = Math.Round(PaddingHorizontalSlider.Value, 1);
        settings.PaddingTop = Math.Round(PaddingTopSlider.Value, 1);
        settings.PaddingBottom = Math.Round(PaddingBottomSlider.Value, 1);
        settings.FitToContent = FitToContentCheckBox.IsChecked == true;
        settings.Width = Math.Round(WidthSlider.Value);
        settings.Height = Math.Round(HeightSlider.Value);
        settings.ClockFontSize = Math.Round(ClockFontSizeSlider.Value);
        settings.ClockFontWeight = (int)Math.Round(ClockFontWeightSlider.Value);
        settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
        settings.SnapToScreenEdges = SnapToScreenEdgesCheckBox.IsChecked == true;
        settings.PomodoroEnabled = PomodoroEnabledCheckBox.IsChecked == true;
        settings.PomodoroFocusMinutes = (int)Math.Round(PomodoroFocusMinutesSlider.Value);
        settings.PomodoroBreakMinutes = (int)Math.Round(PomodoroBreakMinutesSlider.Value);
        settings.PomodoroAutoStartBreak = PomodoroAutoStartBreakCheckBox.IsChecked == true;
        settings.PomodoroReturnToClockAfterBreak = PomodoroReturnToClockCheckBox.IsChecked == true;
        settings.PomodoroPlaySound = PomodoroPlaySoundCheckBox.IsChecked == true;
        settings.PomodoroSound = GetSelectedPomodoroSound();
        settings.Normalize();
        return settings;
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
            MarkPendingChanges();
        }
    }

    private void FitToContentCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdateSizeControlState();
            MarkPendingChanges();
        }
    }

    private void ShowDateCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdateDateControlState();
            MarkPendingChanges();
        }
    }

    private void PomodoroEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdatePomodoroControlState();
            MarkPendingChanges();
        }
    }

    private void SettingControl_Changed(object sender, RoutedEventArgs e)
    {
        MarkPendingChanges();
    }

    private void SettingControl_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        MarkPendingChanges();
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
        MarkClean();
    }

    private void DefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        Settings.ResetAppearanceToDefaults();
        LoadSettingsToControls();
        MarkPendingChanges();
    }

    private void PresetComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isLoading || PresetComboBox.SelectedItem is not SettingsPresetListItem preset)
        {
            return;
        }

        PresetNameTextBox.Text = preset.Name;
        UpdatePresetCommandState();
    }

    private void PresetNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!_isLoading && IsLoaded)
        {
            UpdatePresetCommandState();
        }
    }

    private void SavePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var presetName = PresetNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(presetName))
        {
            WpfMessageBox.Show(this, "Enter preset name first.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        ApplyControlsToSettings();

        var result = _presetCatalog.Save(Settings, presetName);
        UpdatePresetList(presetName);
        PresetKindText.Text = result == SettingsPresetSaveResult.SavedCustomOverride
            ? "Saved as a custom override of a built-in preset."
            : "Saved as a custom preset.";
        MarkPendingChanges();
    }

    private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
    {
        var lookup = GetSelectedPreset();
        if (lookup is null)
        {
            WpfMessageBox.Show(this, "Select a preset to load.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Settings.ApplyPreset(lookup.Preset);
        LoadSettingsToControls();
        PresetComboBox.SelectedItem = GetPresetListItem(lookup.Preset.Name);
        PresetNameTextBox.Text = lookup.Preset.Name;
        PresetKindText.Text = lookup.Kind == SettingsPresetKind.BuiltIn
            ? "Loaded a built-in preset."
            : "Loaded a custom preset.";
        MarkPendingChanges();
    }

    private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var presetName = GetSelectedPresetName();
        var result = _presetCatalog.DeleteCustom(Settings, presetName);
        switch (result)
        {
            case SettingsPresetDeleteResult.BlankName:
            case SettingsPresetDeleteResult.NotFound:
                WpfMessageBox.Show(this, "Select a custom preset to delete.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            case SettingsPresetDeleteResult.BuiltInOnly:
                WpfMessageBox.Show(this, "Built-in presets cannot be deleted.", "Clock Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            case SettingsPresetDeleteResult.ResetCustomOverride:
                UpdatePresetList(presetName);
                PresetKindText.Text = "Custom override reset. The built-in preset is active again.";
                MarkPendingChanges();
                return;
        }

        PresetNameTextBox.Clear();
        UpdatePresetList();
        PresetKindText.Text = "Custom preset deleted.";
        MarkPendingChanges();
    }

    private SettingsPresetLookup? GetSelectedPreset()
    {
        return _presetCatalog.Find(Settings, GetSelectedPresetName());
    }

    private string GetSelectedPresetName()
    {
        var presetName = PresetNameTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(presetName))
        {
            return presetName;
        }

        return PresetComboBox.SelectedItem is SettingsPresetListItem preset
            ? preset.Name
            : "";
    }

    private void UpdatePresetList(string? selectedPresetName = null)
    {
        var presets = _presetCatalog.BuildList(Settings);
        PresetComboBox.ItemsSource = presets;
        PresetComboBox.SelectedItem = string.IsNullOrWhiteSpace(selectedPresetName)
            ? null
            : presets.FirstOrDefault(preset =>
                string.Equals(preset.Name, selectedPresetName, StringComparison.OrdinalIgnoreCase));
        UpdatePresetCommandState();
    }

    private SettingsPresetListItem? GetPresetListItem(string presetName)
    {
        return PresetComboBox.ItemsSource is IEnumerable<SettingsPresetListItem> presets
            ? presets.FirstOrDefault(preset =>
                string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase))
            : null;
    }

    private void UpdatePresetCommandState()
    {
        if (DeletePresetButton is null || SavePresetButton is null || PresetKindText is null)
        {
            return;
        }

        var presetName = GetSelectedPresetName();
        var preset = string.IsNullOrWhiteSpace(presetName) ? null : GetPresetListItem(presetName);
        DeletePresetButton.IsEnabled = preset?.CanDelete == true;
        DeletePresetButton.Content = preset?.Kind == SettingsPresetKind.CustomOverride ? "Reset" : "Delete";
        SavePresetButton.Content = _presetCatalog.IsBuiltInName(presetName) ? "Save override" : "Save";

        PresetKindText.Text = preset?.Kind switch
        {
            SettingsPresetKind.BuiltIn => "Built-in preset. Load it as-is, or save with this name to create a custom override.",
            SettingsPresetKind.CustomOverride => "Custom override. Reset removes your override and restores the built-in preset.",
            SettingsPresetKind.Custom => "Custom preset.",
            _ => ""
        };
    }

    private void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenSettingsFileDialog
        {
            Filter = "Clock settings (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import Clock Settings"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var importedSettings = SettingsStore.Deserialize(File.ReadAllText(dialog.FileName));
            importedSettings.Left = Settings.Left;
            importedSettings.Top = Settings.Top;
            Settings = importedSettings;
            LoadSettingsToControls();
            MarkPendingChanges();
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(
                this,
                $"Could not import settings.\n\n{ex.Message}",
                "Clock Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveSettingsFileDialog
        {
            AddExtension = true,
            DefaultExt = ".json",
            FileName = "ClockWidget.settings.json",
            Filter = "Clock settings (*.json)|*.json|All files (*.*)|*.*",
            Title = "Export Clock Settings"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ApplyControlsToSettings();
            File.WriteAllText(dialog.FileName, SettingsStore.Serialize(Settings));
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show(
                this,
                $"Could not export settings.\n\n{ex.Message}",
                "Clock Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void MarkPendingChanges()
    {
        if (_isLoading || !IsLoaded)
        {
            return;
        }

        _hasPendingChanges = SettingsStore.Serialize(BuildSettingsFromControls()) != _lastAppliedSettingsJson;
        UpdateApplyButtonState();
    }

    private void MarkClean()
    {
        _lastAppliedSettingsJson = SettingsStore.Serialize(Settings);
        _hasPendingChanges = false;
        UpdateApplyButtonState();
    }

    private void UpdateCommandState()
    {
        UpdateApplyButtonState();
        UpdatePresetCommandState();
    }

    private void UpdateApplyButtonState()
    {
        if (ApplyButton is not null)
        {
            ApplyButton.IsEnabled = _hasPendingChanges;
        }
    }
}
