# Wave Function Collapse Architecture

## Core Concepts

### WFCTile (Definition)
A **WFCTile** is the abstract definition of a tile type. It describes:
- What the tile is (id, model asset)
- Its base edges at rotation 0
- Which rotations are allowed (0, 90, 180, 270)

A WFCTile does NOT exist in the grid directly. Instead, it generates **variants**.

### WFCTileVariant (Concrete Instance)
A **WFCTileVariant** is a specific rotated instance of a tile that can occupy a cell. When a WFCTile generates variants via `CreateVariants()`:

1. For each allowed rotation, a variant is created
2. The variant's edges are pre-calculated by rotating the base edges
3. Variant IDs follow the pattern: `{tileId}_rot{degrees}`

**Example:** A wall tile with rotations [0, 90, 180, 270] produces 4 variants:
- `wall_rot0` (edges as defined)
- `wall_rot90` (edges shifted clockwise, sockets rotated)
- `wall_rot180`
- `wall_rot270`

### WFCCell (Grid Position)
A **WFCCell** represents a single position in the dungeon grid. It contains:
- A **possibility set** of `WFCTileVariant` objects that could occupy this cell
- Entropy (count of possibilities)
- Methods to collapse (select a single variant) and propagate constraints

### Socket System

Sockets are **arbitrary strings** that define connection compatibility. The library is generic - socket names are defined by your tile configuration (eventually JSON).

**Rotation-aware sockets** use the `_rot{N}` suffix convention:
- `wall_rot0`, `wall_rot90`, `wall_rot180`, `wall_rot270`
- `wall_L_rot0`, `wall_L_rot90`, etc.

**Rotation-agnostic sockets** have no suffix:
- `empty`, `floor`, `pillar`

### Rotation System

Two types of rotation occur when generating variants:

**1. Edge Position Rotation** (in WFCTile.RotateEdges)
When rotating a tile, edges shift to new directions:
```
0°:   N→N, E→E, S→S, W→W
90°:  N→E, E→S, S→W, W→N
180°: N→S, E→W, S→N, W→E
270°: N→W, E→N, S→E, W→S
```

**2. Socket String Rotation** (in SocketRotation)
Sockets with `_rot{N}` suffix have their rotation advanced:
```
wall_rot0 + 90° → wall_rot90
wall_rot90 + 90° → wall_rot180
wall_L_rot0 + 180° → wall_L_rot180
```
Sockets without the suffix (e.g., `empty`, `floor`) remain unchanged.

### Edge Compatibility

Two adjacent cells are compatible when their touching edges share at least one common socket string:

```
Cell A (East edge):  {"wall_rot0", "pillar"}
Cell B (West edge):  {"wall_rot0", "pillar"}
                             ↑
                      Match! → Compatible
```

When the WFC algorithm propagates constraints, incompatible variants are removed from possibility sets.

## File Structure

```
WFC/
├── Core/                    # Grid and algorithm (to implement)
│   ├── WFCAlgorithm.cs     # Main WFC loop
│   ├── WFCCell.cs          # Cell with possibility set
│   ├── WFCGrid.cs          # Grid of cells
│   └── WFCTileSet.cs       # Collection of all tiles
├── Edges/                   # Edge/socket system
│   ├── Direction.cs        # N, E, S, W enum + Opposite()
│   ├── SocketRotation.cs   # String-based socket rotation
│   └── WFCEdge.cs          # Edge with allowed socket strings
├── Tiles/                   # Tile definitions
│   ├── WFCTile.cs          # Base tile definition
│   └── WFCTileVariant.cs   # Rotated tile instance
├── Rendering/               # Future: 3D rendering
└── Rules/                   # Future: Custom constraints
```

## Implementation Status

| Component | Status | Notes |
|-----------|--------|-------|
| Direction | Done | Enum + Opposite() extension |
| SocketRotation | Done | Parses `_rot{N}` suffix, advances rotation |
| WFCEdge | Done | HashSet<string>, Rotate(), IsCompatible() |
| WFCTile | Done | CreateVariants() factory |
| WFCTileVariant | Done | Pre-calculated rotated edges |
| WFCCell | Skeleton | Needs possibility set logic |
| WFCGrid | Skeleton | Needs initialization/propagation |
| WFCTileSet | Skeleton | Needs tile management |
| WFCAlgorithm | Skeleton | Needs observe/propagate loop |

## Future: JSON Configuration

Tile definitions will eventually be loaded from JSON:

```json
{
  "tiles": [
    {
      "id": "wall_L",
      "model": "Models/wall_L",
      "rotations": [0, 90, 180, 270],
      "edges": {
        "north": ["wall_rot90", "wall_rot180", "wall_L_rot90"],
        "east": ["wall_rot0", "wall_rot270"],
        "south": ["empty"],
        "west": ["empty"]
      }
    }
  ]
}
```
