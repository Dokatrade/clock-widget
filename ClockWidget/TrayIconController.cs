using System.Windows.Threading;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace ClockWidget;

internal sealed class TrayIconController : IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly TrayIconActions _actions;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ContextMenuStrip _menu;
    private readonly Drawing.Icon _icon;
    private readonly Forms.ToolStripMenuItem _showHideMenuItem;
    private readonly Forms.ToolStripMenuItem _alwaysOnTopMenuItem;
    private readonly Forms.ToolStripMenuItem _lockPositionMenuItem;
    private readonly Forms.ToolStripMenuItem _pomodoroModeMenuItem;
    private readonly Forms.ToolStripMenuItem _pomodoroStartPauseMenuItem;
    private readonly Forms.ToolStripMenuItem _pomodoroResetMenuItem;
    private TrayIconState _state = TrayIconState.Default;
    private bool _disposed;

    public TrayIconController(Dispatcher dispatcher, TrayIconActions actions)
    {
        _dispatcher = dispatcher;
        _actions = actions;
        _icon = LoadTrayIcon();
        _menu = new Forms.ContextMenuStrip();
        _showHideMenuItem = new Forms.ToolStripMenuItem("Hide widget", null, (_, _) =>
            InvokeOnUi(_actions.ToggleWidgetVisibility));
        _alwaysOnTopMenuItem = new Forms.ToolStripMenuItem("Always on top", null, (_, _) =>
            InvokeOnUi(_actions.ToggleAlwaysOnTop));
        _lockPositionMenuItem = new Forms.ToolStripMenuItem("Lock position", null, (_, _) =>
            InvokeOnUi(_actions.ToggleLockPosition));
        _pomodoroModeMenuItem = new Forms.ToolStripMenuItem("Show Pomodoro", null, (_, _) =>
            InvokeOnUi(_actions.TogglePomodoroDisplay));
        _pomodoroStartPauseMenuItem = new Forms.ToolStripMenuItem("Start Pomodoro", null, (_, _) =>
            InvokeOnUi(_actions.TogglePomodoroStartPause));
        _pomodoroResetMenuItem = new Forms.ToolStripMenuItem("Reset Pomodoro", null, (_, _) =>
            InvokeOnUi(_actions.ResetPomodoro));

        _menu.Items.AddRange(
        [
            _showHideMenuItem,
            new Forms.ToolStripMenuItem("Settings...", null, (_, _) =>
                InvokeOnUi(_actions.OpenSettings)),
            new Forms.ToolStripSeparator(),
            _alwaysOnTopMenuItem,
            _lockPositionMenuItem,
            new Forms.ToolStripMenuItem("Reset position", null, (_, _) =>
                InvokeOnUi(_actions.ResetPosition)),
            new Forms.ToolStripSeparator(),
            _pomodoroModeMenuItem,
            _pomodoroStartPauseMenuItem,
            _pomodoroResetMenuItem,
            new Forms.ToolStripSeparator(),
            new Forms.ToolStripMenuItem("Exit", null, (_, _) => InvokeOnUi(_actions.Exit))
        ]);
        _menu.Opening += (_, _) => RefreshState();

        _notifyIcon = new Forms.NotifyIcon
        {
            ContextMenuStrip = _menu,
            Icon = _icon,
            Text = "Clock Widget",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => InvokeOnUi(_actions.ToggleWidgetVisibility);
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
        _menu.Dispose();
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
        _showHideMenuItem.Text = _state.IsWidgetVisible ? "Hide widget" : "Show widget";
        _alwaysOnTopMenuItem.Checked = _state.AlwaysOnTop;
        _lockPositionMenuItem.Checked = _state.LockPosition;
        _pomodoroModeMenuItem.Enabled = _state.PomodoroEnabled;
        _pomodoroStartPauseMenuItem.Enabled = _state.PomodoroEnabled;
        _pomodoroResetMenuItem.Enabled = _state.PomodoroEnabled;
        _pomodoroModeMenuItem.Checked = _state.IsPomodoroVisible;
        _pomodoroStartPauseMenuItem.Text = _state.PomodoroStartPauseText;
    }

    private void RefreshState()
    {
        _state = _actions.GetState();
        ApplyState();
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
