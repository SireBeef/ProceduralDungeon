namespace ProceduralDungeon.Passes;

public interface IDungeonPass
{
    void Run(DungeonGrid grid);
}
