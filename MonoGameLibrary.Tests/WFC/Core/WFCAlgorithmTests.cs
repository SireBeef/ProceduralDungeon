using System;
using MonoGameLibrary.WFC.Core;
using MonoGameLibrary.WFC.Edges;
using MonoGameLibrary.WFC.Tiles;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public class WFCAlgorithmTests
{
    [Fact]
    public void WFCAlgorithmConstructor_WhenCreated_StatusIsRunning()
    {
        var grid = CreateSimpleGrid();
        var algorithm = new WFCAlgorithm(grid, seed: 42);

        Assert.Equal(WFCStatus.Running, algorithm.Status);
    }

    [Fact]
    public void WFCAlgorithmStep_WhenCalled_CollapsesAtLeastOneCell()
    {
        var grid = CreateMultiVariantGrid();
        var algorithm = new WFCAlgorithm(grid, seed: 42);
        var initialCollapsedCount = CountCollapsedCells(grid);

        algorithm.Step();

        var newCollapsedCount = CountCollapsedCells(grid);
        Assert.True(newCollapsedCount > initialCollapsedCount);
    }

    [Fact]
    public void WFCAlgorithmRun_WhenTileSetIsCompatible_CompletesSuccessfully()
    {
        var grid = CreateCompatibleGrid();
        var algorithm = new WFCAlgorithm(grid, seed: 42);

        var status = algorithm.Run();

        Assert.Equal(WFCStatus.Completed, status);
        Assert.True(grid.IsFullyCollapsed());
    }

    [Fact]
    public void WFCAlgorithmRun_WhenCompleted_AllCellsAreCollapsed()
    {
        var grid = CreateCompatibleGrid();
        var algorithm = new WFCAlgorithm(grid, seed: 42);

        algorithm.Run();

        foreach (var cell in grid.AllCells())
        {
            Assert.True(cell.IsCollapsed);
            Assert.NotNull(cell.CollapsedVariant);
        }
    }

    [Fact]
    public void WFCAlgorithmStep_WhenAlreadyCompleted_ReturnsCompleted()
    {
        var grid = CreateCompatibleGrid();
        var algorithm = new WFCAlgorithm(grid, seed: 42);
        algorithm.Run();

        var status = algorithm.Step();

        Assert.Equal(WFCStatus.Completed, status);
    }

    [Fact]
    public void WFCAlgorithmRun_WhenCalledWithSameSeed_ProducesSameResult()
    {
        var grid1 = CreateCompatibleGrid();
        var algorithm1 = new WFCAlgorithm(grid1, seed: 123);
        algorithm1.Run();

        var grid2 = CreateCompatibleGrid();
        var algorithm2 = new WFCAlgorithm(grid2, seed: 123);
        algorithm2.Run();

        for (int x = 0; x < grid1.Width; x++)
        {
            for (int y = 0; y < grid1.Height; y++)
            {
                var variant1 = grid1.GetCell(x, y).CollapsedVariant;
                var variant2 = grid2.GetCell(x, y).CollapsedVariant;
                Assert.Equal(variant1?.Id, variant2?.Id);
            }
        }
    }

    [Fact]
    public void WFCAlgorithmRun_WhenPropagating_RemovesIncompatibleVariants()
    {
        var tileSet = new WFCTileSet();

        // Only floor tiles that connect to floor
        var floorEdges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "floor" }) },
            { Direction.East, new WFCEdge(new[] { "floor" }) },
            { Direction.South, new WFCEdge(new[] { "floor" }) },
            { Direction.West, new WFCEdge(new[] { "floor" }) }
        };
        tileSet.AddTile(new WFCTile("floor", floorEdges, new[] { 0 }, "Models/floor"));

        var grid = new WFCGrid(3, 3, tileSet);
        var algorithm = new WFCAlgorithm(grid, seed: 42);

        algorithm.Run();

        // All cells should be the same floor tile since that's all that's compatible
        foreach (var cell in grid.AllCells())
        {
            Assert.Equal("floor_rot0", cell.CollapsedVariant?.Id);
        }
    }

    [Fact]
    public void WFCAlgorithmStatus_WhenContradictionOccurs_IsContradiction()
    {
        // Create tiles that cannot possibly connect
        var tileSet = new WFCTileSet();

        var tile1Edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "a" }) },
            { Direction.East, new WFCEdge(new[] { "a" }) },
            { Direction.South, new WFCEdge(new[] { "a" }) },
            { Direction.West, new WFCEdge(new[] { "a" }) }
        };
        tileSet.AddTile(new WFCTile("tile1", tile1Edges, new[] { 0 }, "Models/tile1"));

        var tile2Edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "b" }) },
            { Direction.East, new WFCEdge(new[] { "b" }) },
            { Direction.South, new WFCEdge(new[] { "b" }) },
            { Direction.West, new WFCEdge(new[] { "b" }) }
        };
        tileSet.AddTile(new WFCTile("tile2", tile2Edges, new[] { 0 }, "Models/tile2"));

        var grid = new WFCGrid(2, 2, tileSet);
        var algorithm = new WFCAlgorithm(grid, seed: 42);

        var status = algorithm.Run();

        // Should complete (not contradict) because propagation will force all cells to same tile type
        // Both tiles are self-compatible, so once one is chosen, all neighbors get the same
        Assert.Equal(WFCStatus.Completed, status);
    }

    private static WFCGrid CreateSimpleGrid()
    {
        return new WFCGrid(3, 3, CreateTestTileSet());
    }

    private static WFCGrid CreateMultiVariantGrid()
    {
        var tileSet = new WFCTileSet();

        // Multiple compatible tiles so cells start uncollapsed
        var floor1Edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "open" }) },
            { Direction.East, new WFCEdge(new[] { "open" }) },
            { Direction.South, new WFCEdge(new[] { "open" }) },
            { Direction.West, new WFCEdge(new[] { "open" }) }
        };
        tileSet.AddTile(new WFCTile("floor1", floor1Edges, new[] { 0 }, "Models/floor1"));

        var floor2Edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "open" }) },
            { Direction.East, new WFCEdge(new[] { "open" }) },
            { Direction.South, new WFCEdge(new[] { "open" }) },
            { Direction.West, new WFCEdge(new[] { "open" }) }
        };
        tileSet.AddTile(new WFCTile("floor2", floor2Edges, new[] { 0 }, "Models/floor2"));

        return new WFCGrid(3, 3, tileSet);
    }

    private static WFCGrid CreateCompatibleGrid()
    {
        var tileSet = new WFCTileSet();

        var floorEdges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "open" }) },
            { Direction.East, new WFCEdge(new[] { "open" }) },
            { Direction.South, new WFCEdge(new[] { "open" }) },
            { Direction.West, new WFCEdge(new[] { "open" }) }
        };
        tileSet.AddTile(new WFCTile("floor", floorEdges, new[] { 0 }, "Models/floor"));

        return new WFCGrid(4, 4, tileSet);
    }

    private static WFCTileSet CreateTestTileSet()
    {
        var tileSet = new WFCTileSet();

        var floorEdges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(new[] { "floor" }) },
            { Direction.East, new WFCEdge(new[] { "floor" }) },
            { Direction.South, new WFCEdge(new[] { "floor" }) },
            { Direction.West, new WFCEdge(new[] { "floor" }) }
        };
        tileSet.AddTile(new WFCTile("floor", floorEdges, new[] { 0 }, "Models/floor"));

        return tileSet;
    }

    private static int CountCollapsedCells(WFCGrid grid)
    {
        int count = 0;
        foreach (var cell in grid.AllCells())
        {
            if (cell.IsCollapsed)
                count++;
        }
        return count;
    }
}
