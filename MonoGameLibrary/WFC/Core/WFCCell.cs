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

    /// <summary>
    /// Removes variants that are incompatible with the source cell's variants.
    /// A variant is compatible if there exists at least one source variant where:
    /// - The source's edge (facing this cell) accepts this variant's ID
    /// - This variant's edge (facing source) accepts the source variant's ID
    /// </summary>
    /// <param name="fromDirection">Direction FROM source TO this cell</param>
    /// <param name="sourceVariants">The possible variants in the source cell</param>
    public int RemoveIncompatibleVariants(Direction fromDirection, IReadOnlyCollection<WFCTileVariant> sourceVariants)
    {
        Direction toSource = fromDirection.Opposite();

        var toRemove = _possibleVariants
            .Where(myVariant => !IsCompatibleWithAny(myVariant, toSource, fromDirection, sourceVariants))
            .ToList();

        foreach (var variant in toRemove)
        {
            _possibleVariants.Remove(variant);
        }

        return toRemove.Count;
    }

    private bool IsCompatibleWithAny(
        WFCTileVariant myVariant,
        Direction toSource,
        Direction fromSource,
        IReadOnlyCollection<WFCTileVariant> sourceVariants)
    {
        foreach (var sourceVariant in sourceVariants)
        {
            // Source's edge facing me must accept my ID
            bool sourceAcceptsMe = sourceVariant.Edges[fromSource].Accepts(myVariant.Id);

            // My edge facing source must accept source's ID
            bool iAcceptSource = myVariant.Edges[toSource].Accepts(sourceVariant.Id);

            if (sourceAcceptsMe && iAcceptSource)
                return true;
        }

        return false;
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
