using System;
using MonoGameLibrary.WFC.Core;
using ProceduralDungeon.WFC;

namespace ProceduralDungeon.Passes;

public class LayoutPass : IDungeonPass
{
    private readonly BitmapTile[,] _reference;
    private readonly int _patternSize;
    private readonly int _seed;

    public LayoutPass(BitmapTile[,] reference, int patternSize, int seed)
    {
        _reference = reference;
        _patternSize = patternSize;
        _seed = seed;
    }

    public void Run(DungeonGrid grid)
    {
        var model = new WFCOverlappingModel<BitmapTile>(
            _reference, _patternSize, grid.Width, grid.Height);

        if (model.PatternCount == 0)
            throw new InvalidOperationException("No patterns found in reference grid");

        bool success = model.Run(_seed);
        if (!success)
            throw new InvalidOperationException("WFC contradiction");

        grid.Layout = model.GetOutput();
    }
}
