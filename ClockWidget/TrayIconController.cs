using System.Windows.Threading;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace ClockWidget;

internal sealed class TrayIconController : IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly TrayIconActions _actions;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Drawing.Icon _icon;
    private TrayMenuWindow? _menuWindow;
    private TrayIconState _state = TrayIconState.Default;
    private bool _disposed;

    public TrayIconController(Dispatcher dispatcher, TrayIconActions actions)
    {
        _dispatcher = dispatcher;
        _actions = actions;
        _icon = LoadTrayIcon();

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = _icon,
            Text = "Clock Widget",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => InvokeOnUi(_actions.ToggleWidgetVisibility);
        _notifyIcon.MouseUp += (_, e) =>
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                InvokeOnUi(ShowMenu);
            }
        };
        RefreshState();
    }

    public void UpdateState(TrayIconState state)
    {
        _state = state;
        ApplyState();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menuWindow?.Close();
        _icon.Dispose();
        _disposed = true;
    }

    private static Drawing.Icon LoadTrayIcon()
    {
        var iconResource = System.Windows.Application.GetResourceStream(
            new Uri("pack://application:,,,/Assets/AppIcon.ico", UriKind.Absolute));

        if (iconResource?.Stream is null)
        {
            return (Drawing.Icon)Drawing.SystemIcons.Application.Clone();
        }

        using var stream = iconResource.Stream;
        return new Drawing.Icon(stream);
    }

    private void ApplyState()
    {
        _menuWindow?.UpdateState(_state);
    }

    private void RefreshState()
    {
        _state = _actions.GetState();
        ApplyState();
    }

    private void ShowMenu()
    {
        RefreshState();

        if (_menuWindow is null)
        {
            _menuWindow = new TrayMenuWindow(_actions);
            _menuWindow.Closed += (_, _) => _menuWindow = null;
        }

        _menuWindow.UpdateState(_state);
        _menuWindow.ShowAtCursor();
    }

    private void InvokeOnUi(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}

internal sealed record TrayIconActions(
    Func<TrayIconState> GetState,
    Action ToggleWidgetVisibility,
    Action OpenSettings,
    Action ToggleAlwaysOnTop,
    Action ToggleLockPosition,
    Action ResetPosition,
    Action TogglePomodoroDisplay,
    Action TogglePomodoroStartPause,
    Action ResetPomodoro,
    Action Exit);

internal sealed record TrayIconState(
    bool IsWidgetVisible,
    bool AlwaysOnTop,
    bool LockPosition,
    bool PomodoroEnabled,
    bool IsPomodoroVisible,
    string PomodoroStartPauseText)
{
    public static TrayIconState Default { get; } = new(
        IsWidgetVisible: true,
        AlwaysOnTop: true,
        LockPosition: false,
        PomodoroEnabled: true,
        IsPomodoroVisible: false,
        PomodoroStartPauseText: "Start Pomodoro");
}
