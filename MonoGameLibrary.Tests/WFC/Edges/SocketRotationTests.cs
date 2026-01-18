using MonoGameLibrary.WFC.Edges;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Edges;

public class SocketRotationTests
{
    [Fact]
    public void SocketRotationRotate_WhenSocketHasNoRotationSuffix_ReturnsSameSocket()
    {
        var result = SocketRotation.Rotate("empty", 90);
        Assert.Equal("empty", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenSocketIsFloorAndRotated180_ReturnsFloor()
    {
        var result = SocketRotation.Rotate("floor", 180);
        Assert.Equal("floor", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot0RotatedBy90_ReturnsWallRot90()
    {
        var result = SocketRotation.Rotate("wall_rot0", 90);
        Assert.Equal("wall_rot90", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot0RotatedBy180_ReturnsWallRot180()
    {
        var result = SocketRotation.Rotate("wall_rot0", 180);
        Assert.Equal("wall_rot180", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot0RotatedBy270_ReturnsWallRot270()
    {
        var result = SocketRotation.Rotate("wall_rot0", 270);
        Assert.Equal("wall_rot270", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot90RotatedBy90_ReturnsWallRot180()
    {
        var result = SocketRotation.Rotate("wall_rot90", 90);
        Assert.Equal("wall_rot180", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot90RotatedBy180_ReturnsWallRot270()
    {
        var result = SocketRotation.Rotate("wall_rot90", 180);
        Assert.Equal("wall_rot270", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot90RotatedBy270_ReturnsWallRot0()
    {
        var result = SocketRotation.Rotate("wall_rot90", 270);
        Assert.Equal("wall_rot0", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot270RotatedBy90_ReturnsWallRot0()
    {
        var result = SocketRotation.Rotate("wall_rot270", 90);
        Assert.Equal("wall_rot0", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot270RotatedBy180_ReturnsWallRot90()
    {
        var result = SocketRotation.Rotate("wall_rot270", 180);
        Assert.Equal("wall_rot90", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot270RotatedBy270_ReturnsWallRot180()
    {
        var result = SocketRotation.Rotate("wall_rot270", 270);
        Assert.Equal("wall_rot180", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallRot180RotatedBy270_ReturnsWallRot90()
    {
        var result = SocketRotation.Rotate("wall_rot180", 270);
        Assert.Equal("wall_rot90", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallLRot0RotatedBy90_ReturnsWallLRot90()
    {
        var result = SocketRotation.Rotate("wall_L_rot0", 90);
        Assert.Equal("wall_L_rot90", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallLRot180RotatedBy180_ReturnsWallLRot0()
    {
        var result = SocketRotation.Rotate("wall_L_rot180", 180);
        Assert.Equal("wall_L_rot0", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenWallLRot270RotatedBy180_ReturnsWallLRot90()
    {
        var result = SocketRotation.Rotate("wall_L_rot270", 180);
        Assert.Equal("wall_L_rot90", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenRotatedBy0_ReturnsSameSocket()
    {
        var result = SocketRotation.Rotate("wall_rot90", 0);
        Assert.Equal("wall_rot90", result);
    }

    [Fact]
    public void SocketRotationRotate_WhenRotatedBy360_ReturnsSameRotation()
    {
        var result = SocketRotation.Rotate("wall_rot0", 360);
        Assert.Equal("wall_rot0", result);
    }
}
