using MonoGameLibrary.WFC.Edges;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Edges;

public class WFCEdgeTests
{
    [Fact]
    public void WFCEdgeIsCompatible_WhenBothEdgesShareOneSocket_ReturnsTrue()
    {
        var edgeA = new WFCEdge(new[] { "wall_rot0", "pillar" });
        var edgeB = new WFCEdge(new[] { "pillar", "floor" });

        var result = edgeA.IsCompatible(edgeB);

        Assert.True(result);
    }

    [Fact]
    public void WFCEdgeIsCompatible_WhenBothEdgesShareMultipleSockets_ReturnsTrue()
    {
        var edgeA = new WFCEdge(new[] { "wall_rot0", "pillar", "floor" });
        var edgeB = new WFCEdge(new[] { "pillar", "floor" });

        var result = edgeA.IsCompatible(edgeB);

        Assert.True(result);
    }

    [Fact]
    public void WFCEdgeIsCompatible_WhenEdgesHaveNoSharedSockets_ReturnsFalse()
    {
        var edgeA = new WFCEdge(new[] { "wall_rot0", "pillar" });
        var edgeB = new WFCEdge(new[] { "floor", "empty" });

        var result = edgeA.IsCompatible(edgeB);

        Assert.False(result);
    }

    [Fact]
    public void WFCEdgeIsCompatible_WhenBothEdgesAreIdentical_ReturnsTrue()
    {
        var edgeA = new WFCEdge(new[] { "wall_rot0" });
        var edgeB = new WFCEdge(new[] { "wall_rot0" });

        var result = edgeA.IsCompatible(edgeB);

        Assert.True(result);
    }

    [Fact]
    public void WFCEdgeIsCompatible_WhenOneEdgeIsEmpty_ReturnsFalse()
    {
        var edgeA = new WFCEdge(new[] { "wall_rot0" });
        var edgeB = new WFCEdge(Array.Empty<string>());

        var result = edgeA.IsCompatible(edgeB);

        Assert.False(result);
    }

    [Fact]
    public void WFCEdgeRotate_WhenRotatedBy90_RotatesAllSockets()
    {
        var edge = new WFCEdge(new[] { "wall_rot0", "wall_L_rot0" });

        var rotated = edge.Rotate(90);

        Assert.Contains("wall_rot90", rotated.Allowed);
        Assert.Contains("wall_L_rot90", rotated.Allowed);
        Assert.Equal(2, rotated.Allowed.Count);
    }

    [Fact]
    public void WFCEdgeRotate_WhenContainsRotationAgnosticSocket_LeavesItUnchanged()
    {
        var edge = new WFCEdge(new[] { "wall_rot0", "empty" });

        var rotated = edge.Rotate(90);

        Assert.Contains("wall_rot90", rotated.Allowed);
        Assert.Contains("empty", rotated.Allowed);
    }

    [Fact]
    public void WFCEdgeRotate_WhenRotatedBy0_ReturnsSameSockets()
    {
        var edge = new WFCEdge(new[] { "wall_rot0", "pillar" });

        var rotated = edge.Rotate(0);

        Assert.Contains("wall_rot0", rotated.Allowed);
        Assert.Contains("pillar", rotated.Allowed);
    }

    [Fact]
    public void WFCEdgeRotate_WhenRotatedBy360_ReturnsSameSockets()
    {
        var edge = new WFCEdge(new[] { "wall_rot90" });

        var rotated = edge.Rotate(360);

        Assert.Contains("wall_rot90", rotated.Allowed);
    }

    [Fact]
    public void WFCEdgeRotate_WhenRotationWrapsAround_ReturnsCorrectSockets()
    {
        var edge = new WFCEdge(new[] { "wall_rot270" });

        var rotated = edge.Rotate(180);

        Assert.Contains("wall_rot90", rotated.Allowed);
    }

    [Fact]
    public void WFCEdgeRotate_ReturnsNewEdgeInstance()
    {
        var edge = new WFCEdge(new[] { "wall_rot0" });

        var rotated = edge.Rotate(90);

        Assert.NotSame(edge, rotated);
    }

    [Fact]
    public void WFCEdgeRotate_DoesNotModifyOriginalEdge()
    {
        var edge = new WFCEdge(new[] { "wall_rot0" });

        edge.Rotate(90);

        Assert.Contains("wall_rot0", edge.Allowed);
        Assert.DoesNotContain("wall_rot90", edge.Allowed);
    }
}
