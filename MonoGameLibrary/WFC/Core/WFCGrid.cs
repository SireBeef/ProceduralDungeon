using System.Collections.Generic;
using System.Linq;
using MonoGameLibrary.WFC.Edges;

namespace MonoGameLibrary.WFC.Core;

public class WFCGrid
{
    private readonly WFCCell[,] _cells;

    public int Width { get; }
    public int Height { get; }

    public WFCGrid(int width, int height, WFCTileSet tileSet)
    {
        Width = width;
        Height = height;
        _cells = new WFCCell[width, height];

        var allVariants = tileSet.Variants;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[x, y] = new WFCCell(x, y, allVariants);
            }
        }
    }

    public WFCCell GetCell(int x, int y)
    {
        return _cells[x, y];
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public WFCCell? GetNeighbor(WFCCell cell, Direction direction)
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

    public IEnumerable<(WFCCell neighbor, Direction direction)> GetNeighbors(WFCCell cell)
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

    public WFCCell? GetLowestEntropyCell()
    {
        WFCCell? lowest = null;
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

    public IEnumerable<WFCCell> AllCells()
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
