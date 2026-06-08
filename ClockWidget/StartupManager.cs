using System.Diagnostics;
using Microsoft.Win32;

namespace ClockWidget;

internal static class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ClockWidget";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        var command = key?.GetValue(ValueName) as string;
        return CommandTargetsCurrentExecutable(command);
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (key is null)
        {
            throw new InvalidOperationException("Could not open Windows startup registry key.");
        }

        if (enabled)
        {
            var executablePath = GetExecutablePath()
                ?? throw new InvalidOperationException("Could not determine the application executable path.");

            key.SetValue(ValueName, Quote(executablePath), RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }

    private static bool CommandTargetsCurrentExecutable(string? command)
    {
        var executablePath = GetExecutablePath();
        if (string.IsNullOrWhiteSpace(command) || string.IsNullOrWhiteSpace(executablePath))
        {
            return false;
        }

        var trimmedCommand = command.Trim();
        var quotedPath = Quote(executablePath);

        return string.Equals(trimmedCommand, executablePath, StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmedCommand, quotedPath, StringComparison.OrdinalIgnoreCase)
            || trimmedCommand.StartsWith(quotedPath + " ", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetExecutablePath()
    {
        return Environment.ProcessPath
            ?? Process.GetCurrentProcess().MainModule?.FileName;
    }

    private static string Quote(string value)
    {
        return $"\"{value}\"";
    }
}
