using System.IO;

namespace ClockWidget.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public void Save_WhenSettingsMatchLastSavedJson_DoesNotRewriteFile()
    {
        using var tempDirectory = TempDirectory.Create();
        var store = new SettingsStore(tempDirectory.Path);
        var settings = new WidgetSettings
        {
            Left = 10,
            Top = 20,
            ShowSeconds = false
        };

        store.Save(settings);
        var settingsPath = Path.Combine(tempDirectory.Path, "settings.json");
        const string externallyChangedContent = "{\"external\":true}";
        File.WriteAllText(settingsPath, externallyChangedContent);

        store.Save(settings);

        Assert.Equal(externallyChangedContent, File.ReadAllText(settingsPath));
    }

    [Fact]
    public void Save_WhenSettingsChange_WritesUpdatedJson()
    {
        using var tempDirectory = TempDirectory.Create();
        var store = new SettingsStore(tempDirectory.Path);
        var settings = new WidgetSettings
        {
            ShowSeconds = true
        };

        store.Save(settings);
        var settingsPath = Path.Combine(tempDirectory.Path, "settings.json");
        File.WriteAllText(settingsPath, "{\"external\":true}");
        settings.ShowSeconds = false;

        store.Save(settings);

        var savedJson = File.ReadAllText(settingsPath);
        Assert.Contains("\"ShowSeconds\": false", savedJson);
        Assert.DoesNotContain("\"external\":true", savedJson);
    }

    [Fact]
    public void Load_WhenJsonIsInvalid_ReturnsDefaultSettings()
    {
        using var tempDirectory = TempDirectory.Create();
        Directory.CreateDirectory(tempDirectory.Path);
        File.WriteAllText(Path.Combine(tempDirectory.Path, "settings.json"), "{ invalid json");

        var settings = new SettingsStore(tempDirectory.Path).Load();

        Assert.Equal(330d, settings.Width);
        Assert.Equal(118d, settings.Height);
        Assert.True(settings.ShowSeconds);
    }

    [Fact]
    public void SerializeAndDeserialize_RoundTripsNormalizedSettings()
    {
        var settings = new WidgetSettings
        {
            Width = 9999,
            ShowSeconds = false,
            Presets =
            [
                new WidgetPreset { Name = "  Compact Custom  ", ClockFontSize = 44 }
            ]
        };

        var json = SettingsStore.Serialize(settings);
        var restored = SettingsStore.Deserialize(json);

        Assert.Equal(WidgetSettings.MaxWidth, restored.Width);
        Assert.False(restored.ShowSeconds);
        Assert.Single(restored.Presets);
        Assert.Equal("Compact Custom", restored.Presets[0].Name);
        Assert.Equal(44d, restored.Presets[0].ClockFontSize);
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TempDirectory Create()
        {
            return new TempDirectory(System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "ClockWidget.Tests",
                Guid.NewGuid().ToString("N")));
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
