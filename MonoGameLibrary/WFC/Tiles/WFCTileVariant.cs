using Microsoft.Xna.Framework;
using MonoGameLibrary.WFC.Edges;
using System.Collections.Generic;

namespace MonoGameLibrary.WFC.Tiles
{
    public sealed class WFCTileVariant
    {
        public string Id { get; }
        public WFCTile ParentTile { get; }

        public int RotationDegrees { get; }   // 0, 90, 180, 270
        public Dictionary<Direction, WFCEdge> Edges { get; }

        public string ModelAssetName { get; }

        public WFCTileVariant(
            string id,
            WFCTile parentTile,
            int rotationDegrees,
            Dictionary<Direction, WFCEdge> edges,
            string modelAssetName,
            float weight = 1f)
        {
            Id = id;
            ParentTile = parentTile;
            RotationDegrees = rotationDegrees;
            Edges = edges;
            ModelAssetName = modelAssetName;
        }

        public WFCEdge GetEdge(Direction dir)
            => Edges[dir];
    }
}

