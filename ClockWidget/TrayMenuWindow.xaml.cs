using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;
using Screen = System.Windows.Forms.Screen;

namespace ClockWidget;

public partial class TrayMenuWindow : Window
{
    private readonly TrayIconActions _actions;
    private bool _closingFromCommand;

    internal TrayMenuWindow(TrayIconActions actions)
    {
        InitializeComponent();
        _actions = actions;
        AddHandler(
            Mouse.PreviewMouseDownOutsideCapturedElementEvent,
            new MouseButtonEventHandler(Window_PreviewMouseDownOutsideCapturedElement));
    }

    internal void UpdateState(TrayIconState state)
    {
        ShowHideText.Text = state.IsWidgetVisible ? "Hide widget" : "Show widget";
        AlwaysOnTopCheck.Visibility = state.AlwaysOnTop ? Visibility.Visible : Visibility.Collapsed;
        LockPositionCheck.Visibility = state.LockPosition ? Visibility.Visible : Visibility.Collapsed;
        PomodoroModeButton.IsEnabled = state.PomodoroEnabled;
        PomodoroStartPauseButton.IsEnabled = state.PomodoroEnabled;
        PomodoroResetButton.IsEnabled = state.PomodoroEnabled;
        PomodoroModeCheck.Visibility = state.IsPomodoroVisible ? Visibility.Visible : Visibility.Collapsed;
        PomodoroStartPauseText.Text = state.PomodoroStartPauseText;
    }

    internal void ShowAtCursor()
    {
        _closingFromCommand = false;

        if (!IsVisible)
        {
            Opacity = 0;
            Show();
        }

        UpdateLayout();
        PositionNearCursor();
        Opacity = 1;
        ActivateForOutsideClickDismissal();
    }

    private void PositionNearCursor()
    {
        var cursorPosition = Forms.Cursor.Position;
        var screen = Screen.FromPoint(cursorPosition);
        var dpi = VisualTreeHelper.GetDpi(this);
        var cursorX = cursorPosition.X / dpi.DpiScaleX;
        var cursorY = cursorPosition.Y / dpi.DpiScaleY;
        var workAreaLeft = screen.WorkingArea.Left / dpi.DpiScaleX;
        var workAreaTop = screen.WorkingArea.Top / dpi.DpiScaleY;
        var workAreaRight = screen.WorkingArea.Right / dpi.DpiScaleX;
        var workAreaBottom = screen.WorkingArea.Bottom / dpi.DpiScaleY;
        var menuWidth = ActualWidth > 0 ? ActualWidth : Width;
        var menuHeight = ActualHeight > 0 ? ActualHeight : Height;

        Left = Math.Clamp(cursorX - menuWidth + 10, workAreaLeft + 6, workAreaRight - menuWidth - 6);
        Top = Math.Clamp(cursorY - menuHeight + 10, workAreaTop + 6, workAreaBottom - menuHeight - 6);
    }

    private void RunCommand(Action action)
    {
        _closingFromCommand = true;
        HideMenu();
        action();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (!_closingFromCommand)
        {
            HideMenu();
        }
    }

    private void Window_PreviewMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
    {
        if (!_closingFromCommand)
        {
            HideMenu();
        }
    }

    private void ActivateForOutsideClickDismissal()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(handle);
        }

        Activate();
        Focus();
        Mouse.Capture(this, CaptureMode.SubTree);
    }

    private void HideMenu()
    {
        if (Mouse.Captured == this)
        {
            Mouse.Capture(null);
        }

        Hide();
    }

    private void ShowHideButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.ToggleWidgetVisibility);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.OpenSettings);
    }

    private void AlwaysOnTopButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.ToggleAlwaysOnTop);
    }

    private void LockPositionButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.ToggleLockPosition);
    }

    private void ResetPositionButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.ResetPosition);
    }

    private void PomodoroModeButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.TogglePomodoroDisplay);
    }

    private void PomodoroStartPauseButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.TogglePomodoroStartPause);
    }

    private void PomodoroResetButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.ResetPomodoro);
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        RunCommand(_actions.Exit);
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
