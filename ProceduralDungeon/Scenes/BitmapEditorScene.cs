using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.WFC.Core;
using ProceduralDungeon.Passes;
using ProceduralDungeon.WFC;

namespace ProceduralDungeon.Scenes;

public class BitmapEditorScene : Scene
{
    private const int REF_WIDTH = 12;
    private const int REF_HEIGHT = 12;
    private const int OUTPUT_WIDTH = 40;
    private const int OUTPUT_HEIGHT = 40;
    private const int CELL_SIZE = 16;
    private const int GAP = 32;

    private static readonly Dictionary<BitmapTile, Color> TileColors = new()
    {
        { BitmapTile.Empty, new Color(40, 40, 40) },
        { BitmapTile.Wall, Color.White },
    };

    // Reference grid the user paints on
    private BitmapTile[,] _referenceGrid;

    // Output from WFC generation
    private BitmapTile?[,] _outputGrid;
    private string _generationStatus = "Not run";
    private int _seed = 42;
    private int _patternSize = 3;
    private int _patternCount = 0;

    // Selected paint tile
    private BitmapTile[] _tileTypes;
    private int _selectedTileIndex = 1; // default to Floor

    // Save/Load state
    private bool _isTypingFilename;
    private string _filenameInput = "";
    private bool _isFileListOpen;
    private string[] _fileList = Array.Empty<string>();
    private int _fileListSelectedIndex;
    private string _statusMessage = "";
    private const string PatternsDir = "Content/patterns";

    // Camera (pan offset)
    private Vector2 _cameraOffset = Vector2.Zero;
    private const float PAN_SPEED = 8f;

    // Rendering
    private Texture2D _pixel;
    private SpriteFont _font;

    public override void Initialize()
    {
        _referenceGrid = new BitmapTile[REF_WIDTH, REF_HEIGHT];
        _outputGrid = new BitmapTile?[OUTPUT_WIDTH, OUTPUT_HEIGHT];
        _tileTypes = Enum.GetValues<BitmapTile>();

        base.Initialize();

        Core.ExitOnEscape = true;
        Core.Instance.IsMouseVisible = true;
    }

    public override void LoadContent()
    {
        _pixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = Core.Content.Load<SpriteFont>("fonts/fps_font");
    }

    public override void Update(GameTime gameTime)
    {
        var kb = Core.Input.Keyboard;
        var mouse = Core.Input.Mouse;

        // Save filename input mode
        if (_isTypingFilename)
        {
            if (kb.WasKeyJustPressed(Keys.Escape))
            {
                ExitTypingMode();
            }
            else if (kb.WasKeyJustPressed(Keys.Enter) && _filenameInput.Length > 0)
            {
                SavePattern(_filenameInput);
                ExitTypingMode();
            }
            return;
        }

        // File list overlay mode
        if (_isFileListOpen)
        {
            if (kb.WasKeyJustPressed(Keys.Escape))
            {
                _isFileListOpen = false;
                Core.ExitOnEscape = true;
            }
            else if (kb.WasKeyJustPressed(Keys.Up) && _fileList.Length > 0)
            {
                _fileListSelectedIndex = (_fileListSelectedIndex - 1 + _fileList.Length) % _fileList.Length;
            }
            else if (kb.WasKeyJustPressed(Keys.Down) && _fileList.Length > 0)
            {
                _fileListSelectedIndex = (_fileListSelectedIndex + 1) % _fileList.Length;
            }
            else if (kb.WasKeyJustPressed(Keys.Enter) && _fileList.Length > 0)
            {
                LoadPattern(Path.Combine(PatternsDir, _fileList[_fileListSelectedIndex] + ".json"));
                _isFileListOpen = false;
                Core.ExitOnEscape = true;
            }
            return;
        }

        // Pan camera
        if (kb.IsKeyDown(Keys.W)) _cameraOffset.Y += PAN_SPEED;
        if (kb.IsKeyDown(Keys.S)) _cameraOffset.Y -= PAN_SPEED;
        if (kb.IsKeyDown(Keys.A)) _cameraOffset.X += PAN_SPEED;
        if (kb.IsKeyDown(Keys.D)) _cameraOffset.X -= PAN_SPEED;

        // Tile selection with scroll wheel
        if (mouse.ScrollWheelDelta != 0)
        {
            int dir = mouse.ScrollWheelDelta > 0 ? 1 : -1;
            _selectedTileIndex = (_selectedTileIndex + dir + _tileTypes.Length) % _tileTypes.Length;
        }

        // Number keys for quick selection
        for (int i = 0; i < _tileTypes.Length && i < 9; i++)
        {
            if (kb.WasKeyJustPressed(Keys.D1 + i))
                _selectedTileIndex = i;
        }

        // Paint on reference grid
        var refPos = ScreenToRefGrid(mouse.Position);
        if (refPos.HasValue)
        {
            if (mouse.IsButtonDown(MouseButton.Left))
                _referenceGrid[refPos.Value.x, refPos.Value.y] = _tileTypes[_selectedTileIndex];

            if (mouse.IsButtonDown(MouseButton.Right))
                _referenceGrid[refPos.Value.x, refPos.Value.y] = BitmapTile.Empty;
        }

        // F2: Save
        if (kb.WasKeyJustPressed(Keys.F2))
            EnterTypingMode();

        // F3: Load
        if (kb.WasKeyJustPressed(Keys.F3))
        {
            _fileList = GetSavedPatterns();
            _fileListSelectedIndex = 0;
            _isFileListOpen = true;
            Core.ExitOnEscape = false;
        }

        // F5: Generate
        if (kb.WasKeyJustPressed(Keys.F5))
            Generate();

        // F6: Clear reference
        if (kb.WasKeyJustPressed(Keys.F6))
        {
            _referenceGrid = new BitmapTile[REF_WIDTH, REF_HEIGHT];
            _generationStatus = "Not run";
        }

        // F7: Re-run with new seed
        if (kb.WasKeyJustPressed(Keys.F7))
        {
            _seed++;
            Generate();
        }

        // F8/F9: Adjust pattern size
        if (kb.WasKeyJustPressed(Keys.F8) && _patternSize > 2)
            _patternSize--;
        if (kb.WasKeyJustPressed(Keys.F9) && _patternSize < 5)
            _patternSize++;

        // F10: View 3D (run Pass 2 pipeline and switch to 3D scene)
        if (kb.WasKeyJustPressed(Keys.F10))
            ViewIn3D();
    }

    private void Generate()
    {
        var model = new WFCOverlappingModel<BitmapTile>(
            _referenceGrid, _patternSize, OUTPUT_WIDTH, OUTPUT_HEIGHT);

        _patternCount = model.PatternCount;

        if (_patternCount == 0)
        {
            _generationStatus = "No patterns - paint something first";
            return;
        }

        bool success = model.Run(_seed);
        _outputGrid = new BitmapTile?[OUTPUT_WIDTH, OUTPUT_HEIGHT];

        if (success)
        {
            var result = model.GetOutput();
            for (int x = 0; x < OUTPUT_WIDTH; x++)
                for (int y = 0; y < OUTPUT_HEIGHT; y++)
                    _outputGrid[x, y] = result[x, y];
        }

        _generationStatus = success
            ? $"OK (seed:{_seed} N:{_patternSize} patterns:{_patternCount})"
            : $"Contradiction (seed:{_seed} N:{_patternSize} patterns:{_patternCount})";
    }

    private void ViewIn3D()
    {
        // Check if we have a generated output
        bool hasOutput = false;
        for (int x = 0; x < OUTPUT_WIDTH && !hasOutput; x++)
            for (int y = 0; y < OUTPUT_HEIGHT && !hasOutput; y++)
                if (_outputGrid[x, y].HasValue) hasOutput = true;

        if (!hasOutput)
        {
            _statusMessage = "Generate first (F5) before viewing 3D";
            return;
        }

        var grid = new DungeonGrid(OUTPUT_WIDTH, OUTPUT_HEIGHT);
        for (int x = 0; x < OUTPUT_WIDTH; x++)
            for (int y = 0; y < OUTPUT_HEIGHT; y++)
                grid.Layout[x, y] = _outputGrid[x, y] ?? BitmapTile.Empty;

        var modelAssignmentPass = new ModelAssignmentPass("Content/passes/model_assignment.json");
        var pipeline = new DungeonPipeline().Add(modelAssignmentPass);
        pipeline.Run(grid);

        Core.ChangeScene(new WFCPlayGroundScene(grid, modelAssignmentPass.TileSize));
    }

    private void EnterTypingMode()
    {
        _isTypingFilename = true;
        _filenameInput = "";
        Core.ExitOnEscape = false;
        Core.Instance.Window.TextInput += OnTextInput;
    }

    private void ExitTypingMode()
    {
        _isTypingFilename = false;
        Core.ExitOnEscape = true;
        Core.Instance.Window.TextInput -= OnTextInput;
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (e.Key == Keys.Back)
        {
            if (_filenameInput.Length > 0)
                _filenameInput = _filenameInput[..^1];
        }
        else if (e.Key != Keys.Enter && e.Key != Keys.Escape && !char.IsControl(e.Character))
        {
            _filenameInput += e.Character;
        }
    }

    private void SavePattern(string name)
    {
        try
        {
            Directory.CreateDirectory(PatternsDir);
            var tiles = new string[REF_WIDTH * REF_HEIGHT];
            for (int y = 0; y < REF_HEIGHT; y++)
                for (int x = 0; x < REF_WIDTH; x++)
                    tiles[y * REF_WIDTH + x] = _referenceGrid[x, y].ToString();

            var data = new { width = REF_WIDTH, height = REF_HEIGHT, tiles };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(PatternsDir, $"{name}.json"), json);
            _statusMessage = $"Saved: {name}";
        }
        catch (Exception ex)
        {
            _statusMessage = $"Save failed: {ex.Message}";
        }
    }

    private void LoadPattern(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            int width = root.GetProperty("width").GetInt32();
            int height = root.GetProperty("height").GetInt32();
            var tiles = root.GetProperty("tiles");

            _referenceGrid = new BitmapTile[REF_WIDTH, REF_HEIGHT];
            int count = Math.Min(tiles.GetArrayLength(), REF_WIDTH * REF_HEIGHT);
            for (int i = 0; i < count; i++)
            {
                int x = i % width;
                int y = i / width;
                if (x < REF_WIDTH && y < REF_HEIGHT)
                {
                    if (Enum.TryParse<BitmapTile>(tiles[i].GetString(), out var tile))
                        _referenceGrid[x, y] = tile;
                }
            }

            _outputGrid = new BitmapTile?[OUTPUT_WIDTH, OUTPUT_HEIGHT];
            _generationStatus = "Not run";
            _statusMessage = $"Loaded: {Path.GetFileNameWithoutExtension(path)}";
        }
        catch (Exception ex)
        {
            _statusMessage = $"Load failed: {ex.Message}";
        }
    }

    private string[] GetSavedPatterns()
    {
        if (!Directory.Exists(PatternsDir))
            return Array.Empty<string>();

        return Directory.GetFiles(PatternsDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n)
            .ToArray();
    }

    private (int x, int y)? ScreenToRefGrid(Point screenPos)
    {
        int ox = (int)_cameraOffset.X + GAP;
        int oy = (int)_cameraOffset.Y + GAP;

        int gx = (screenPos.X - ox) / CELL_SIZE;
        int gy = (screenPos.Y - oy) / CELL_SIZE;

        if (gx >= 0 && gx < REF_WIDTH && gy >= 0 && gy < REF_HEIGHT)
            return (gx, gy);

        return null;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.Begin();

        int refOriginX = (int)_cameraOffset.X + GAP;
        int refOriginY = (int)_cameraOffset.Y + GAP;

        // Draw reference grid label
        Core.SpriteBatch.DrawString(_font, "Reference", new Vector2(refOriginX, refOriginY - 18), Color.Yellow);

        // Draw reference grid
        DrawGrid(_referenceGrid, REF_WIDTH, REF_HEIGHT, refOriginX, refOriginY, true);

        // Draw output grid
        int outputOriginX = refOriginX + REF_WIDTH * CELL_SIZE + GAP;
        int outputOriginY = refOriginY;

        Core.SpriteBatch.DrawString(_font, "Output", new Vector2(outputOriginX, outputOriginY - 18), Color.Yellow);
        DrawOutputGrid(outputOriginX, outputOriginY);

        // Highlight cell under cursor in reference grid
        var mousePos = Core.Input.Mouse.Position;
        var refPos = ScreenToRefGrid(mousePos);
        if (refPos.HasValue)
        {
            DrawCellOutline(refOriginX + refPos.Value.x * CELL_SIZE, refOriginY + refPos.Value.y * CELL_SIZE, Color.Yellow);
        }

        // Draw UI
        var selectedTile = _tileTypes[_selectedTileIndex];
        var selectedColor = TileColors.GetValueOrDefault(selectedTile, Color.Magenta);

        string statusLine = _statusMessage.Length > 0 ? _statusMessage : _generationStatus;
        string helpText = $"Selected: {selectedTile}  |  Pattern size: {_patternSize}x{_patternSize}\n" +
                         $"Status: {statusLine}\n" +
                         $"LMB: Paint | RMB: Erase | Scroll/1-2: Select tile | WASD: Pan\n" +
                         $"F2: Save | F3: Load | F5: Generate | F6: Clear | F7: New seed\n" +
                         $"F8/F9: Pattern size -/+ | F10: View 3D";

        int uiY = refOriginY + Math.Max(REF_HEIGHT, OUTPUT_HEIGHT) * CELL_SIZE + GAP;
        Core.SpriteBatch.DrawString(_font, helpText, new Vector2(refOriginX, uiY), Color.White);

        // Draw selected tile color swatch
        Core.SpriteBatch.Draw(_pixel, new Rectangle(refOriginX + 100, uiY - 2, 14, 14), selectedColor);

        // Draw save prompt
        if (_isTypingFilename)
        {
            int promptY = Core.GraphicsDevice.Viewport.Height - 40;
            Core.SpriteBatch.Draw(_pixel, new Rectangle(0, promptY - 4, Core.GraphicsDevice.Viewport.Width, 30), new Color(0, 0, 0, 200));
            Core.SpriteBatch.DrawString(_font, $"Save as: {_filenameInput}_", new Vector2(10, promptY), Color.Yellow);
        }

        // Draw load file list overlay
        if (_isFileListOpen)
        {
            var viewport = Core.GraphicsDevice.Viewport;
            Core.SpriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, 180));

            int listX = viewport.Width / 2 - 120;
            int listY = 60;
            Core.SpriteBatch.DrawString(_font, "Load Pattern (Enter/Esc)", new Vector2(listX, listY), Color.Yellow);
            listY += 24;

            if (_fileList.Length == 0)
            {
                Core.SpriteBatch.DrawString(_font, "No saved patterns", new Vector2(listX, listY), Color.Gray);
            }
            else
            {
                for (int i = 0; i < _fileList.Length; i++)
                {
                    bool selected = i == _fileListSelectedIndex;
                    if (selected)
                        Core.SpriteBatch.Draw(_pixel, new Rectangle(listX - 4, listY - 2, 248, 20), new Color(80, 80, 160));
                    Core.SpriteBatch.DrawString(_font, _fileList[i], new Vector2(listX, listY), selected ? Color.White : Color.LightGray);
                    listY += 22;
                }
            }
        }

        Core.SpriteBatch.End();
    }

    private void DrawGrid(BitmapTile[,] grid, int width, int height, int originX, int originY, bool drawBorder)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = grid[x, y];
                var color = TileColors.GetValueOrDefault(tile, Color.Magenta);
                var rect = new Rectangle(originX + x * CELL_SIZE, originY + y * CELL_SIZE, CELL_SIZE - 1, CELL_SIZE - 1);
                Core.SpriteBatch.Draw(_pixel, rect, color);
            }
        }

        if (drawBorder)
        {
            var borderRect = new Rectangle(originX - 1, originY - 1, width * CELL_SIZE + 1, height * CELL_SIZE + 1);
            DrawRectOutline(borderRect, Color.Yellow);
        }
    }

    private void DrawOutputGrid(int originX, int originY)
    {
        for (int x = 0; x < OUTPUT_WIDTH; x++)
        {
            for (int y = 0; y < OUTPUT_HEIGHT; y++)
            {
                var tile = _outputGrid[x, y];
                Color color;
                if (tile.HasValue)
                    color = TileColors.GetValueOrDefault(tile.Value, Color.Magenta);
                else
                    color = new Color(20, 20, 20);

                var rect = new Rectangle(originX + x * CELL_SIZE, originY + y * CELL_SIZE, CELL_SIZE - 1, CELL_SIZE - 1);
                Core.SpriteBatch.Draw(_pixel, rect, color);
            }
        }
    }

    private void DrawCellOutline(int x, int y, Color color)
    {
        DrawRectOutline(new Rectangle(x, y, CELL_SIZE - 1, CELL_SIZE - 1), color);
    }

    private void DrawRectOutline(Rectangle rect, Color color)
    {
        // Top
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
        // Bottom
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 1), color);
        // Left
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
        // Right
        Core.SpriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width, rect.Y, 1, rect.Height + 1), color);
    }
}
