using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Manages virtual resolution rendering using a RenderTarget2D.
/// Renders the game at a fixed virtual resolution, then scales it to fit the actual screen.
/// </summary>
public class Resolution
{
    private readonly GraphicsDevice _graphicsDevice;
    private RenderTarget2D _renderTarget;
    private Rectangle _destinationRectangle;

    /// <summary>
    /// Gets the virtual resolution width.
    /// </summary>
    public int VirtualWidth { get; private set; }

    /// <summary>
    /// Gets the virtual resolution height.
    /// </summary>
    public int VirtualHeight { get; private set; }

    /// <summary>
    /// Creates a new Resolution instance.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device to use for rendering.</param>
    /// <param name="virtualWidth">The virtual resolution width.</param>
    /// <param name="virtualHeight">The virtual resolution height.</param>
    public Resolution(GraphicsDevice graphicsDevice, int virtualWidth, int virtualHeight)
    {
        _graphicsDevice = graphicsDevice;
        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;

        // Create the render target at virtual resolution
        _renderTarget = new RenderTarget2D(_graphicsDevice, VirtualWidth, VirtualHeight);

        // Calculate initial destination rectangle
        CalculateDestinationRectangle();
    }

    /// <summary>
    /// Recalculates the destination rectangle for scaling.
    /// Call this when the window/screen size changes.
    /// </summary>
    public void CalculateDestinationRectangle()
    {
        float targetAspectRatio = (float)VirtualWidth / VirtualHeight;
        int width = _graphicsDevice.PresentationParameters.BackBufferWidth;
        int height = (int)(width / targetAspectRatio);

        if (height > _graphicsDevice.PresentationParameters.BackBufferHeight)
        {
            height = _graphicsDevice.PresentationParameters.BackBufferHeight;
            width = (int)(height * targetAspectRatio);
        }

        int x = (_graphicsDevice.PresentationParameters.BackBufferWidth - width) / 2;
        int y = (_graphicsDevice.PresentationParameters.BackBufferHeight - height) / 2;

        _destinationRectangle = new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Begins rendering to the virtual resolution render target.
    /// </summary>
    public void BeginDraw()
    {
        _graphicsDevice.SetRenderTarget(_renderTarget);
        _graphicsDevice.Clear(Color.Black);
    }

    /// <summary>
    /// Ends rendering to the virtual resolution and draws the scaled result to the screen.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
    public void EndDraw(SpriteBatch spriteBatch)
    {
        // Reset render target to back buffer
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.Black);

        // Draw the render target scaled to fit the screen
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_renderTarget, _destinationRectangle, Color.White);
        spriteBatch.End();
    }
}
