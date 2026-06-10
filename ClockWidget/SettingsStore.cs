using System.IO;
using System.Text.Json;

namespace ClockWidget;

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsDirectory;
    private readonly string _settingsPath;
    private string? _lastSavedJson;

    public SettingsStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClockWidget"))
    {
    }

    internal SettingsStore(string settingsDirectory)
    {
        _settingsDirectory = settingsDirectory;
        _settingsPath = Path.Combine(_settingsDirectory, "settings.json");
    }

    public WidgetSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new WidgetSettings();
            }

            _lastSavedJson = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<WidgetSettings>(_lastSavedJson) ?? new WidgetSettings();
            settings.Normalize();
            return settings;
        }
        catch
        {
            _lastSavedJson = null;
            return new WidgetSettings();
        }
    }

    public void Save(WidgetSettings settings)
    {
        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        var json = JsonSerializer.Serialize(normalizedSettings, JsonOptions);
        if (json == _lastSavedJson)
        {
            return;
        }

        Directory.CreateDirectory(_settingsDirectory);
        File.WriteAllText(_settingsPath, json);
        _lastSavedJson = json;
    }
}
