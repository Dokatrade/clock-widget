using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace ClockWidget;

internal static class WindowPlacementService
{
    private const double SnapThreshold = 48;

    public static bool IsOnScreen(double left, double top)
    {
        const double margin = 40;
        return left >= SystemParameters.VirtualScreenLeft - margin
            && top >= SystemParameters.VirtualScreenTop - margin
            && left <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - margin
            && top <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - margin;
    }

    public static bool EnsureOnScreen(Window window, double fallbackWidth, double fallbackHeight)
    {
        var bounds = GetVirtualScreenBounds();
        var width = GetWindowWidth(window, fallbackWidth);
        var height = GetWindowHeight(window, fallbackHeight);

        var currentLeft = double.IsFinite(window.Left) ? window.Left : bounds.Right - width - 32;
        var currentTop = double.IsFinite(window.Top) ? window.Top : bounds.Top + 32;
        var newLeft = ClampPosition(currentLeft, bounds.Left, bounds.Right - width);
        var newTop = ClampPosition(currentTop, bounds.Top, bounds.Bottom - height);

        if (Math.Abs(newLeft - window.Left) < 0.5 && Math.Abs(newTop - window.Top) < 0.5)
        {
            return false;
        }

        window.Left = newLeft;
        window.Top = newTop;
        return true;
    }

    public static bool SnapToScreenEdges(Window window, double fallbackWidth, double fallbackHeight, bool enabled)
    {
        if (!enabled || !window.IsLoaded)
        {
            return false;
        }

        var bounds = GetCurrentScreenWorkArea(window, fallbackWidth, fallbackHeight);
        var width = GetWindowWidth(window, fallbackWidth);
        var height = GetWindowHeight(window, fallbackHeight);

        var leftEdge = bounds.Left;
        var topEdge = bounds.Top;
        var rightEdge = bounds.Right - width;
        var bottomEdge = bounds.Bottom - height;

        var snapped = false;

        if (Math.Abs(window.Left - leftEdge) <= SnapThreshold)
        {
            window.Left = leftEdge;
            snapped = true;
        }
        else if (Math.Abs(window.Left - rightEdge) <= SnapThreshold)
        {
            window.Left = rightEdge;
            snapped = true;
        }

        if (Math.Abs(window.Top - topEdge) <= SnapThreshold)
        {
            window.Top = topEdge;
            snapped = true;
        }
        else if (Math.Abs(window.Top - bottomEdge) <= SnapThreshold)
        {
            window.Top = bottomEdge;
            snapped = true;
        }

        return snapped;
    }

    public static void RestoreRightEdge(Window window, double right, double fallbackWidth, double fallbackHeight)
    {
        window.UpdateLayout();
        if (double.IsFinite(right) && window.ActualWidth > 0)
        {
            window.Left = right - window.ActualWidth;
        }

        EnsureOnScreen(window, fallbackWidth, fallbackHeight);
    }

    public static void MoveToDefaultPosition(Window window, double fallbackWidth, double fallbackHeight)
    {
        window.UpdateLayout();
        var bounds = window.IsLoaded
            ? GetCurrentScreenWorkArea(window, fallbackWidth, fallbackHeight)
            : GetVirtualScreenBounds();
        var width = GetWindowWidth(window, fallbackWidth);

        window.Left = bounds.Right - width - 32;
        window.Top = bounds.Top + 32;
        EnsureOnScreen(window, fallbackWidth, fallbackHeight);
    }

    private static double GetWindowWidth(Window window, double fallbackWidth)
    {
        if (window.ActualWidth > 0)
        {
            return window.ActualWidth;
        }

        return double.IsFinite(window.Width) && window.Width > 0 ? window.Width : fallbackWidth;
    }

    private static double GetWindowHeight(Window window, double fallbackHeight)
    {
        if (window.ActualHeight > 0)
        {
            return window.ActualHeight;
        }

        return double.IsFinite(window.Height) && window.Height > 0 ? window.Height : fallbackHeight;
    }

    private static Rect GetVirtualScreenBounds()
    {
        return new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
    }

    private static Rect GetCurrentScreenWorkArea(Window window, double fallbackWidth, double fallbackHeight)
    {
        var scale = GetDpiScale(window);
        var width = Math.Max(1, (int)Math.Round(GetWindowWidth(window, fallbackWidth) * scale.M11));
        var height = Math.Max(1, (int)Math.Round(GetWindowHeight(window, fallbackHeight) * scale.M22));
        var windowBounds = new Drawing.Rectangle(
            (int)Math.Round(window.Left * scale.M11),
            (int)Math.Round(window.Top * scale.M22),
            width,
            height);
        var workArea = Forms.Screen.FromRectangle(windowBounds).WorkingArea;

        return new Rect(
            workArea.Left / scale.M11,
            workArea.Top / scale.M22,
            workArea.Width / scale.M11,
            workArea.Height / scale.M22);
    }

    private static Matrix GetDpiScale(Window window)
    {
        return PresentationSource.FromVisual(window)?.CompositionTarget?.TransformToDevice
            ?? Matrix.Identity;
    }

    private static double ClampPosition(double value, double min, double max)
    {
        return min > max ? min : Math.Clamp(value, min, max);
    }
}
