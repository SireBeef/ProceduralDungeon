using System;
using System.Collections.Generic;

namespace MonoGameLibrary.WFC.Core;

public class WFCGrid<T> where T : struct, Enum
{
    private readonly WFCCell<T>[,] _cells;

    public int Width { get; }
    public int Height { get; }

    public WFCGrid(int width, int height, WFCAdjacencyRules<T> rules)
    {
        Width = width;
        Height = height;
        _cells = new WFCCell<T>[width, height];

        var knownTiles = rules.KnownTiles;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[x, y] = new WFCCell<T>(x, y, knownTiles);
            }
        }
    }

    public WFCCell<T> GetCell(int x, int y)
    {
        return _cells[x, y];
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public WFCCell<T>? GetNeighbor(WFCCell<T> cell, Direction direction)
    {
        int nx = cell.X;
        int ny = cell.Y;

        switch (direction)
        {
            case Direction.North: ny -= 1; break;
            case Direction.East: nx += 1; break;
            case Direction.South: ny += 1; break;
            case Direction.West: nx -= 1; break;
        }

        return IsInBounds(nx, ny) ? _cells[nx, ny] : null;
    }

    public IEnumerable<(WFCCell<T> neighbor, Direction direction)> GetNeighbors(WFCCell<T> cell)
    {
        foreach (Direction dir in new[] { Direction.North, Direction.East, Direction.South, Direction.West })
        {
            var neighbor = GetNeighbor(cell, dir);
            if (neighbor != null)
            {
                yield return (neighbor, dir);
            }
        }
    }

    public bool IsFullyCollapsed()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!_cells[x, y].IsCollapsed)
                    return false;
            }
        }
        return true;
    }

    public bool HasContradiction()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_cells[x, y].IsContradiction)
                    return true;
            }
        }
        return false;
    }

    public WFCCell<T>? GetLowestEntropyCell()
    {
        WFCCell<T>? lowest = null;
        int lowestEntropy = int.MaxValue;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var cell = _cells[x, y];
                if (!cell.IsCollapsed && cell.Entropy > 0 && cell.Entropy < lowestEntropy)
                {
                    lowest = cell;
                    lowestEntropy = cell.Entropy;
                }
            }
        }

        return lowest;
    }

    public IEnumerable<WFCCell<T>> AllCells()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                yield return _cells[x, y];
            }
        }
    }
}
