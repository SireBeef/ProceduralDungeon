namespace MonoGameLibrary.WFC.Edges;

public enum SocketType
{
    // These can be any rotation
    Empty,
    Floor,
    Pillar,

    // These have specific rotation
    // Requirements.
    // Order of definition matters here.
    // N E S W <-- always this order.
    PillarWall_N,
    PillarWall_E,
    PillarWall_S,
    PillarWall_W,

    PillarWall_L_N,
    PillarWall_L_E,
    PillarWall_L_S,
    PillarWall_L_W,

    PillarWall_T_N,
    PillarWall_T_E,
    PillarWall_T_S,
    PillarWall_T_W,

    Wall_N,
    Wall_E,
    Wall_S,
    Wall_W,
}
