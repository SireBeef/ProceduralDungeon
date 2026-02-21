using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProceduralDungeon.WFC;

namespace ProceduralDungeon.Passes;

public class DungeonGrid
{
    public int Width { get; }
    public int Height { get; }

    /// <summary>Pass 1 output: the abstract bitmap layout.</summary>
    public BitmapTile[,] Layout { get; set; }

    /// <summary>Pass 2+ output: accumulated 3D model placements.</summary>
    public List<ModelPlacement> ModelPlacements { get; } = new();

    public DungeonGrid(int width, int height)
    {
        Width = width;
        Height = height;
        Layout = new BitmapTile[width, height];
    }
}

public struct ModelPlacement
{
    public string ModelAsset;
    public int GridX;
    public int GridY;
    public int RotationDegrees;

    public Matrix ToWorldMatrix(float tileSize)
    {
        var position = new Vector3(GridX * tileSize, 0, GridY * tileSize);
        var rotation = Matrix.CreateRotationY(
            MathHelper.ToRadians(-RotationDegrees));
        return rotation * Matrix.CreateTranslation(position);
    }
}
