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
            return Deserialize(_lastSavedJson);
        }
        catch
        {
            _lastSavedJson = null;
            return new WidgetSettings();
        }
    }

    public void Save(WidgetSettings settings)
    {
        var json = Serialize(settings);
        if (json == _lastSavedJson)
        {
            return;
        }

        Directory.CreateDirectory(_settingsDirectory);
        File.WriteAllText(_settingsPath, json);
        _lastSavedJson = json;
    }

    public static string Serialize(WidgetSettings settings)
    {
        var normalizedSettings = settings.Clone();
        normalizedSettings.Normalize();
        return JsonSerializer.Serialize(normalizedSettings, JsonOptions);
    }

    public static WidgetSettings Deserialize(string json)
    {
        var settings = JsonSerializer.Deserialize<WidgetSettings>(json) ?? new WidgetSettings();
        settings.Normalize();
        return settings;
    }
}
