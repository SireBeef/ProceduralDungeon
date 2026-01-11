using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.UI;

/// <summary>
/// UI component that displays the current frames per second.
/// </summary>
public class FpsCounter : UIComponent
{
    private readonly SpriteFont _font;
    private readonly Vector2 _position;
    private readonly Color _color;

    private int _frameCount;
    private double _elapsedTime;
    private int _fps;

    /// <summary>
    /// Creates a new FPS counter.
    /// </summary>
    /// <param name="font">The font to use for rendering the FPS text.</param>
    /// <param name="position">The position on screen to draw the FPS counter.</param>
    /// <param name="color">The color of the FPS text.</param>
    public FpsCounter(SpriteFont font, Vector2 position, Color color)
    {
        _font = font;
        _position = position;
        _color = color;
    }

    /// <summary>
    /// Updates the FPS counter.
    /// </summary>
    public override void Update(GameTime gameTime)
    {
        _frameCount++;
        _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

        if (_elapsedTime >= 1.0)
        {
            _fps = _frameCount;
            _frameCount = 0;
            _elapsedTime = 0;
        }
    }

    /// <summary>
    /// Draws the FPS counter.
    /// </summary>
    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!Visible) return;

        spriteBatch.DrawString(_font, $"FPS: {_fps}", _position, _color);
    }
}
