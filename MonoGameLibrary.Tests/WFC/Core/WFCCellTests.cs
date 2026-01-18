using System;
using MonoGameLibrary.WFC.Core;
using MonoGameLibrary.WFC.Edges;
using MonoGameLibrary.WFC.Tiles;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public class WFCCellTests
{
    [Fact]
    public void WFCCellConstructor_WhenCreated_StoresPosition()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(3, 5, variants);

        Assert.Equal(3, cell.X);
        Assert.Equal(5, cell.Y);
    }

    [Fact]
    public void WFCCellConstructor_WhenCreatedWithVariants_ContainsAllVariants()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);

        Assert.Equal(variants.Count, cell.PossibleVariants.Count);
    }

    [Fact]
    public void WFCCellEntropy_WhenMultiplePossibilities_ReturnsCount()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);

        Assert.Equal(variants.Count, cell.Entropy);
    }

    [Fact]
    public void WFCCellIsCollapsed_WhenMultiplePossibilities_ReturnsFalse()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);

        Assert.False(cell.IsCollapsed);
    }

    [Fact]
    public void WFCCellIsCollapsed_WhenOnePossibility_ReturnsTrue()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, new[] { variants[0] });

        Assert.True(cell.IsCollapsed);
    }

    [Fact]
    public void WFCCellIsContradiction_WhenNoPossibilities_ReturnsTrue()
    {
        var cell = new WFCCell(0, 0, Array.Empty<WFCTileVariant>());

        Assert.True(cell.IsContradiction);
    }

    [Fact]
    public void WFCCellIsContradiction_WhenHasPossibilities_ReturnsFalse()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);

        Assert.False(cell.IsContradiction);
    }

    [Fact]
    public void WFCCellCollapsedVariant_WhenCollapsed_ReturnsTheVariant()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, new[] { variants[0] });

        Assert.NotNull(cell.CollapsedVariant);
        Assert.Same(variants[0], cell.CollapsedVariant);
    }

    [Fact]
    public void WFCCellCollapsedVariant_WhenNotCollapsed_ReturnsNull()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);

        Assert.Null(cell.CollapsedVariant);
    }

    [Fact]
    public void WFCCellRemovePossibility_WhenVariantExists_RemovesIt()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);
        var initialCount = cell.Entropy;

        var removed = cell.RemovePossibility(variants[0]);

        Assert.True(removed);
        Assert.Equal(initialCount - 1, cell.Entropy);
        Assert.DoesNotContain(variants[0], cell.PossibleVariants);
    }

    [Fact]
    public void WFCCellRemovePossibility_WhenVariantDoesNotExist_ReturnsFalse()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, new[] { variants[0] });
        var otherVariant = variants[1];

        var removed = cell.RemovePossibility(otherVariant);

        Assert.False(removed);
    }

    [Fact]
    public void WFCCellCollapse_WhenCalled_LeavesOnePossibility()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);
        var random = new Random(42);

        cell.Collapse(random);

        Assert.True(cell.IsCollapsed);
        Assert.Equal(1, cell.Entropy);
    }

    [Fact]
    public void WFCCellCollapse_WhenCalledOnAlreadyCollapsed_DoesNothing()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, new[] { variants[0] });
        var random = new Random(42);

        cell.Collapse(random);

        Assert.True(cell.IsCollapsed);
        Assert.Same(variants[0], cell.CollapsedVariant);
    }

    [Fact]
    public void WFCCellCollapseToVariant_WhenVariantInPossibilities_CollapsesToIt()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, variants);

        cell.CollapseToVariant(variants[1]);

        Assert.True(cell.IsCollapsed);
        Assert.Same(variants[1], cell.CollapsedVariant);
    }

    [Fact]
    public void WFCCellCollapseToVariant_WhenVariantNotInPossibilities_ThrowsException()
    {
        var variants = CreateTestVariants();
        var cell = new WFCCell(0, 0, new[] { variants[0] });

        Assert.Throws<InvalidOperationException>(() => cell.CollapseToVariant(variants[1]));
    }

    [Fact]
    public void WFCCellRemoveIncompatibleVariants_WhenNeighborEdgeIncompatible_RemovesVariants()
    {
        var floorVariant = CreateFloorVariant();
        var wallVariant = CreateWallVariant();
        var cell = new WFCCell(0, 0, new[] { floorVariant, wallVariant });

        // Neighbor to the East has an edge that only accepts "floor"
        var neighborEdge = new WFCEdge(new[] { "floor" });
        var removedCount = cell.RemoveIncompatibleVariants(Direction.East, neighborEdge);

        // Wall variant's West edge has "wall_rot0", not "floor", so it should be removed
        Assert.Equal(1, removedCount);
        Assert.Contains(floorVariant, cell.PossibleVariants);
        Assert.DoesNotContain(wallVariant, cell.PossibleVariants);
    }

    [Fact]
    public void WFCCellRemoveIncompatibleVariants_WhenAllCompatible_RemovesNothing()
    {
        var floorVariant = CreateFloorVariant();
        var cell = new WFCCell(0, 0, new[] { floorVariant });

        var neighborEdge = new WFCEdge(new[] { "floor" });
        var removedCount = cell.RemoveIncompatibleVariants(Direction.East, neighborEdge);

        Assert.Equal(0, removedCount);
        Assert.Single(cell.PossibleVariants);
    }

    private static List<WFCTileVariant> CreateTestVariants()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "test" }) },
            { Direction.East, new WFCEdge(new[] { "test" }) },
            { Direction.South, new WFCEdge(new[] { "test" }) },
            { Direction.West, new WFCEdge(new[] { "test" }) }
        };
        var tile = new WFCTile("test", edges, new[] { 0, 90, 180 }, "Models/test");
        return tile.CreateVariants().ToList();
    }

    private static WFCTileVariant CreateFloorVariant()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "floor" }) },
            { Direction.East, new WFCEdge(new[] { "floor" }) },
            { Direction.South, new WFCEdge(new[] { "floor" }) },
            { Direction.West, new WFCEdge(new[] { "floor" }) }
        };
        var tile = new WFCTile("floor", edges, new[] { 0 }, "Models/floor");
        return tile.CreateVariants().First();
    }

    private static WFCTileVariant CreateWallVariant()
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "empty" }) },
            { Direction.East, new WFCEdge(new[] { "wall_rot0" }) },
            { Direction.South, new WFCEdge(new[] { "empty" }) },
            { Direction.West, new WFCEdge(new[] { "wall_rot0" }) }
        };
        var tile = new WFCTile("wall", edges, new[] { 0 }, "Models/wall");
        return tile.CreateVariants().First();
    }
}
