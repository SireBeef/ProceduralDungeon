using System;
using System.Collections.Generic;

namespace MonoGameLibrary.WFC.Core;

public enum WFCStatus
{
    Running,
    Completed,
    Contradiction
}

public class WFCAlgorithm<T> where T : struct, Enum
{
    private readonly WFCGrid<T> _grid;
    private readonly WFCAdjacencyRules<T> _rules;
    private readonly Random _random;

    public WFCStatus Status { get; private set; } = WFCStatus.Running;

    public WFCAlgorithm(WFCGrid<T> grid, WFCAdjacencyRules<T> rules, Random random)
    {
        _grid = grid;
        _rules = rules;
        _random = random;
    }

    public WFCAlgorithm(WFCGrid<T> grid, WFCAdjacencyRules<T> rules, int seed)
        : this(grid, rules, new Random(seed))
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

    private void Propagate(WFCCell<T> startCell)
    {
        var queue = new Queue<WFCCell<T>>();
        queue.Enqueue(startCell);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();

            foreach (var (neighbor, direction) in _grid.GetNeighbors(cell))
            {
                if (neighbor.IsCollapsed)
                    continue;

                // Compute the union of allowed neighbors for all possible tiles in this cell
                var allowedInNeighbor = new HashSet<T>();
                foreach (var tile in cell.PossibleTiles)
                {
                    foreach (var allowed in _rules.GetAllowedNeighbors(tile, direction))
                    {
                        allowedInNeighbor.Add(allowed);
                    }
                }

                int removed = neighbor.RetainOnly(allowedInNeighbor);

                if (removed > 0 && !neighbor.IsContradiction)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}
