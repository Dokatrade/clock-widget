namespace ClockWidget;

internal sealed class StartupSettingsService
{
    public bool ReadEnabled()
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

    public StartupSettingsResult ApplyEnabled(bool enabled)
    {
        try
        {
            StartupManager.SetEnabled(enabled);
            return StartupSettingsResult.Success(enabled);
        }
        catch (Exception ex)
        {
            return StartupSettingsResult.Failure(ReadEnabled(), ex.Message);
        }
    }
}

internal sealed record StartupSettingsResult(
    bool Succeeded,
    bool CurrentEnabled,
    string? ErrorMessage)
{
    public static StartupSettingsResult Success(bool enabled)
    {
        return new StartupSettingsResult(true, enabled, null);
    }

    public static StartupSettingsResult Failure(bool currentEnabled, string errorMessage)
    {
        return new StartupSettingsResult(false, currentEnabled, errorMessage);
    }
}
