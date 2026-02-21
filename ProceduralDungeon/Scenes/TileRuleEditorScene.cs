using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System.Text.Json.Serialization;
using MonoGameLibrary.WFC.Core;

namespace ProceduralDungeon.Scenes;

public class TileRuleEditorScene : Scene
{
    private const int GRID_WIDTH = 20;
    private const int GRID_HEIGHT = 20;
    private const float TILE_SIZE = 2f;
    private const float CAMERA_SPEED = 0.5f;

    // Grid state - stores variant ID or null for empty
    private string?[,] _grid;

    // Available tile variants for placement
    private record EditorTileVariant(string Id, string ModelAssetName, int RotationDegrees);
    private List<EditorTileVariant> _allVariants;
    private int _selectedVariantIndex = 0;

    // Camera
    private Vector3 _cameraPosition;
    private Matrix _viewMatrix;
    private Matrix _projectionMatrix;

    // Models cache
    private Dictionary<string, Model> _modelCache;

    // Input state
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    // UI
    private SpriteFont _font;

    public override void Initialize()
    {
        // Initialize before base.Initialize() since LoadContent is called during base.Initialize()
        _grid = new string?[GRID_WIDTH, GRID_HEIGHT];
        _modelCache = new Dictionary<string, Model>();

        base.Initialize();

        Core.ExitOnEscape = true;
        Core.Instance.IsMouseVisible = true;

        // Set up orthographic camera looking down
        _cameraPosition = new Vector3(GRID_WIDTH * TILE_SIZE / 2, 30f, GRID_HEIGHT * TILE_SIZE / 2);
        UpdateCamera();
    }

    public override void LoadContent()
    {
        // Load tile palette from simplified tiles.json
        var json = File.ReadAllText("Content/tiles.json");
        var palette = JsonSerializer.Deserialize<EditorTilePalette>(json);

        _allVariants = new List<EditorTileVariant>();
        foreach (var tile in palette!.Tiles)
        {
            foreach (var rot in tile.Rotations)
            {
                _allVariants.Add(new EditorTileVariant($"{tile.Id}_rot{rot}", tile.Model, rot));
            }
        }

        // Pre-load all models
        foreach (var variant in _allVariants)
        {
            if (!_modelCache.ContainsKey(variant.ModelAssetName))
            {
                _modelCache[variant.ModelAssetName] = Core.Content.Load<Model>(variant.ModelAssetName);
            }
        }

        _font = Core.Content.Load<SpriteFont>("fonts/fps_font");
    }

    // JSON deserialization classes for the editor tile palette
    private class EditorTilePalette
    {
        [JsonPropertyName("tiles")]
        public List<EditorTileEntry> Tiles { get; set; } = new();
    }

    private class EditorTileEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("rotations")]
        public List<int> Rotations { get; set; } = new() { 0 };
    }

    private void UpdateCamera()
    {
        _viewMatrix = Matrix.CreateLookAt(
            _cameraPosition,
            new Vector3(_cameraPosition.X, 0, _cameraPosition.Z),
            Vector3.Forward  // "Up" is forward when looking down
        );

        float aspectRatio = Core.GraphicsDevice.Viewport.AspectRatio;
        float viewHeight = _cameraPosition.Y * 1.5f;
        float viewWidth = viewHeight * aspectRatio;

        _projectionMatrix = Matrix.CreateOrthographic(viewWidth, viewHeight, 0.1f, 100f);
    }

    public override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        // Camera movement with WASD
        if (keyboardState.IsKeyDown(Keys.W))
            _cameraPosition.Z -= CAMERA_SPEED;
        if (keyboardState.IsKeyDown(Keys.S))
            _cameraPosition.Z += CAMERA_SPEED;
        if (keyboardState.IsKeyDown(Keys.A))
            _cameraPosition.X -= CAMERA_SPEED;
        if (keyboardState.IsKeyDown(Keys.D))
            _cameraPosition.X += CAMERA_SPEED;

        // Zoom with Q/E
        if (keyboardState.IsKeyDown(Keys.Q))
            _cameraPosition.Y = Math.Max(10f, _cameraPosition.Y - CAMERA_SPEED);
        if (keyboardState.IsKeyDown(Keys.E))
            _cameraPosition.Y = Math.Min(100f, _cameraPosition.Y + CAMERA_SPEED);

        UpdateCamera();

        // Tile variant selection with scroll wheel
        int scrollDelta = mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            int direction = scrollDelta > 0 ? 1 : -1;
            _selectedVariantIndex = (_selectedVariantIndex + direction + _allVariants.Count) % _allVariants.Count;
        }

        // Also allow number keys for quick selection (1-9 for first 9 variants)
        for (int i = 0; i < 9 && i < _allVariants.Count; i++)
        {
            if (keyboardState.IsKeyDown(Keys.D1 + i) && !_previousKeyboardState.IsKeyDown(Keys.D1 + i))
            {
                _selectedVariantIndex = i;
            }
        }

        // Get grid position under mouse
        var gridPos = ScreenToGrid(mouseState.Position);

        // Place tile with left click
        if (mouseState.LeftButton == ButtonState.Pressed && gridPos.HasValue)
        {
            var (gx, gy) = gridPos.Value;
            _grid[gx, gy] = _allVariants[_selectedVariantIndex].Id;
        }

        // Remove tile with right click
        if (mouseState.RightButton == ButtonState.Pressed && gridPos.HasValue)
        {
            var (gx, gy) = gridPos.Value;
            _grid[gx, gy] = null;
        }

        // Export rules with F5
        if (keyboardState.IsKeyDown(Keys.F5) && !_previousKeyboardState.IsKeyDown(Keys.F5))
        {
            ExportRules();
        }

        // Clear grid with F6
        if (keyboardState.IsKeyDown(Keys.F6) && !_previousKeyboardState.IsKeyDown(Keys.F6))
        {
            _grid = new string?[GRID_WIDTH, GRID_HEIGHT];
            Console.WriteLine("Grid cleared.");
        }

        _previousMouseState = mouseState;
        _previousKeyboardState = keyboardState;
    }

    private (int x, int y)? ScreenToGrid(Point screenPos)
    {
        // Create a ray from the screen position
        var viewport = Core.GraphicsDevice.Viewport;

        Vector3 nearPoint = viewport.Unproject(
            new Vector3(screenPos.X, screenPos.Y, 0),
            _projectionMatrix, _viewMatrix, Matrix.Identity);

        Vector3 farPoint = viewport.Unproject(
            new Vector3(screenPos.X, screenPos.Y, 1),
            _projectionMatrix, _viewMatrix, Matrix.Identity);

        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

        // Intersect with Y=0 plane
        if (Math.Abs(direction.Y) < 0.0001f)
            return null;

        float t = -nearPoint.Y / direction.Y;
        if (t < 0)
            return null;

        Vector3 worldPos = nearPoint + direction * t;

        int gx = (int)Math.Floor(worldPos.X / TILE_SIZE);
        int gy = (int)Math.Floor(worldPos.Z / TILE_SIZE);

        if (gx >= 0 && gx < GRID_WIDTH && gy >= 0 && gy < GRID_HEIGHT)
            return (gx, gy);

        return null;
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw grid lines
        DrawGridLines();

        // Draw placed tiles
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                var variantId = _grid[x, y];
                if (variantId != null)
                {
                    var variant = _allVariants.FirstOrDefault(v => v.Id == variantId);
                    if (variant != null)
                    {
                        DrawTile(variant, x, y);
                    }
                }
            }
        }

        // Draw UI
        Core.SpriteBatch.Begin();

        var selectedVariant = _allVariants[_selectedVariantIndex];
        string helpText = $"Selected: {selectedVariant.Id} (scroll to change)\n" +
                         $"Left click: Place | Right click: Remove\n" +
                         $"WASD: Pan | Q/E: Zoom\n" +
                         $"F5: Export rules | F6: Clear grid\n" +
                         $"Variants: {_selectedVariantIndex + 1}/{_allVariants.Count}";

        Core.SpriteBatch.DrawString(_font, helpText, new Vector2(10, 10), Color.Yellow);

        // Show grid position under mouse
        var mousePos = Mouse.GetState().Position;
        var gridPos = ScreenToGrid(mousePos);
        if (gridPos.HasValue)
        {
            string posText = $"Grid: ({gridPos.Value.x}, {gridPos.Value.y})";
            Core.SpriteBatch.DrawString(_font, posText, new Vector2(10, 120), Color.White);
        }

        Core.SpriteBatch.End();
    }

    private void DrawGridLines()
    {
        var effect = new BasicEffect(Core.GraphicsDevice)
        {
            VertexColorEnabled = true,
            View = _viewMatrix,
            Projection = _projectionMatrix,
            World = Matrix.Identity
        };

        var vertices = new List<VertexPositionColor>();

        // Draw grid lines
        for (int x = 0; x <= GRID_WIDTH; x++)
        {
            vertices.Add(new VertexPositionColor(new Vector3(x * TILE_SIZE, 0.01f, 0), Color.Gray));
            vertices.Add(new VertexPositionColor(new Vector3(x * TILE_SIZE, 0.01f, GRID_HEIGHT * TILE_SIZE), Color.Gray));
        }

        for (int y = 0; y <= GRID_HEIGHT; y++)
        {
            vertices.Add(new VertexPositionColor(new Vector3(0, 0.01f, y * TILE_SIZE), Color.Gray));
            vertices.Add(new VertexPositionColor(new Vector3(GRID_WIDTH * TILE_SIZE, 0.01f, y * TILE_SIZE), Color.Gray));
        }

        if (vertices.Count > 0)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.ToArray(), 0, vertices.Count / 2);
            }
        }
    }

    private void DrawTile(EditorTileVariant variant, int gridX, int gridY)
    {
        if (!_modelCache.TryGetValue(variant.ModelAssetName, out var model))
            return;

        Vector3 position = new Vector3(gridX * TILE_SIZE, 0, gridY * TILE_SIZE);
        Matrix rotation = Matrix.CreateRotationY(MathHelper.ToRadians(-variant.RotationDegrees));
        Matrix world = rotation * Matrix.CreateTranslation(position);

        foreach (var mesh in model.Meshes)
        {
            Matrix meshWorld = mesh.ParentBone.Transform * world;
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = meshWorld;
                effect.View = _viewMatrix;
                effect.Projection = _projectionMatrix;
                effect.EnableDefaultLighting();
            }
            mesh.Draw();
        }
    }

    private const string EMPTY_VARIANT_ID = "empty_rot0";
    private const string EMPTY_MODEL = "empty";

    private void ExportRules()
    {
        // Collect adjacency information from the grid — no un-rotation needed.
        // Each variant is its own explicit entry with its actual neighbors.
        // Key: (variantId, direction) -> Set of neighbor variant IDs
        var adjacencies = new Dictionary<(string variantId, Direction dir), HashSet<string>>();

        // Track variant info: variantId -> (model, rotation)
        var variantInfo = new Dictionary<string, (string model, int rotation)>();

        // Add empty variant info
        variantInfo[EMPTY_VARIANT_ID] = (EMPTY_MODEL, 0);

        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                var variantId = _grid[x, y];

                // Determine this cell's variant ID (null grid cells are empty)
                string cellVariantId = variantId ?? EMPTY_VARIANT_ID;

                // Track variant info if it's a placed tile
                if (variantId != null)
                {
                    var variant = _allVariants.FirstOrDefault(v => v.Id == variantId);
                    if (variant != null && !variantInfo.ContainsKey(variantId))
                    {
                        variantInfo[variantId] = (variant.ModelAssetName, variant.RotationDegrees);
                    }
                }

                // Check each neighbor — record exactly what's there
                CheckNeighborDirect(cellVariantId, x, y - 1, Direction.North, adjacencies);
                CheckNeighborDirect(cellVariantId, x + 1, y, Direction.East, adjacencies);
                CheckNeighborDirect(cellVariantId, x, y + 1, Direction.South, adjacencies);
                CheckNeighborDirect(cellVariantId, x - 1, y, Direction.West, adjacencies);
            }
        }

        // Build output: one entry per variant ID
        Console.WriteLine("\n=== Extracted Adjacency Rules (per-variant) ===\n");

        var allVariantIds = adjacencies.Keys.Select(k => k.variantId).Distinct().OrderBy(x => x).ToList();

        foreach (var vid in allVariantIds)
        {
            var north = adjacencies.GetValueOrDefault((vid, Direction.North), new HashSet<string>());
            var east = adjacencies.GetValueOrDefault((vid, Direction.East), new HashSet<string>());
            var south = adjacencies.GetValueOrDefault((vid, Direction.South), new HashSet<string>());
            var west = adjacencies.GetValueOrDefault((vid, Direction.West), new HashSet<string>());

            Console.WriteLine($"\"{vid}\": {{");
            Console.WriteLine($"  north: [{string.Join(", ", north.OrderBy(x => x).Select(x => $"\"{x}\""))}]");
            Console.WriteLine($"  east: [{string.Join(", ", east.OrderBy(x => x).Select(x => $"\"{x}\""))}]");
            Console.WriteLine($"  south: [{string.Join(", ", south.OrderBy(x => x).Select(x => $"\"{x}\""))}]");
            Console.WriteLine($"  west: [{string.Join(", ", west.OrderBy(x => x).Select(x => $"\"{x}\""))}]");
            Console.WriteLine("}");
        }

        // Save to file
        string outputPath = "Content/extracted_rules.json";
        var jsonOutput = new
        {
            tiles = allVariantIds.Select(vid =>
            {
                var info = variantInfo.GetValueOrDefault(vid, ("models/unknown", 0));
                return new
                {
                    id = vid,
                    model = info.Item1,
                    rotation = info.Item2,
                    edges = new
                    {
                        north = adjacencies.GetValueOrDefault((vid, Direction.North), new HashSet<string>()).OrderBy(x => x).ToList(),
                        east = adjacencies.GetValueOrDefault((vid, Direction.East), new HashSet<string>()).OrderBy(x => x).ToList(),
                        south = adjacencies.GetValueOrDefault((vid, Direction.South), new HashSet<string>()).OrderBy(x => x).ToList(),
                        west = adjacencies.GetValueOrDefault((vid, Direction.West), new HashSet<string>()).OrderBy(x => x).ToList()
                    }
                };
            }).ToList()
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(jsonOutput, options);
        File.WriteAllText(outputPath, json);

        Console.WriteLine($"\nRules exported to {outputPath}");
    }

    private void CheckNeighborDirect(string cellVariantId, int nx, int ny, Direction direction,
        Dictionary<(string, Direction), HashSet<string>> adjacencies)
    {
        if (nx < 0 || nx >= GRID_WIDTH || ny < 0 || ny >= GRID_HEIGHT)
            return;

        var key = (cellVariantId, direction);
        if (!adjacencies.ContainsKey(key))
            adjacencies[key] = new HashSet<string>();

        var neighborVariantId = _grid[nx, ny] ?? EMPTY_VARIANT_ID;
        adjacencies[key].Add(neighborVariantId);
    }
}
