using System.Collections.Generic;

namespace ProceduralDungeon.Passes;

public class DungeonPipeline
{
    private readonly List<IDungeonPass> _passes = new();

    public DungeonPipeline Add(IDungeonPass pass)
    {
        _passes.Add(pass);
        return this;
    }

    public DungeonGrid Run(int width, int height)
    {
        var grid = new DungeonGrid(width, height);
        Run(grid);
        return grid;
    }

    public void Run(DungeonGrid grid)
    {
        foreach (var pass in _passes)
            pass.Run(grid);
    }
}
