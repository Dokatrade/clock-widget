namespace ClockWidget;

internal sealed class PomodoroController
{
    private readonly IClock _clock;
    private DateTime _endsAt;

    public PomodoroController(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }

    public PomodoroPhase Phase { get; private set; } = PomodoroPhase.Focus;
    public TimeSpan Remaining { get; private set; } = TimeSpan.Zero;
    public bool IsRunning { get; private set; }
    public bool HasActiveSession { get; private set; }

    public void Reset(TimeSpan focusDuration)
    {
        Phase = PomodoroPhase.Focus;
        Remaining = focusDuration;
        IsRunning = false;
        HasActiveSession = false;
        _endsAt = default;
    }

    public void EnsureRemaining(TimeSpan focusDuration, TimeSpan breakDuration)
    {
        if (Remaining <= TimeSpan.Zero)
        {
            Remaining = Phase == PomodoroPhase.Focus
                ? focusDuration
                : breakDuration;
        }
    }

    public void ToggleStartPause(TimeSpan focusDuration, TimeSpan breakDuration)
    {
        EnsureRemaining(focusDuration, breakDuration);

        if (IsRunning)
        {
            Remaining = _endsAt - _clock.Now;
            if (Remaining < TimeSpan.Zero)
            {
                Remaining = TimeSpan.Zero;
            }

            IsRunning = false;
            return;
        }

        _endsAt = _clock.Now + Remaining;
        IsRunning = true;
        HasActiveSession = true;
    }

    public PomodoroPhaseCompletion Update(TimeSpan breakDuration, bool autoStartBreak)
    {
        if (!IsRunning)
        {
            return PomodoroPhaseCompletion.None;
        }

        var remaining = _endsAt - _clock.Now;
        if (remaining > TimeSpan.Zero)
        {
            Remaining = remaining;
            return PomodoroPhaseCompletion.None;
        }

        Remaining = TimeSpan.Zero;

        if (Phase == PomodoroPhase.Focus)
        {
            Phase = PomodoroPhase.Break;
            Remaining = breakDuration;
            IsRunning = autoStartBreak;
            if (IsRunning)
            {
                _endsAt = _clock.Now + Remaining;
            }

            return PomodoroPhaseCompletion.FocusCompleted;
        }

        IsRunning = false;
        return PomodoroPhaseCompletion.BreakCompleted;
    }

    public void ApplyFocusDurationIfIdle(TimeSpan focusDuration)
    {
        if (!IsRunning && Phase == PomodoroPhase.Focus)
        {
            Remaining = focusDuration;
        }
    }
}

internal enum PomodoroPhaseCompletion
{
    None,
    FocusCompleted,
    BreakCompleted
}
