namespace ClockWidget.Tests;

public sealed class WindowPlacementGeometryTests
{
    private static readonly WindowPlacementBounds Bounds = new(Left: 0, Top: 0, Right: 1920, Bottom: 1080);

    [Fact]
    public void ClampIntoBounds_WhenWindowIsOutsideRightAndBottom_ClampsInsideBounds()
    {
        var result = WindowPlacementGeometry.ClampIntoBounds(
            left: 1900,
            top: 1060,
            width: 330,
            height: 118,
            Bounds);

        Assert.True(result.Changed);
        Assert.Equal(1590d, result.Left);
        Assert.Equal(962d, result.Top);
    }

    [Fact]
    public void ClampIntoBounds_WhenWindowIsLargerThanBounds_UsesTopLeft()
    {
        var result = WindowPlacementGeometry.ClampIntoBounds(
            left: 100,
            top: 100,
            width: 2200,
            height: 1200,
            Bounds);

        Assert.True(result.Changed);
        Assert.Equal(0d, result.Left);
        Assert.Equal(0d, result.Top);
    }

    [Fact]
    public void SnapToEdges_WhenNearRightAndTop_SnapsToBothEdges()
    {
        var result = WindowPlacementGeometry.SnapToEdges(
            left: 1580,
            top: 24,
            width: 330,
            height: 118,
            Bounds,
            threshold: 48);

        Assert.True(result.Changed);
        Assert.Equal(1590d, result.Left);
        Assert.Equal(0d, result.Top);
    }

    [Fact]
    public void SnapToEdges_WhenOutsideThreshold_DoesNotMove()
    {
        var result = WindowPlacementGeometry.SnapToEdges(
            left: 1500,
            top: 80,
            width: 330,
            height: 118,
            Bounds,
            threshold: 48);

        Assert.False(result.Changed);
        Assert.Equal(1500d, result.Left);
        Assert.Equal(80d, result.Top);
    }

    [Fact]
    public void GetDefaultPosition_ReturnsTopRightWithOffset()
    {
        var result = WindowPlacementGeometry.GetDefaultPosition(
            width: 330,
            Bounds,
            offset: 32);

        Assert.True(result.Changed);
        Assert.Equal(1558d, result.Left);
        Assert.Equal(32d, result.Top);
    }
}
