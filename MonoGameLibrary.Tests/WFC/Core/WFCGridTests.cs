using System;
using System.Linq;
using MonoGameLibrary.WFC.Core;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public class WFCGridTests
{
    [Fact]
    public void WFCGridConstructor_WhenCreated_HasCorrectDimensions()
    {
        var grid = new WFCGrid<TestTile>(5, 3, CreateAllConnectRules());

        Assert.Equal(5, grid.Width);
        Assert.Equal(3, grid.Height);
    }

    [Fact]
    public void WFCGridConstructor_WhenCreated_AllCellsHaveKnownTiles()
    {
        var rules = CreateAllConnectRules();
        var grid = new WFCGrid<TestTile>(3, 3, rules);

        var cell = grid.GetCell(1, 1);

        Assert.Equal(rules.KnownTiles.Count, cell.Entropy);
    }

    [Fact]
    public void WFCGridConstructor_WhenRulesHaveSubset_CellsOnlyHaveSubset()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.East, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.South, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.West, TestTile.Floor);

        var grid = new WFCGrid<TestTile>(3, 3, rules);
        var cell = grid.GetCell(0, 0);

        Assert.Equal(1, cell.Entropy);
        Assert.Contains(TestTile.Floor, cell.PossibleTiles);
        Assert.DoesNotContain(TestTile.Wall, cell.PossibleTiles);
    }

    [Fact]
    public void WFCGridGetCell_WhenValidPosition_ReturnsCell()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());

        var cell = grid.GetCell(2, 1);

        Assert.NotNull(cell);
        Assert.Equal(2, cell.X);
        Assert.Equal(1, cell.Y);
    }

    [Fact]
    public void WFCGridIsInBounds_WhenInsideGrid_ReturnsTrue()
    {
        var grid = new WFCGrid<TestTile>(5, 5, CreateAllConnectRules());

        Assert.True(grid.IsInBounds(0, 0));
        Assert.True(grid.IsInBounds(4, 4));
        Assert.True(grid.IsInBounds(2, 3));
    }

    [Fact]
    public void WFCGridIsInBounds_WhenOutsideGrid_ReturnsFalse()
    {
        var grid = new WFCGrid<TestTile>(5, 5, CreateAllConnectRules());

        Assert.False(grid.IsInBounds(-1, 0));
        Assert.False(grid.IsInBounds(0, -1));
        Assert.False(grid.IsInBounds(5, 0));
        Assert.False(grid.IsInBounds(0, 5));
    }

    [Fact]
    public void WFCGridGetNeighbor_WhenNeighborExists_ReturnsNeighbor()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());
        var centerCell = grid.GetCell(1, 1);

        var northNeighbor = grid.GetNeighbor(centerCell, Direction.North);
        var eastNeighbor = grid.GetNeighbor(centerCell, Direction.East);
        var southNeighbor = grid.GetNeighbor(centerCell, Direction.South);
        var westNeighbor = grid.GetNeighbor(centerCell, Direction.West);

        Assert.NotNull(northNeighbor);
        Assert.Equal(1, northNeighbor.X);
        Assert.Equal(0, northNeighbor.Y);

        Assert.NotNull(eastNeighbor);
        Assert.Equal(2, eastNeighbor.X);
        Assert.Equal(1, eastNeighbor.Y);

        Assert.NotNull(southNeighbor);
        Assert.Equal(1, southNeighbor.X);
        Assert.Equal(2, southNeighbor.Y);

        Assert.NotNull(westNeighbor);
        Assert.Equal(0, westNeighbor.X);
        Assert.Equal(1, westNeighbor.Y);
    }

    [Fact]
    public void WFCGridGetNeighbor_WhenAtEdge_ReturnsNull()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());
        var cornerCell = grid.GetCell(0, 0);

        var northNeighbor = grid.GetNeighbor(cornerCell, Direction.North);
        var westNeighbor = grid.GetNeighbor(cornerCell, Direction.West);

        Assert.Null(northNeighbor);
        Assert.Null(westNeighbor);
    }

    [Fact]
    public void WFCGridGetNeighbors_WhenCenterCell_ReturnsFourNeighbors()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());
        var centerCell = grid.GetCell(1, 1);

        var neighbors = grid.GetNeighbors(centerCell).ToList();

        Assert.Equal(4, neighbors.Count);
    }

    [Fact]
    public void WFCGridGetNeighbors_WhenCornerCell_ReturnsTwoNeighbors()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());
        var cornerCell = grid.GetCell(0, 0);

        var neighbors = grid.GetNeighbors(cornerCell).ToList();

        Assert.Equal(2, neighbors.Count);
    }

    [Fact]
    public void WFCGridIsFullyCollapsed_WhenNoCellsCollapsed_ReturnsFalse()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());

        Assert.False(grid.IsFullyCollapsed());
    }

    [Fact]
    public void WFCGridIsFullyCollapsed_WhenAllCellsCollapsed_ReturnsTrue()
    {
        var grid = new WFCGrid<TestTile>(2, 2, CreateAllConnectRules());
        var random = new Random(42);

        foreach (var cell in grid.AllCells())
        {
            cell.Collapse(random);
        }

        Assert.True(grid.IsFullyCollapsed());
    }

    [Fact]
    public void WFCGridHasContradiction_WhenNoContradictions_ReturnsFalse()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());

        Assert.False(grid.HasContradiction());
    }

    [Fact]
    public void WFCGridGetLowestEntropyCell_WhenAllSameEntropy_ReturnsACell()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());

        var lowestCell = grid.GetLowestEntropyCell();

        Assert.NotNull(lowestCell);
    }

    [Fact]
    public void WFCGridGetLowestEntropyCell_WhenOneCellHasLowerEntropy_ReturnsThatCell()
    {
        var grid = new WFCGrid<TestTile>(3, 3, CreateAllConnectRules());
        var targetCell = grid.GetCell(1, 1);

        targetCell.RemovePossibility(TestTile.Empty);

        var lowestCell = grid.GetLowestEntropyCell();

        Assert.Same(targetCell, lowestCell);
    }

    [Fact]
    public void WFCGridGetLowestEntropyCell_WhenAllCollapsed_ReturnsNull()
    {
        var grid = new WFCGrid<TestTile>(2, 2, CreateAllConnectRules());
        var random = new Random(42);

        foreach (var cell in grid.AllCells())
        {
            cell.Collapse(random);
        }

        var lowestCell = grid.GetLowestEntropyCell();

        Assert.Null(lowestCell);
    }

    [Fact]
    public void WFCGridAllCells_ReturnsAllCells()
    {
        var grid = new WFCGrid<TestTile>(3, 4, CreateAllConnectRules());

        var allCells = grid.AllCells().ToList();

        Assert.Equal(12, allCells.Count);
    }

    private static WFCAdjacencyRules<TestTile> CreateAllConnectRules()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        var allTiles = Enum.GetValues<TestTile>();
        var allDirections = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

        foreach (var tile in allTiles)
            foreach (var dir in allDirections)
                foreach (var neighbor in allTiles)
                    rules.AddRule(tile, dir, neighbor);

        return rules;
    }
}
