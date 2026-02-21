using System;
using System.Linq;
using MonoGameLibrary.WFC.Core;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Core;

public enum SingleTile { Floor }

public class WFCAlgorithmTests
{
    [Fact]
    public void WFCAlgorithmConstructor_WhenCreated_StatusIsRunning()
    {
        var grid = new WFCGrid<TestTile>(3, 3);
        var rules = CreateAllConnectRules();
        var algorithm = new WFCAlgorithm<TestTile>(grid, rules, seed: 42);

        Assert.Equal(WFCStatus.Running, algorithm.Status);
    }

    [Fact]
    public void WFCAlgorithmStep_WhenCalled_CollapsesAtLeastOneCell()
    {
        var grid = new WFCGrid<TestTile>(3, 3);
        var rules = CreateAllConnectRules();
        var algorithm = new WFCAlgorithm<TestTile>(grid, rules, seed: 42);
        var initialCollapsedCount = CountCollapsedCells(grid);

        algorithm.Step();

        var newCollapsedCount = CountCollapsedCells(grid);
        Assert.True(newCollapsedCount > initialCollapsedCount);
    }

    [Fact]
    public void WFCAlgorithmRun_WhenRulesAreCompatible_CompletesSuccessfully()
    {
        var grid = new WFCGrid<TestTile>(4, 4);
        var rules = CreateAllConnectRules();
        var algorithm = new WFCAlgorithm<TestTile>(grid, rules, seed: 42);

        var status = algorithm.Run();

        Assert.Equal(WFCStatus.Completed, status);
        Assert.True(grid.IsFullyCollapsed());
    }

    [Fact]
    public void WFCAlgorithmRun_WhenCompleted_AllCellsAreCollapsed()
    {
        var grid = new WFCGrid<TestTile>(4, 4);
        var rules = CreateAllConnectRules();
        var algorithm = new WFCAlgorithm<TestTile>(grid, rules, seed: 42);

        algorithm.Run();

        foreach (var cell in grid.AllCells())
        {
            Assert.True(cell.IsCollapsed);
            Assert.NotNull(cell.CollapsedTile);
        }
    }

    [Fact]
    public void WFCAlgorithmStep_WhenAlreadyCompleted_ReturnsCompleted()
    {
        var grid = new WFCGrid<TestTile>(4, 4);
        var rules = CreateAllConnectRules();
        var algorithm = new WFCAlgorithm<TestTile>(grid, rules, seed: 42);
        algorithm.Run();

        var status = algorithm.Step();

        Assert.Equal(WFCStatus.Completed, status);
    }

    [Fact]
    public void WFCAlgorithmRun_WhenCalledWithSameSeed_ProducesSameResult()
    {
        var rules = CreateAllConnectRules();

        var grid1 = new WFCGrid<TestTile>(4, 4);
        var algorithm1 = new WFCAlgorithm<TestTile>(grid1, rules, seed: 123);
        algorithm1.Run();

        var grid2 = new WFCGrid<TestTile>(4, 4);
        var algorithm2 = new WFCAlgorithm<TestTile>(grid2, rules, seed: 123);
        algorithm2.Run();

        for (int x = 0; x < grid1.Width; x++)
        {
            for (int y = 0; y < grid1.Height; y++)
            {
                var tile1 = grid1.GetCell(x, y).CollapsedTile;
                var tile2 = grid2.GetCell(x, y).CollapsedTile;
                Assert.Equal(tile1, tile2);
            }
        }
    }

    [Fact]
    public void WFCAlgorithmRun_WhenSingleTileEnum_AllCellsGetThatTile()
    {
        var rules = new WFCAdjacencyRules<SingleTile>();
        rules.AddRule(SingleTile.Floor, Direction.North, SingleTile.Floor);
        rules.AddRule(SingleTile.Floor, Direction.East, SingleTile.Floor);
        rules.AddRule(SingleTile.Floor, Direction.South, SingleTile.Floor);
        rules.AddRule(SingleTile.Floor, Direction.West, SingleTile.Floor);

        var grid = new WFCGrid<SingleTile>(3, 3);
        var algorithm = new WFCAlgorithm<SingleTile>(grid, rules, seed: 42);

        var status = algorithm.Run();

        Assert.Equal(WFCStatus.Completed, status);
        foreach (var cell in grid.AllCells())
        {
            Assert.Equal(SingleTile.Floor, cell.CollapsedTile);
        }
    }

    [Fact]
    public void WFCAlgorithmRun_WhenTwoSelfCompatibleTypes_Completes()
    {
        // Two tile types that are each self-compatible but mutually exclusive
        var rules = new WFCAdjacencyRules<TestTile>();

        // Floor only connects to Floor
        rules.AddRule(TestTile.Floor, Direction.North, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.East, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.South, TestTile.Floor);
        rules.AddRule(TestTile.Floor, Direction.West, TestTile.Floor);

        // Wall only connects to Wall
        rules.AddRule(TestTile.Wall, Direction.North, TestTile.Wall);
        rules.AddRule(TestTile.Wall, Direction.East, TestTile.Wall);
        rules.AddRule(TestTile.Wall, Direction.South, TestTile.Wall);
        rules.AddRule(TestTile.Wall, Direction.West, TestTile.Wall);

        var grid = new WFCGrid<TestTile>(2, 2);
        var algorithm = new WFCAlgorithm<TestTile>(grid, rules, seed: 42);

        var status = algorithm.Run();

        // Should complete because propagation forces all cells to same tile type
        Assert.Equal(WFCStatus.Completed, status);
    }

    /// <summary>
    /// Creates rules where every tile type can be adjacent to every tile type in all directions.
    /// </summary>
    private static WFCAdjacencyRules<TestTile> CreateAllConnectRules()
    {
        var rules = new WFCAdjacencyRules<TestTile>();
        var allTiles = Enum.GetValues<TestTile>();
        var allDirections = new[] { Direction.North, Direction.East, Direction.South, Direction.West };

        foreach (var tile in allTiles)
        {
            foreach (var dir in allDirections)
            {
                foreach (var neighbor in allTiles)
                {
                    rules.AddRule(tile, dir, neighbor);
                }
            }
        }

        return rules;
    }

    private static int CountCollapsedCells<T>(WFCGrid<T> grid) where T : struct, Enum
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
