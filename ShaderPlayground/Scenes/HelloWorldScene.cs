using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.UI;

namespace ShaderPlayground.Scenes;

public class HelloWorldScene : Scene
{


    private Effect _helloWorldShader;
    private Texture2D _renderTexture;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        Core.ExitOnEscape = true;
        Core.Instance.IsMouseVisible = false;

    }

    public override void LoadContent()
    {
        _helloWorldShader = Content.Load<Effect>("shaders/hello-world");

        // Create a texture that matches viewport dimensions
        int width = Core.Graphics.GraphicsDevice.Viewport.Width;
        int height = Core.Graphics.GraphicsDevice.Viewport.Height;
        _renderTexture = new Texture2D(Core.Graphics.GraphicsDevice, width, height);

        // Fill with white pixels
        Color[] data = new Color[width * height];
        for (int i = 0; i < data.Length; i++)
            data[i] = Color.White;
        _renderTexture.SetData(data);
    }

    public override void Update(GameTime gameTime)
    {


    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.Begin(effect: _helloWorldShader);
        Core.SpriteBatch.Draw(_renderTexture,
            new Rectangle(0, 0, Core.Graphics.GraphicsDevice.Viewport.Width, Core.Graphics.GraphicsDevice.Viewport.Height),
            Color.White);
        Core.SpriteBatch.End();
    }
}

