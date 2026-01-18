using MonoGameLibrary.WFC.Core;
using MonoGameLibrary.WFC.Edges;
using MonoGameLibrary.WFC.Tiles;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public class WFCTileSetTests
{
    [Fact]
    public void WFCTileSetAddTile_WhenTileAdded_TileAppearsInTilesList()
    {
        var tileSet = new WFCTileSet();
        var tile = CreateFloorTile();

        tileSet.AddTile(tile);

        Assert.Contains(tile, tileSet.Tiles);
    }

    [Fact]
    public void WFCTileSetAddTiles_WhenMultipleTilesAdded_AllTilesAppearInTilesList()
    {
        var tileSet = new WFCTileSet();
        var floor = CreateFloorTile();
        var wall = CreateWallTile();

        tileSet.AddTiles(new[] { floor, wall });

        Assert.Contains(floor, tileSet.Tiles);
        Assert.Contains(wall, tileSet.Tiles);
        Assert.Equal(2, tileSet.Tiles.Count);
    }

    [Fact]
    public void WFCTileSetVariants_WhenTileHasFourRotations_GeneratesFourVariants()
    {
        var tileSet = new WFCTileSet();
        var wall = CreateWallTile();

        tileSet.AddTile(wall);

        Assert.Equal(4, tileSet.Variants.Count);
    }

    [Fact]
    public void WFCTileSetVariants_WhenTileHasOneRotation_GeneratesOneVariant()
    {
        var tileSet = new WFCTileSet();
        var floor = CreateFloorTile();

        tileSet.AddTile(floor);

        Assert.Single(tileSet.Variants);
    }

    [Fact]
    public void WFCTileSetVariants_WhenMultipleTilesAdded_GeneratesVariantsForAll()
    {
        var tileSet = new WFCTileSet();
        var floor = CreateFloorTile();
        var wall = CreateWallTile();

        tileSet.AddTiles(new[] { floor, wall });

        Assert.Equal(5, tileSet.Variants.Count);
    }

    [Fact]
    public void WFCTileSetVariants_WhenAccessedMultipleTimes_ReturnsSameVariants()
    {
        var tileSet = new WFCTileSet();
        tileSet.AddTile(CreateWallTile());

        var firstAccess = tileSet.Variants;
        var secondAccess = tileSet.Variants;

        Assert.Same(firstAccess, secondAccess);
    }

    [Fact]
    public void WFCTileSetVariants_WhenNewTileAddedAfterAccess_RegeneratesVariants()
    {
        var tileSet = new WFCTileSet();
        tileSet.AddTile(CreateFloorTile());
        var initialCount = tileSet.Variants.Count;

        tileSet.AddTile(CreateWallTile());

        Assert.Equal(5, tileSet.Variants.Count);
        Assert.NotEqual(initialCount, tileSet.Variants.Count);
    }

    [Fact]
    public void WFCTileSetGetVariantById_WhenVariantExists_ReturnsVariant()
    {
        var tileSet = new WFCTileSet();
        tileSet.AddTile(CreateWallTile());

        var variant = tileSet.GetVariantById("wall_rot90");

        Assert.NotNull(variant);
        Assert.Equal("wall_rot90", variant.Id);
    }

    [Fact]
    public void WFCTileSetGetVariantById_WhenVariantDoesNotExist_ReturnsNull()
    {
        var tileSet = new WFCTileSet();
        tileSet.AddTile(CreateFloorTile());

        var variant = tileSet.GetVariantById("nonexistent_rot0");

        Assert.Null(variant);
    }

    [Fact]
    public void WFCTileSetTiles_WhenEmpty_ReturnsEmptyList()
    {
        var tileSet = new WFCTileSet();

        Assert.Empty(tileSet.Tiles);
    }

    [Fact]
    public void WFCTileSetVariants_WhenEmpty_ReturnsEmptyList()
    {
        var tileSet = new WFCTileSet();

        Assert.Empty(tileSet.Variants);
    }

    private static WFCTile CreateFloorTile()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "floor" }) },
            { Direction.East, new WFCEdge(new[] { "floor" }) },
            { Direction.South, new WFCEdge(new[] { "floor" }) },
            { Direction.West, new WFCEdge(new[] { "floor" }) }
        };
        return new WFCTile("floor", edges, new[] { 0 }, "Models/floor");
    }

    private static WFCTile CreateWallTile()
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
