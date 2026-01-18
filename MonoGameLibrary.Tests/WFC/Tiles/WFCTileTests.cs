using MonoGameLibrary.WFC.Edges;
using MonoGameLibrary.WFC.Tiles;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Tiles;

public class WFCTileTests
{
    [Fact]
    public void WFCTileCreateVariants_WhenTileHasFourRotations_CreatesFourVariants()
    {
        var tile = CreateSimpleWallTile();

        var variants = tile.CreateVariants().ToList();

        Assert.Equal(4, variants.Count);
    }

    [Fact]
    public void WFCTileCreateVariants_WhenTileHasOneRotation_CreatesOneVariant()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "floor" }) },
            { Direction.East, new WFCEdge(new[] { "floor" }) },
            { Direction.South, new WFCEdge(new[] { "floor" }) },
            { Direction.West, new WFCEdge(new[] { "floor" }) }
        };
        var tile = new WFCTile("floor", edges, new[] { 0 }, "Models/floor");

        var variants = tile.CreateVariants().ToList();

        Assert.Single(variants);
    }

    [Fact]
    public void WFCTileCreateVariants_VariantIdsFollowNamingConvention()
    {
        var tile = CreateSimpleWallTile();

        var variants = tile.CreateVariants().ToList();

        Assert.Contains(variants, v => v.Id == "wall_rot0");
        Assert.Contains(variants, v => v.Id == "wall_rot90");
        Assert.Contains(variants, v => v.Id == "wall_rot180");
        Assert.Contains(variants, v => v.Id == "wall_rot270");
    }

    [Fact]
    public void WFCTileCreateVariants_VariantsHaveCorrectRotationDegrees()
    {
        var tile = CreateSimpleWallTile();

        var variants = tile.CreateVariants().ToList();

        Assert.Contains(variants, v => v.RotationDegrees == 0);
        Assert.Contains(variants, v => v.RotationDegrees == 90);
        Assert.Contains(variants, v => v.RotationDegrees == 180);
        Assert.Contains(variants, v => v.RotationDegrees == 270);
    }

    [Fact]
    public void WFCTileCreateVariants_VariantsReferenceParentTile()
    {
        var tile = CreateSimpleWallTile();

        var variants = tile.CreateVariants().ToList();

        Assert.All(variants, v => Assert.Same(tile, v.ParentTile));
    }

    [Fact]
    public void WFCTileCreateVariants_WhenRotated90_EdgePositionsShiftClockwise()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "north_socket" }) },
            { Direction.East, new WFCEdge(new[] { "east_socket" }) },
            { Direction.South, new WFCEdge(new[] { "south_socket" }) },
            { Direction.West, new WFCEdge(new[] { "west_socket" }) }
        };
        var tile = new WFCTile("test", edges, new[] { 0, 90 }, "Models/test");

        var variants = tile.CreateVariants().ToList();
        var rot90Variant = variants.First(v => v.RotationDegrees == 90);

        // North edge should now be on East (N→E after 90° clockwise)
        Assert.Contains("north_socket", rot90Variant.Edges[Direction.East].Allowed);
        // East edge should now be on South (E→S after 90° clockwise)
        Assert.Contains("east_socket", rot90Variant.Edges[Direction.South].Allowed);
        // South edge should now be on West (S→W after 90° clockwise)
        Assert.Contains("south_socket", rot90Variant.Edges[Direction.West].Allowed);
        // West edge should now be on North (W→N after 90° clockwise)
        Assert.Contains("west_socket", rot90Variant.Edges[Direction.North].Allowed);
    }

    [Fact]
    public void WFCTileCreateVariants_WhenRotated90_SocketTypesAlsoRotate()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "empty" }) },
            { Direction.East, new WFCEdge(new[] { "wall_rot0" }) },
            { Direction.South, new WFCEdge(new[] { "empty" }) },
            { Direction.West, new WFCEdge(new[] { "wall_rot0" }) }
        };
        var tile = new WFCTile("wall", edges, new[] { 0, 90 }, "Models/wall");

        var variants = tile.CreateVariants().ToList();
        var rot90Variant = variants.First(v => v.RotationDegrees == 90);

        // The wall_rot0 socket should become wall_rot90 after rotation
        Assert.Contains("wall_rot90", rot90Variant.Edges[Direction.South].Allowed);
        Assert.Contains("wall_rot90", rot90Variant.Edges[Direction.North].Allowed);
    }

    [Fact]
    public void WFCTileCreateVariants_WhenRotated180_EdgePositionsFlip()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "north_socket" }) },
            { Direction.East, new WFCEdge(new[] { "east_socket" }) },
            { Direction.South, new WFCEdge(new[] { "south_socket" }) },
            { Direction.West, new WFCEdge(new[] { "west_socket" }) }
        };
        var tile = new WFCTile("test", edges, new[] { 0, 180 }, "Models/test");

        var variants = tile.CreateVariants().ToList();
        var rot180Variant = variants.First(v => v.RotationDegrees == 180);

        Assert.Contains("north_socket", rot180Variant.Edges[Direction.South].Allowed);
        Assert.Contains("south_socket", rot180Variant.Edges[Direction.North].Allowed);
        Assert.Contains("east_socket", rot180Variant.Edges[Direction.West].Allowed);
        Assert.Contains("west_socket", rot180Variant.Edges[Direction.East].Allowed);
    }

    [Fact]
    public void WFCTileCreateVariants_Rot0Variant_HasUnmodifiedEdgePositions()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "north_socket" }) },
            { Direction.East, new WFCEdge(new[] { "east_socket" }) },
            { Direction.South, new WFCEdge(new[] { "south_socket" }) },
            { Direction.West, new WFCEdge(new[] { "west_socket" }) }
        };
        var tile = new WFCTile("test", edges, new[] { 0 }, "Models/test");

        var variants = tile.CreateVariants().ToList();
        var rot0Variant = variants.First(v => v.RotationDegrees == 0);

        Assert.Contains("north_socket", rot0Variant.Edges[Direction.North].Allowed);
        Assert.Contains("east_socket", rot0Variant.Edges[Direction.East].Allowed);
        Assert.Contains("south_socket", rot0Variant.Edges[Direction.South].Allowed);
        Assert.Contains("west_socket", rot0Variant.Edges[Direction.West].Allowed);
    }

    private static WFCTile CreateSimpleWallTile()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "empty" }) },
            { Direction.East, new WFCEdge(new[] { "wall_rot0" }) },
            { Direction.South, new WFCEdge(new[] { "empty" }) },
            { Direction.West, new WFCEdge(new[] { "wall_rot0" }) }
        };
        return new WFCTile("wall", edges, new[] { 0, 90, 180, 270 }, "Models/wall");
    }
}
