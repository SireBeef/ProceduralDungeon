using Microsoft.Xna.Framework;
using MonoGameLibrary;
using ProceduralDungeon.Scenes;

namespace ProceduralDungeon;

public class Game1 : Core
{

    private static int DEFAULT_RESOLUTION_WIDTH = 1280;
    private static int DEFAULT_RESOLUTION_HEIGHT = 720;



    public Game1() : base("Procedural Dungeon", DEFAULT_RESOLUTION_WIDTH, DEFAULT_RESOLUTION_HEIGHT, true)
    {

    }


    protected override void Initialize()
    {
        base.Initialize();

        ChangeScene(new PlayGroundScene());

    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }


    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

}
