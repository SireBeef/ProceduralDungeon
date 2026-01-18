using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameLibrary.WFC.Edges;
using MonoGameLibrary.WFC.Tiles;

namespace MonoGameLibrary.WFC.Core;

public class WFCCell
{
    private readonly HashSet<WFCTileVariant> _possibleVariants;

    public int X { get; }
    public int Y { get; }

    public IReadOnlyCollection<WFCTileVariant> PossibleVariants => _possibleVariants;
    public int Entropy => _possibleVariants.Count;
    public bool IsCollapsed => _possibleVariants.Count == 1;
    public bool IsContradiction => _possibleVariants.Count == 0;

    public WFCTileVariant? CollapsedVariant =>
        IsCollapsed ? _possibleVariants.First() : null;

    public WFCCell(int x, int y, IEnumerable<WFCTileVariant> initialPossibilities)
    {
        X = x;
        Y = y;
        _possibleVariants = new HashSet<WFCTileVariant>(initialPossibilities);
    }

    public bool RemovePossibility(WFCTileVariant variant)
    {
        return _possibleVariants.Remove(variant);
    }

    public int RemoveIncompatibleVariants(Direction direction, WFCEdge neighborEdge)
    {
        var oppositeDirection = direction.Opposite();
        var toRemove = _possibleVariants
            .Where(v => !v.Edges[oppositeDirection].IsCompatible(neighborEdge))
            .ToList();

        foreach (var variant in toRemove)
        {
            _possibleVariants.Remove(variant);
        }

        return toRemove.Count;
    }

    public void Collapse(Random random)
    {
        if (_possibleVariants.Count <= 1)
            return;

        var selected = _possibleVariants.ElementAt(random.Next(_possibleVariants.Count));
        _possibleVariants.Clear();
        _possibleVariants.Add(selected);
    }

    public void CollapseToVariant(WFCTileVariant variant)
    {
        if (!_possibleVariants.Contains(variant))
            throw new InvalidOperationException($"Cannot collapse to variant '{variant.Id}' - not in possibility set");

        _possibleVariants.Clear();
        _possibleVariants.Add(variant);
    }
}
