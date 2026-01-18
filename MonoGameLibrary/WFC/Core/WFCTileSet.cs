using System.Collections.Generic;
using System.Linq;
using MonoGameLibrary.WFC.Tiles;

namespace MonoGameLibrary.WFC.Core;

public class WFCTileSet
{
    private readonly List<WFCTile> _tiles = new();
    private readonly List<WFCTileVariant> _variants = new();
    private bool _variantsGenerated = false;

    public IReadOnlyList<WFCTile> Tiles => _tiles;
    public IReadOnlyList<WFCTileVariant> Variants
    {
        get
        {
            if (!_variantsGenerated)
            {
                GenerateVariants();
            }
            return _variants;
        }
    }

    public void AddTile(WFCTile tile)
    {
        _tiles.Add(tile);
        _variantsGenerated = false;
    }

    public void AddTiles(IEnumerable<WFCTile> tiles)
    {
        _tiles.AddRange(tiles);
        _variantsGenerated = false;
    }

    private void GenerateVariants()
    {
        _variants.Clear();
        foreach (var tile in _tiles)
        {
            _variants.AddRange(tile.CreateVariants());
        }
        _variantsGenerated = true;
    }

    public WFCTileVariant? GetVariantById(string id)
    {
        return Variants.FirstOrDefault(v => v.Id == id);
    }
}
