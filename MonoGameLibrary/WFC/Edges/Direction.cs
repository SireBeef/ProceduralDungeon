using System;

namespace MonoGameLibrary.WFC.Edges;

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

// Utility: get opposite direction
public static class DirectionExtensions
{
    public static Direction Opposite(this Direction dir) => dir switch
    {
        Direction.North => Direction.South,
        Direction.East => Direction.West,
        Direction.South => Direction.North,
        Direction.West => Direction.East,
        _ => throw new ArgumentOutOfRangeException()
    };
}
