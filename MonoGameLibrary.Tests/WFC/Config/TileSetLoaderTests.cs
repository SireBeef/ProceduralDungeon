using MonoGameLibrary.WFC.Config;
using MonoGameLibrary.WFC.Edges;
using Xunit;

namespace MonoGameLibrary.Tests.WFC.Config;

public class TileSetLoaderTests
{
    [Fact]
    public void TileSetLoaderLoadFromJson_WhenValidJson_CreatesTileSet()
    {
        var json = @"{
            ""tiles"": [
                {
                    ""id"": ""floor"",
                    ""model"": ""models/floor"",
                    ""rotations"": [0],
                    ""edges"": {
                        ""north"": [""floor""],
                        ""east"": [""floor""],
                        ""south"": [""floor""],
                        ""west"": [""floor""]
                    }
                }
            ]
        }";

        var tileSet = TileSetLoader.LoadFromJson(json);

        Assert.Single(tileSet.Tiles);
        Assert.Equal("floor", tileSet.Tiles[0].Id);
    }

    [Fact]
    public void TileSetLoaderLoadFromJson_WhenMultipleTiles_LoadsAll()
    {
        var json = @"{
            ""tiles"": [
                {
                    ""id"": ""floor"",
                    ""model"": ""models/floor"",
                    ""rotations"": [0],
                    ""edges"": {
                        ""north"": [""floor""],
                        ""east"": [""floor""],
                        ""south"": [""floor""],
                        ""west"": [""floor""]
                    }
                },
                {
                    ""id"": ""wall"",
                    ""model"": ""models/wall"",
                    ""rotations"": [0, 90, 180, 270],
                    ""edges"": {
                        ""north"": [""empty""],
                        ""east"": [""wall_rot0""],
                        ""south"": [""empty""],
                        ""west"": [""wall_rot0""]
                    }
                }
            ]
        }";

        var tileSet = TileSetLoader.LoadFromJson(json);

        Assert.Equal(2, tileSet.Tiles.Count);
        Assert.Equal(5, tileSet.Variants.Count); // 1 floor + 4 wall rotations
    }

    [Fact]
    public void TileSetLoaderLoadFromJson_WhenMultipleRotations_GeneratesVariants()
    {
        var json = @"{
            ""tiles"": [
                {
                    ""id"": ""wall"",
                    ""model"": ""models/wall"",
                    ""rotations"": [0, 90, 180, 270],
                    ""edges"": {
                        ""north"": [""empty""],
                        ""east"": [""wall_rot0""],
                        ""south"": [""empty""],
                        ""west"": [""wall_rot0""]
                    }
                }
            ]
        }";

        var tileSet = TileSetLoader.LoadFromJson(json);

        Assert.Equal(4, tileSet.Variants.Count);
        Assert.NotNull(tileSet.GetVariantById("wall_rot0"));
        Assert.NotNull(tileSet.GetVariantById("wall_rot90"));
        Assert.NotNull(tileSet.GetVariantById("wall_rot180"));
        Assert.NotNull(tileSet.GetVariantById("wall_rot270"));
    }

    [Fact]
    public void TileSetLoaderLoadFromJson_WhenEdgesHaveMultipleSockets_LoadsAll()
    {
        var json = @"{
            ""tiles"": [
                {
                    ""id"": ""corner"",
                    ""model"": ""models/corner"",
                    ""rotations"": [0],
                    ""edges"": {
                        ""north"": [""wall_rot0"", ""wall_L_rot0""],
                        ""east"": [""floor""],
                        ""south"": [""floor""],
                        ""west"": [""wall_rot0"", ""wall_L_rot0""]
                    }
                }
            ]
        }";

        var tileSet = TileSetLoader.LoadFromJson(json);
        var variant = tileSet.GetVariantById("corner_rot0");

        Assert.NotNull(variant);
        Assert.Equal(2, variant.Edges[Direction.North].Allowed.Count);
        Assert.Contains("wall_rot0", variant.Edges[Direction.North].Allowed);
        Assert.Contains("wall_L_rot0", variant.Edges[Direction.North].Allowed);
    }

    [Fact]
    public void TileSetLoaderLoadFromJson_WhenRotated_SocketsRotateCorrectly()
    {
        var json = @"{
            ""tiles"": [
                {
                    ""id"": ""wall"",
                    ""model"": ""models/wall"",
                    ""rotations"": [0, 90],
                    ""edges"": {
                        ""north"": [""empty""],
                        ""east"": [""wall_rot0""],
                        ""south"": [""empty""],
                        ""west"": [""wall_rot0""]
                    }
                }
            ]
        }";

        var tileSet = TileSetLoader.LoadFromJson(json);
        var rot90Variant = tileSet.GetVariantById("wall_rot90");

        Assert.NotNull(rot90Variant);
        // After 90 degree rotation:
        // - North edge (was empty) -> East edge (still empty)
        // - East edge (was wall_rot0) -> South edge (now wall_rot90)
        Assert.Contains("wall_rot90", rot90Variant.Edges[Direction.South].Allowed);
        Assert.Contains("wall_rot90", rot90Variant.Edges[Direction.North].Allowed);
    }
}
