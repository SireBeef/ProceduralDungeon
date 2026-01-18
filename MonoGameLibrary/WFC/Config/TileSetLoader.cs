using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MonoGameLibrary.WFC.Core;
using MonoGameLibrary.WFC.Edges;
using MonoGameLibrary.WFC.Tiles;

namespace MonoGameLibrary.WFC.Config;

public static class TileSetLoader
{
    public static WFCTileSet LoadFromJson(string jsonContent)
    {
        var definition = JsonSerializer.Deserialize<TileSetDefinition>(jsonContent);
        if (definition == null)
        {
            throw new InvalidDataException("Failed to parse tile set JSON");
        }

        return CreateTileSet(definition);
    }

    public static WFCTileSet LoadFromFile(string filePath)
    {
        var jsonContent = File.ReadAllText(filePath);
        return LoadFromJson(jsonContent);
    }

    private static WFCTileSet CreateTileSet(TileSetDefinition definition)
    {
        var tileSet = new WFCTileSet();

        foreach (var tileDef in definition.Tiles)
        {
            var tile = CreateTile(tileDef);
            tileSet.AddTile(tile);
        }

        return tileSet;
    }

    private static WFCTile CreateTile(TileDefinition tileDef)
    {
        var edges = new Dictionary<Direction, WFCEdge>
        {
            { Direction.North, new WFCEdge(tileDef.Edges.North) },
            { Direction.East, new WFCEdge(tileDef.Edges.East) },
            { Direction.South, new WFCEdge(tileDef.Edges.South) },
            { Direction.West, new WFCEdge(tileDef.Edges.West) }
        };

        return new WFCTile(
            tileDef.Id,
            edges,
            tileDef.Rotations,
            tileDef.Model
        );
    }
}
