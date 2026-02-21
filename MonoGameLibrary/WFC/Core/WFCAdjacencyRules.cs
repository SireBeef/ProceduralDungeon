using System;
using System.Collections.Generic;

namespace MonoGameLibrary.WFC.Core;

public class WFCAdjacencyRules<T> where T : struct, Enum
{
    private readonly Dictionary<T, Dictionary<Direction, HashSet<T>>> _rules = new();
    private readonly HashSet<T> _knownTiles = new();

    public IReadOnlyCollection<T> KnownTiles => _knownTiles;

    public void AddRule(T tile, Direction direction, T allowedNeighbor)
    {
        _knownTiles.Add(tile);
        _knownTiles.Add(allowedNeighbor);

        if (!_rules.TryGetValue(tile, out var directionRules))
        {
            directionRules = new Dictionary<Direction, HashSet<T>>();
            _rules[tile] = directionRules;
        }

        if (!directionRules.TryGetValue(direction, out var allowed))
        {
            allowed = new HashSet<T>();
            directionRules[direction] = allowed;
        }

        allowed.Add(allowedNeighbor);
    }

    public HashSet<T> GetAllowedNeighbors(T tile, Direction direction)
    {
        if (_rules.TryGetValue(tile, out var directionRules) &&
            directionRules.TryGetValue(direction, out var allowed))
        {
            return allowed;
        }

        return new HashSet<T>();
    }

    public bool IsAllowed(T tile, Direction direction, T neighbor)
    {
        return GetAllowedNeighbors(tile, direction).Contains(neighbor);
    }
}
