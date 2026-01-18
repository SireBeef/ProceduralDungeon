using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonoGameLibrary.WFC.Config;

public class TileSetDefinition
{
    [JsonPropertyName("tiles")]
    public List<TileDefinition> Tiles { get; set; } = new();
}

public class TileDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("rotations")]
    public List<int> Rotations { get; set; } = new() { 0 };

    [JsonPropertyName("edges")]
    public EdgeDefinitions Edges { get; set; } = new();
}

public class EdgeDefinitions
{
    [JsonPropertyName("north")]
    public List<string> North { get; set; } = new();

    [JsonPropertyName("east")]
    public List<string> East { get; set; } = new();

    [JsonPropertyName("south")]
    public List<string> South { get; set; } = new();

    [JsonPropertyName("west")]
    public List<string> West { get; set; } = new();
}
