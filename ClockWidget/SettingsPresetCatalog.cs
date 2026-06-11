namespace ClockWidget;

internal sealed class SettingsPresetCatalog
{
    private readonly IReadOnlyList<WidgetPreset> _builtInPresets;

    public SettingsPresetCatalog()
        : this(WidgetSettings.CreateBuiltInPresets())
    {
    }

    internal SettingsPresetCatalog(IReadOnlyList<WidgetPreset> builtInPresets)
    {
        _builtInPresets = builtInPresets.Select(preset => preset.Clone()).ToList();
    }

    public IReadOnlyList<SettingsPresetListItem> BuildList(WidgetSettings settings)
    {
        var userPresets = settings.Presets ?? [];
        var userNames = new HashSet<string>(
            userPresets.Select(preset => preset.Name),
            StringComparer.OrdinalIgnoreCase);
        var builtInNames = new HashSet<string>(
            _builtInPresets.Select(preset => preset.Name),
            StringComparer.OrdinalIgnoreCase);

        return userPresets
            .Select(preset => new SettingsPresetListItem(
                preset.Name,
                builtInNames.Contains(preset.Name)
                    ? SettingsPresetKind.CustomOverride
                    : SettingsPresetKind.Custom))
            .Concat(_builtInPresets
                .Where(preset => !userNames.Contains(preset.Name))
                .Select(preset => new SettingsPresetListItem(preset.Name, SettingsPresetKind.BuiltIn)))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public SettingsPresetLookup? Find(WidgetSettings settings, string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return null;
        }

        presetName = presetName.Trim();
        var userPreset = settings.Presets.FirstOrDefault(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));
        if (userPreset is not null)
        {
            return new SettingsPresetLookup(
                userPreset.Clone(),
                IsBuiltInName(userPreset.Name)
                    ? SettingsPresetKind.CustomOverride
                    : SettingsPresetKind.Custom);
        }

        var builtInPreset = _builtInPresets.FirstOrDefault(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));
        return builtInPreset is null
            ? null
            : new SettingsPresetLookup(builtInPreset.Clone(), SettingsPresetKind.BuiltIn);
    }

    public SettingsPresetSaveResult Save(WidgetSettings settings, string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return SettingsPresetSaveResult.BlankName;
        }

        presetName = presetName.Trim();
        var existingPreset = settings.Presets.FirstOrDefault(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));
        if (existingPreset is not null)
        {
            settings.Presets.Remove(existingPreset);
        }

        settings.Presets.Add(settings.CreatePreset(presetName));
        settings.Presets = settings.Presets
            .OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return IsBuiltInName(presetName)
            ? SettingsPresetSaveResult.SavedCustomOverride
            : SettingsPresetSaveResult.SavedCustom;
    }

    public SettingsPresetDeleteResult DeleteCustom(WidgetSettings settings, string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return SettingsPresetDeleteResult.BlankName;
        }

        presetName = presetName.Trim();
        var existingPreset = settings.Presets.FirstOrDefault(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));
        if (existingPreset is not null)
        {
            settings.Presets.Remove(existingPreset);
            return IsBuiltInName(presetName)
                ? SettingsPresetDeleteResult.ResetCustomOverride
                : SettingsPresetDeleteResult.DeletedCustom;
        }

        return IsBuiltInName(presetName)
            ? SettingsPresetDeleteResult.BuiltInOnly
            : SettingsPresetDeleteResult.NotFound;
    }

    public bool IsBuiltInName(string presetName)
    {
        presetName = presetName.Trim();
        return _builtInPresets.Any(preset =>
            string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed record SettingsPresetListItem(string Name, SettingsPresetKind Kind)
{
    public string DisplayName => Kind switch
    {
        SettingsPresetKind.BuiltIn => $"{Name} (built-in)",
        SettingsPresetKind.CustomOverride => $"{Name} (custom override)",
        _ => $"{Name} (custom)"
    };

    public bool CanDelete => Kind is not SettingsPresetKind.BuiltIn;
}

internal sealed record SettingsPresetLookup(WidgetPreset Preset, SettingsPresetKind Kind);

internal enum SettingsPresetKind
{
    BuiltIn,
    Custom,
    CustomOverride
}

internal enum SettingsPresetSaveResult
{
    BlankName,
    SavedCustom,
    SavedCustomOverride
}

internal enum SettingsPresetDeleteResult
{
    BlankName,
    NotFound,
    BuiltInOnly,
    DeletedCustom,
    ResetCustomOverride
}
