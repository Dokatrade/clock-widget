namespace ClockWidget;

internal static class WindowPlacementGeometry
{
    public static WindowPlacementResult ClampIntoBounds(
        double left,
        double top,
        double width,
        double height,
        WindowPlacementBounds bounds)
    {
        var newLeft = ClampPosition(left, bounds.Left, bounds.Right - width);
        var newTop = ClampPosition(top, bounds.Top, bounds.Bottom - height);
        return new WindowPlacementResult(
            newLeft,
            newTop,
            HasMoved(left, top, newLeft, newTop));
    }

    public static WindowPlacementResult SnapToEdges(
        double left,
        double top,
        double width,
        double height,
        WindowPlacementBounds bounds,
        double threshold)
    {
        var newLeft = left;
        var newTop = top;
        var snapped = false;

        var rightEdge = bounds.Right - width;
        if (Math.Abs(left - bounds.Left) <= threshold)
        {
            newLeft = bounds.Left;
            snapped = true;
        }
        else if (Math.Abs(left - rightEdge) <= threshold)
        {
            newLeft = rightEdge;
            snapped = true;
        }

        var bottomEdge = bounds.Bottom - height;
        if (Math.Abs(top - bounds.Top) <= threshold)
        {
            newTop = bounds.Top;
            snapped = true;
        }
        else if (Math.Abs(top - bottomEdge) <= threshold)
        {
            newTop = bottomEdge;
            snapped = true;
        }

        return new WindowPlacementResult(newLeft, newTop, snapped);
    }

    public static WindowPlacementResult GetDefaultPosition(
        double width,
        WindowPlacementBounds bounds,
        double offset)
    {
        return new WindowPlacementResult(
            bounds.Right - width - offset,
            bounds.Top + offset,
            Changed: true);
    }

    private static double ClampPosition(double value, double min, double max)
    {
        return min > max ? min : Math.Clamp(value, min, max);
    }

    private static bool HasMoved(double oldLeft, double oldTop, double newLeft, double newTop)
    {
        return Math.Abs(newLeft - oldLeft) >= 0.5 || Math.Abs(newTop - oldTop) >= 0.5;
    }
}

internal readonly record struct WindowPlacementBounds(double Left, double Top, double Right, double Bottom);

internal readonly record struct WindowPlacementResult(double Left, double Top, bool Changed);
