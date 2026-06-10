namespace ClockWidget;

internal sealed class PomodoroSession
{
    public PomodoroSession(PomodoroController? controller = null)
    {
        Controller = controller ?? new PomodoroController();
    }

    public PomodoroController Controller { get; }
    public WidgetDisplayMode DisplayMode { get; private set; } = WidgetDisplayMode.Clock;

    public bool IsPomodoroDisplayVisible(WidgetSettings settings)
    {
        return DisplayMode == WidgetDisplayMode.Pomodoro && settings.PomodoroEnabled;
    }

    public void Reset(TimeSpan focusDuration, bool showClock)
    {
        Controller.Reset(focusDuration);
        DisplayMode = showClock ? WidgetDisplayMode.Clock : WidgetDisplayMode.Pomodoro;
    }

    public bool ToggleDisplay(WidgetSettings settings, TimeSpan focusDuration, TimeSpan breakDuration)
    {
        if (!settings.PomodoroEnabled)
        {
            return false;
        }

        Controller.EnsureRemaining(focusDuration, breakDuration);
        DisplayMode = DisplayMode == WidgetDisplayMode.Pomodoro
            ? WidgetDisplayMode.Clock
            : WidgetDisplayMode.Pomodoro;
        return true;
    }

    public bool ToggleStartPause(WidgetSettings settings, TimeSpan focusDuration, TimeSpan breakDuration)
    {
        if (!settings.PomodoroEnabled)
        {
            return false;
        }

        DisplayMode = WidgetDisplayMode.Pomodoro;
        Controller.ToggleStartPause(focusDuration, breakDuration);
        return true;
    }

    public PomodoroPhaseCompletion Update(TimeSpan breakDuration, bool autoStartBreak)
    {
        return Controller.Update(breakDuration, autoStartBreak);
    }

    public void CompletePhase(PomodoroPhaseCompletion completion, TimeSpan focusDuration, bool returnToClockAfterBreak)
    {
        if (completion == PomodoroPhaseCompletion.FocusCompleted)
        {
            DisplayMode = WidgetDisplayMode.Pomodoro;
            return;
        }

        if (completion == PomodoroPhaseCompletion.BreakCompleted)
        {
            Reset(focusDuration, showClock: returnToClockAfterBreak);
        }
    }

    public void ApplySettings(WidgetSettings settings, TimeSpan focusDuration)
    {
        if (!settings.PomodoroEnabled)
        {
            Reset(focusDuration, showClock: true);
            return;
        }

        Controller.ApplyFocusDurationIfIdle(focusDuration);
    }

    public string GetStartPauseHeader(TimeSpan focusDuration, TimeSpan breakDuration)
    {
        if (Controller.IsRunning)
        {
            return "Pause Pomodoro";
        }

        var phase = Controller.Phase == PomodoroPhase.Focus ? "Pomodoro" : "Break";
        var fullDuration = Controller.Phase == PomodoroPhase.Focus ? focusDuration : breakDuration;
        return Controller.Remaining < fullDuration ? $"Resume {phase}" : $"Start {phase}";
    }

    public DisplayTickState GetTickState(WidgetSettings settings)
    {
        return new DisplayTickState(
            ShowSeconds: settings.ShowSeconds,
            IsPomodoroDisplayVisible: IsPomodoroDisplayVisible(settings),
            IsPomodoroRunning: settings.PomodoroEnabled && Controller.IsRunning);
    }
}
