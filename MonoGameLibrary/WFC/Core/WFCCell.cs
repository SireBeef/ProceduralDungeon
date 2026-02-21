using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameLibrary.WFC.Core;

public class WFCCell<T> where T : struct, Enum
{
    private readonly HashSet<T> _possibleTiles;

    public int X { get; }
    public int Y { get; }
    public int Entropy => _possibleTiles.Count;
    public bool IsCollapsed => _possibleTiles.Count == 1;
    public bool IsContradiction => _possibleTiles.Count == 0;
    public T? CollapsedTile => IsCollapsed ? _possibleTiles.First() : null;
    public IReadOnlyCollection<T> PossibleTiles => _possibleTiles;

    public WFCCell(int x, int y)
    {
        X = x;
        Y = y;
        _possibleTiles = new HashSet<T>(Enum.GetValues<T>());
    }

    public void Collapse(Random random)
    {
        if (_possibleTiles.Count <= 1)
            return;

        var selected = _possibleTiles.ElementAt(random.Next(_possibleTiles.Count));
        _possibleTiles.Clear();
        _possibleTiles.Add(selected);
    }

    public bool RemovePossibility(T tile)
    {
        return _possibleTiles.Remove(tile);
    }

    public int RetainOnly(HashSet<T> allowed)
    {
        int removed = 0;
        var toRemove = new List<T>();

        foreach (var tile in _possibleTiles)
        {
            if (!allowed.Contains(tile))
            {
                toRemove.Add(tile);
            }
        }

        foreach (var tile in toRemove)
        {
            _possibleTiles.Remove(tile);
            removed++;
        }

        return removed;
    }
}
