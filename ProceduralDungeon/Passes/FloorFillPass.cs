using ProceduralDungeon.WFC;

namespace ProceduralDungeon.Passes;

public class FloorFillPass : IDungeonPass
{
    public void Run(DungeonGrid grid)
    {
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                if (grid.Layout[x, y] == BitmapTile.Empty)
                {
                    grid.ModelPlacements.Add(new ModelPlacement
                    {
                        ModelAsset = "models/modular/floor_tile",
                        GridX = x,
                        GridY = y,
                        RotationDegrees = 0
                    });
                }
            }
        }
    }
}
