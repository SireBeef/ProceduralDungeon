using System;
using MonoGameLibrary.WFC.Edges;

public static class SocketRotation
{
    public static SocketType Rotate(SocketType socket, int degrees)
    {
        int steps = (degrees / 90) % 4;

        return socket switch
        {
            SocketType.Wall_N => RotateDir(SocketType.Wall_N, steps),
            SocketType.Wall_E => RotateDir(SocketType.Wall_E, steps),
            SocketType.Wall_S => RotateDir(SocketType.Wall_S, steps),
            SocketType.Wall_W => RotateDir(SocketType.Wall_W, steps),

            SocketType.PillarWall_N => RotateDir(SocketType.PillarWall_N, steps),
            SocketType.PillarWall_E => RotateDir(SocketType.PillarWall_E, steps),
            SocketType.PillarWall_S => RotateDir(SocketType.PillarWall_S, steps),
            SocketType.PillarWall_W => RotateDir(SocketType.PillarWall_W, steps),

            SocketType.PillarWall_L_N => RotateDir(SocketType.PillarWall_L_N, steps),
            SocketType.PillarWall_L_E => RotateDir(SocketType.PillarWall_L_E, steps),
            SocketType.PillarWall_L_S => RotateDir(SocketType.PillarWall_L_S, steps),
            SocketType.PillarWall_L_W => RotateDir(SocketType.PillarWall_L_W, steps),

            SocketType.PillarWall_T_N => RotateDir(SocketType.PillarWall_T_N, steps),
            SocketType.PillarWall_T_E => RotateDir(SocketType.PillarWall_T_E, steps),
            SocketType.PillarWall_T_S => RotateDir(SocketType.PillarWall_T_S, steps),
            SocketType.PillarWall_T_W => RotateDir(SocketType.PillarWall_T_W, steps),

            // Fallback case where the orientation
            // just works.  For example: a floor tile which does not care about rotation
            _ => socket
        };
    }

    private static SocketType RotateDir(SocketType socket, int steps)
    {
        int baseIndex = socket switch
        {

            SocketType.PillarWall_N => 0,
            SocketType.PillarWall_E => 1,
            SocketType.PillarWall_S => 2,
            SocketType.PillarWall_W => 3,

            SocketType.PillarWall_L_N => 0,
            SocketType.PillarWall_L_E => 1,
            SocketType.PillarWall_L_S => 2,
            SocketType.PillarWall_L_W => 3,

            SocketType.PillarWall_T_N => 0,
            SocketType.PillarWall_T_E => 1,
            SocketType.PillarWall_T_S => 2,
            SocketType.PillarWall_T_W => 3,

            SocketType.Wall_N => 0,
            SocketType.Wall_E => 1,
            SocketType.Wall_S => 2,
            SocketType.Wall_W => 3,

            _ => throw new InvalidOperationException()
        };

        int newIndex = (baseIndex + steps) % 4;

        return socket switch
        {
            SocketType.PillarWall_N or SocketType.PillarWall_E or SocketType.PillarWall_S or SocketType.PillarWall_W
                => (SocketType)(Enum.Parse(typeof(SocketType), $"PillarWall_{DirFromIndex(newIndex)}")),
            SocketType.PillarWall_L_N or SocketType.PillarWall_L_E or SocketType.PillarWall_L_S or SocketType.PillarWall_L_W
                => (SocketType)(Enum.Parse(typeof(SocketType), $"PillarWall_L_{DirFromIndex(newIndex)}")),
            SocketType.PillarWall_T_N or SocketType.PillarWall_T_E or SocketType.PillarWall_T_S or SocketType.PillarWall_T_W
                => (SocketType)(Enum.Parse(typeof(SocketType), $"PillarWall_T_{DirFromIndex(newIndex)}")),
            SocketType.Wall_N or SocketType.Wall_E or SocketType.Wall_S or SocketType.Wall_W
                => (SocketType)(Enum.Parse(typeof(SocketType), $"Wall_{DirFromIndex(newIndex)}")),

            _ => throw new InvalidOperationException(),
        };
    }

    private static string DirFromIndex(int i)
        => i switch { 0 => "N", 1 => "E", 2 => "S", 3 => "W", _ => "N" };
}
