namespace ClockWidget.Tests;

public sealed class SettingsPresetCatalogTests
{
    [Fact]
    public void BuildList_LabelsBuiltInCustomAndCustomOverridePresets()
    {
        var settings = new WidgetSettings
        {
            Presets =
            [
                new WidgetPreset { Name = "Minimal", ClockFontSize = 44 },
                new WidgetPreset { Name = "Work", ClockFontSize = 62 }
            ]
        };
        var catalog = new SettingsPresetCatalog();

        var items = catalog.BuildList(settings);

        Assert.Contains(items, item =>
            item.Name == "Compact" && item.Kind == SettingsPresetKind.BuiltIn && !item.CanDelete);
        Assert.Contains(items, item =>
            item.Name == "Minimal" && item.Kind == SettingsPresetKind.CustomOverride && item.CanDelete);
        Assert.Contains(items, item =>
            item.Name == "Work" && item.Kind == SettingsPresetKind.Custom && item.CanDelete);
    }

    [Fact]
    public void Find_WhenCustomOverrideExists_ReturnsCustomPreset()
    {
        var settings = new WidgetSettings
        {
            Presets =
            [
                new WidgetPreset { Name = "Minimal", ClockFontSize = 44 }
            ]
        };
        var catalog = new SettingsPresetCatalog();

        var lookup = catalog.Find(settings, "Minimal");

        Assert.NotNull(lookup);
        Assert.Equal(SettingsPresetKind.CustomOverride, lookup.Kind);
        Assert.Equal(44d, lookup.Preset.ClockFontSize);
    }

    [Fact]
    public void Save_WhenNameMatchesBuiltIn_CreatesCustomOverride()
    {
        var settings = new WidgetSettings
        {
            ClockFontSize = 72
        };
        var catalog = new SettingsPresetCatalog();

        var result = catalog.Save(settings, "Minimal");

        Assert.Equal(SettingsPresetSaveResult.SavedCustomOverride, result);
        Assert.Single(settings.Presets);
        Assert.Equal("Minimal", settings.Presets[0].Name);
        Assert.Equal(72d, settings.Presets[0].ClockFontSize);
    }

    [Fact]
    public void Save_TrimsPresetNameBeforeMatchingBuiltIns()
    {
        var settings = new WidgetSettings();
        var catalog = new SettingsPresetCatalog();

        var result = catalog.Save(settings, "  Minimal  ");

        Assert.Equal(SettingsPresetSaveResult.SavedCustomOverride, result);
        Assert.Single(settings.Presets);
        Assert.Equal("Minimal", settings.Presets[0].Name);
    }

    [Fact]
    public void DeleteCustom_WhenPresetIsBuiltInOnly_DoesNotRemoveBuiltIn()
    {
        var settings = new WidgetSettings();
        var catalog = new SettingsPresetCatalog();

        var result = catalog.DeleteCustom(settings, "Minimal");

        Assert.Equal(SettingsPresetDeleteResult.BuiltInOnly, result);
        Assert.Empty(settings.Presets);
        Assert.NotNull(catalog.Find(settings, "Minimal"));
    }

    [Fact]
    public void DeleteCustom_WhenPresetIsCustomOverride_RevealsBuiltIn()
    {
        var settings = new WidgetSettings
        {
            Presets =
            [
                new WidgetPreset { Name = "Minimal", ClockFontSize = 44 }
            ]
        };
        var catalog = new SettingsPresetCatalog();

        var result = catalog.DeleteCustom(settings, "Minimal");
        var lookup = catalog.Find(settings, "Minimal");

        Assert.Equal(SettingsPresetDeleteResult.ResetCustomOverride, result);
        Assert.Empty(settings.Presets);
        Assert.NotNull(lookup);
        Assert.Equal(SettingsPresetKind.BuiltIn, lookup.Kind);
        Assert.NotEqual(44d, lookup.Preset.ClockFontSize);
    }
}
