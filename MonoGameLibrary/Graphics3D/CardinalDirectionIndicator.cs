using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics3D;

/// <summary>
/// Renders cardinal direction indicators (N, E, S, W) as colored lines in 3D space.
/// North = Blue (negative Z in MonoGame)
/// East = Red (positive X)
/// South = Cyan (positive Z)
/// West = Green (negative X)
/// Up = Yellow (positive Y)
/// </summary>
public class CardinalDirectionIndicator
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BasicEffect _effect;
    private readonly VertexPositionColor[] _vertices;

    public Vector3 Position { get; set; } = Vector3.Zero;
    public float LineLength { get; set; } = 2f;
    public float ArrowHeadSize { get; set; } = 0.2f;

    public CardinalDirectionIndicator(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        // 5 directions * 2 vertices per line = 10 vertices for main lines
        // Plus arrow heads: 5 directions * 4 vertices (2 lines per arrow head) = 20 vertices
        _vertices = new VertexPositionColor[30];
    }

    public void Draw(Matrix view, Matrix projection)
    {
        BuildVertices();

        _effect.View = view;
        _effect.Projection = projection;
        _effect.World = Matrix.Identity;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(
                PrimitiveType.LineList,
                _vertices,
                0,
                15 // 30 vertices / 2 = 15 lines
            );
        }
    }

    private void BuildVertices()
    {
        int idx = 0;

        // North (negative Z) - Blue
        var northDir = new Vector3(0, 0, -1);
        var northColor = Color.Blue;
        AddDirectionLine(ref idx, northDir, northColor);

        // East (positive X) - Red
        var eastDir = new Vector3(1, 0, 0);
        var eastColor = Color.Red;
        AddDirectionLine(ref idx, eastDir, eastColor);

        // South (positive Z) - Cyan
        var southDir = new Vector3(0, 0, 1);
        var southColor = Color.Cyan;
        AddDirectionLine(ref idx, southDir, southColor);

        // West (negative X) - Green
        var westDir = new Vector3(-1, 0, 0);
        var westColor = Color.Green;
        AddDirectionLine(ref idx, westDir, westColor);

        // Up (positive Y) - Yellow
        var upDir = new Vector3(0, 1, 0);
        var upColor = Color.Yellow;
        AddDirectionLine(ref idx, upDir, upColor);
    }

    private void AddDirectionLine(ref int idx, Vector3 direction, Color color)
    {
        var start = Position;
        var end = Position + direction * LineLength;

        // Main line
        _vertices[idx++] = new VertexPositionColor(start, color);
        _vertices[idx++] = new VertexPositionColor(end, color);

        // Arrow head - two lines forming a V
        var arrowBase = end - direction * ArrowHeadSize;

        // Get perpendicular vectors for arrow head
        Vector3 perp1, perp2;
        if (Math.Abs(direction.Y) < 0.9f)
        {
            perp1 = Vector3.Cross(direction, Vector3.Up);
        }
        else
        {
            perp1 = Vector3.Cross(direction, Vector3.Right);
        }
        perp1.Normalize();
        perp2 = Vector3.Cross(direction, perp1);
        perp2.Normalize();

        var arrowPoint1 = arrowBase + perp1 * ArrowHeadSize * 0.5f;
        var arrowPoint2 = arrowBase - perp1 * ArrowHeadSize * 0.5f;

        _vertices[idx++] = new VertexPositionColor(end, color);
        _vertices[idx++] = new VertexPositionColor(arrowPoint1, color);

        _vertices[idx++] = new VertexPositionColor(end, color);
        _vertices[idx++] = new VertexPositionColor(arrowPoint2, color);
    }
}
