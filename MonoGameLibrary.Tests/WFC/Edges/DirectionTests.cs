using MonoGameLibrary.WFC.Edges;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Edges;

public class DirectionTests
{
    [Fact]
    public void DirectionOpposite_WhenInputIsNorth_ReturnsSouth()
    {
        var result = Direction.North.Opposite();
        Assert.Equal(Direction.South, result);
    }

    [Fact]
    public void DirectionOpposite_WhenInputIsSouth_ReturnsNorth()
    {
        var result = Direction.South.Opposite();
        Assert.Equal(Direction.North, result);
    }

    [Fact]
    public void DirectionOpposite_WhenInputIsEast_ReturnsWest()
    {
        var result = Direction.East.Opposite();
        Assert.Equal(Direction.West, result);
    }

    [Fact]
    public void DirectionOpposite_WhenInputIsWest_ReturnsEast()
    {
        var result = Direction.West.Opposite();
        Assert.Equal(Direction.East, result);
    }

    [Fact]
    public void DirectionOpposite_WhenCalledTwiceOnNorth_ReturnsNorth()
    {
        var result = Direction.North.Opposite().Opposite();
        Assert.Equal(Direction.North, result);
    }

    [Fact]
    public void DirectionOpposite_WhenCalledTwiceOnSouth_ReturnsSouth()
    {
        var result = Direction.South.Opposite().Opposite();
        Assert.Equal(Direction.South, result);
    }

    [Fact]
    public void DirectionOpposite_WhenCalledTwiceOnEast_ReturnsEast()
    {
        var result = Direction.East.Opposite().Opposite();
        Assert.Equal(Direction.East, result);
    }

    [Fact]
    public void DirectionOpposite_WhenCalledTwiceOnWest_ReturnsWest()
    {
        var result = Direction.West.Opposite().Opposite();
        Assert.Equal(Direction.West, result);
    }
}
