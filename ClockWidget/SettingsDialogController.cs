using System.Windows;

namespace ClockWidget;

internal sealed class SettingsDialogController
{
    public void Show(Window owner, WidgetSettings settings, Action<WidgetSettings> applySettings)
    {
        var settingsWindow = new SettingsWindow(settings)
        {
            Owner = owner
        };

        settingsWindow.SettingsApplied += (_, updatedSettings) =>
        {
            applySettings(updatedSettings.Clone());
        };

        if (settingsWindow.ShowDialog() == true)
        {
            applySettings(settingsWindow.Settings.Clone());
        }
    }
}
