using System;
using System.Collections.Generic;
using MonoGameLibrary.WFC.Edges;

namespace MonoGameLibrary.WFC.Core;

public enum WFCStatus
{
    Running,
    Completed,
    Contradiction
}

public class WFCAlgorithm
{
    private readonly WFCGrid _grid;
    private readonly Random _random;

    public WFCStatus Status { get; private set; } = WFCStatus.Running;

    public WFCAlgorithm(WFCGrid grid, Random random)
    {
        _grid = grid;
        _random = random;
    }

    public WFCAlgorithm(WFCGrid grid, int seed) : this(grid, new Random(seed))
    {
    }

    public WFCAlgorithm(WFCGrid grid) : this(grid, new Random())
    {
    }

    public WFCStatus Run()
    {
        while (Status == WFCStatus.Running)
        {
            Step();
        }
        return Status;
    }

    public WFCStatus Step()
    {
        if (Status != WFCStatus.Running)
            return Status;

        if (_grid.HasContradiction())
        {
            Status = WFCStatus.Contradiction;
            return Status;
        }

        if (_grid.IsFullyCollapsed())
        {
            Status = WFCStatus.Completed;
            return Status;
        }

        var cellToCollapse = _grid.GetLowestEntropyCell();
        if (cellToCollapse == null)
        {
            Status = _grid.IsFullyCollapsed() ? WFCStatus.Completed : WFCStatus.Contradiction;
            return Status;
        }

        cellToCollapse.Collapse(_random);
        Propagate(cellToCollapse);

        if (_grid.HasContradiction())
        {
            Status = WFCStatus.Contradiction;
        }
        else if (_grid.IsFullyCollapsed())
        {
            Status = WFCStatus.Completed;
        }

        return Status;
    }

    private void Propagate(WFCCell startCell)
    {
        var queue = new Queue<WFCCell>();
        queue.Enqueue(startCell);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();

            foreach (var (neighbor, direction) in _grid.GetNeighbors(cell))
            {
                if (neighbor.IsCollapsed)
                    continue;

                var cellEdge = GetCombinedEdge(cell, direction);
                int removed = neighbor.RemoveIncompatibleVariants(direction, cellEdge);

                if (removed > 0 && !neighbor.IsContradiction)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private WFCEdge GetCombinedEdge(WFCCell cell, Direction direction)
    {
        var allSockets = new HashSet<string>();

        foreach (var variant in cell.PossibleVariants)
        {
            foreach (var socket in variant.Edges[direction].Allowed)
            {
                allSockets.Add(socket);
            }
        }

        return new WFCEdge(allSockets);
    }
}
