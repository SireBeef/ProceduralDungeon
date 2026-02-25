using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProceduralDungeon.WFC;

namespace ProceduralDungeon.Passes;

public class ModelAssignmentPass : IDungeonPass
{
    private readonly List<PatternRule> _rules;
    private readonly float _tileSize;

    public ModelAssignmentPass(string rulesJsonPath)
    {
        var json = File.ReadAllText(rulesJsonPath);
        var config = JsonSerializer.Deserialize<RuleConfig>(json);
        _tileSize = config.TileSize;
        _rules = config.Rules;
    }

    public float TileSize => _tileSize;

    public void Run(DungeonGrid grid)
    {
        grid.ModelPlacements.Clear();

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                BitmapTile center = grid.Layout[x, y];
                BitmapTile north = GetTile(grid, x, y - 1);
                BitmapTile east = GetTile(grid, x + 1, y);
                BitmapTile south = GetTile(grid, x, y + 1);
                BitmapTile west = GetTile(grid, x - 1, y);

                foreach (PatternRule rule in _rules)
                {
                    if (rule.Matches(center, north, east, south, west))
                    {
                        grid.ModelPlacements.Add(new ModelPlacement
                        {
                            ModelAsset = rule.Model,
                            GridX = x,
                            GridY = y,
                            RotationDegrees = rule.Rotation
                        });
                        break; // first match wins
                    }
                }
            }
        }
    }

    private static BitmapTile GetTile(DungeonGrid grid, int x, int y)
    {
        if (x < 0 || x >= grid.Width || y < 0 || y >= grid.Height)
            return BitmapTile.Empty;
        return grid.Layout[x, y];
    }
}

public class RuleConfig
{
    [JsonPropertyName("tileSize")]
    public float TileSize { get; set; } = 2f;

    [JsonPropertyName("rules")]
    public List<PatternRule> Rules { get; set; } = new();
}

public class PatternRule
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("center")]
    public string Center { get; set; }

    [JsonPropertyName("north")]
    public string North { get; set; }

    [JsonPropertyName("east")]
    public string East { get; set; }

    [JsonPropertyName("south")]
    public string South { get; set; }

    [JsonPropertyName("west")]
    public string West { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("rotation")]
    public int Rotation { get; set; }

    public bool Matches(BitmapTile center, BitmapTile n, BitmapTile e,
                        BitmapTile s, BitmapTile w)
    {
        return MatchesSingle(Center, center)
            && MatchesSingle(North, n)
            && MatchesSingle(East, e)
            && MatchesSingle(South, s)
            && MatchesSingle(West, w);
    }

    private static bool MatchesSingle(string pattern, BitmapTile actual)
    {
        if (pattern == "*") return true;
        return Enum.TryParse<BitmapTile>(pattern, out var expected)
            && expected == actual;
    }
}
