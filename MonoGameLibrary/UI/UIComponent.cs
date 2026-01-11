using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.UI;

/// <summary>
/// Base class for all UI components that can be drawn on screen.
/// </summary>
public abstract class UIComponent
{
    /// <summary>
    /// Gets or sets whether this component is visible and should be drawn.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Updates this UI component.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>
    /// Draws this UI component.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
    /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime) { }
}
