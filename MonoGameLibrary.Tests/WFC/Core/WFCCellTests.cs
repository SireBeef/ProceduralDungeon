using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameLibrary.WFC.Core;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public class WFCCellTests
{
    private static readonly TestTile[] AllTestTiles = Enum.GetValues<TestTile>();

    [Fact]
    public void WFCCellConstructor_WhenCreated_StoresPosition()
    {
        var cell = new WFCCell<TestTile>(3, 5, AllTestTiles);

        Assert.Equal(3, cell.X);
        Assert.Equal(5, cell.Y);
    }

    [Fact]
    public void WFCCellConstructor_WhenCreated_ContainsGivenPossibilities()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);

        Assert.Equal(AllTestTiles.Length, cell.Entropy);
        foreach (var value in AllTestTiles)
        {
            Assert.Contains(value, cell.PossibleTiles);
        }
    }

    [Fact]
    public void WFCCellConstructor_WhenGivenSubset_OnlyContainsSubset()
    {
        var subset = new[] { TestTile.Floor, TestTile.Wall };
        var cell = new WFCCell<TestTile>(0, 0, subset);

        Assert.Equal(2, cell.Entropy);
        Assert.Contains(TestTile.Floor, cell.PossibleTiles);
        Assert.Contains(TestTile.Wall, cell.PossibleTiles);
        Assert.DoesNotContain(TestTile.Empty, cell.PossibleTiles);
    }

    [Fact]
    public void WFCCellEntropy_WhenMultiplePossibilities_ReturnsCount()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);

        Assert.Equal(3, cell.Entropy);
    }

    [Fact]
    public void WFCCellIsCollapsed_WhenMultiplePossibilities_ReturnsFalse()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);

        Assert.False(cell.IsCollapsed);
    }

    [Fact]
    public void WFCCellIsContradiction_WhenHasPossibilities_ReturnsFalse()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);

        Assert.False(cell.IsContradiction);
    }

    [Fact]
    public void WFCCellIsContradiction_WhenEmpty_ReturnsTrue()
    {
        var cell = new WFCCell<TestTile>(0, 0, Array.Empty<TestTile>());

        Assert.True(cell.IsContradiction);
    }

    [Fact]
    public void WFCCellCollapsedTile_WhenNotCollapsed_ReturnsNull()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);

        Assert.Null(cell.CollapsedTile);
    }

    [Fact]
    public void WFCCellRemovePossibility_WhenTileExists_RemovesIt()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        var initialCount = cell.Entropy;

        var removed = cell.RemovePossibility(TestTile.Empty);

        Assert.True(removed);
        Assert.Equal(initialCount - 1, cell.Entropy);
        Assert.DoesNotContain(TestTile.Empty, cell.PossibleTiles);
    }

    [Fact]
    public void WFCCellRemovePossibility_WhenTileDoesNotExist_ReturnsFalse()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        cell.RemovePossibility(TestTile.Wall);

        var removed = cell.RemovePossibility(TestTile.Wall);

        Assert.False(removed);
    }

    [Fact]
    public void WFCCellCollapse_WhenCalled_LeavesOnePossibility()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        var random = new Random(42);

        cell.Collapse(random);

        Assert.True(cell.IsCollapsed);
        Assert.Equal(1, cell.Entropy);
        Assert.NotNull(cell.CollapsedTile);
    }

    [Fact]
    public void WFCCellCollapse_WhenCalledOnAlreadyCollapsed_DoesNothing()
    {
        var cell = new WFCCell<TestTile>(0, 0, new[] { TestTile.Floor });

        var tile = cell.CollapsedTile;
        cell.Collapse(new Random(42));

        Assert.True(cell.IsCollapsed);
        Assert.Equal(tile, cell.CollapsedTile);
    }

    [Fact]
    public void WFCCellRetainOnly_WhenCalledWithSubset_RemovesOthers()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        var allowed = new HashSet<TestTile> { TestTile.Floor };

        int removed = cell.RetainOnly(allowed);

        Assert.Equal(2, removed);
        Assert.Single(cell.PossibleTiles);
        Assert.Contains(TestTile.Floor, cell.PossibleTiles);
    }

    [Fact]
    public void WFCCellRetainOnly_WhenCalledWithAllTiles_RemovesNothing()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        var allowed = new HashSet<TestTile> { TestTile.Empty, TestTile.Floor, TestTile.Wall };

        int removed = cell.RetainOnly(allowed);

        Assert.Equal(0, removed);
        Assert.Equal(3, cell.Entropy);
    }

    [Fact]
    public void WFCCellRetainOnly_WhenCalledWithEmptySet_RemovesAll()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        var allowed = new HashSet<TestTile>();

        int removed = cell.RetainOnly(allowed);

        Assert.Equal(3, removed);
        Assert.True(cell.IsContradiction);
    }

    [Fact]
    public void WFCCellIsCollapsed_WhenOnlyOneTileRemains_ReturnsTrue()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        cell.RemovePossibility(TestTile.Empty);
        cell.RemovePossibility(TestTile.Wall);

        Assert.True(cell.IsCollapsed);
        Assert.Equal(TestTile.Floor, cell.CollapsedTile);
    }

    [Fact]
    public void WFCCellIsContradiction_WhenAllRemoved_ReturnsTrue()
    {
        var cell = new WFCCell<TestTile>(0, 0, AllTestTiles);
        cell.RemovePossibility(TestTile.Empty);
        cell.RemovePossibility(TestTile.Floor);
        cell.RemovePossibility(TestTile.Wall);

        Assert.True(cell.IsContradiction);
    }
}
