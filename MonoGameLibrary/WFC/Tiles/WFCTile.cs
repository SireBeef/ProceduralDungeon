using System.Collections.Generic;
using MonoGameLibrary.WFC.Edges;

namespace WFC.Tiles
{
    public sealed class WFCTile
    {
        public string Id { get; }

        // Base edges at rotation = 0
        public Dictionary<Direction, WFCEdge> BaseEdges { get; }

        // Which rotations exist for this tile
        public IReadOnlyList<int> AllowedRotations { get; }

        public string ModelAssetName { get; }

        public WFCTile(
            string id,
            Dictionary<Direction, WFCEdge> baseEdges,
            IReadOnlyList<int> allowedRotations,
            string modelAssetName)
        {
            Id = id;
            BaseEdges = baseEdges;
            AllowedRotations = allowedRotations;
            ModelAssetName = modelAssetName;
        }

        public IEnumerable<WFCTileVariant> CreateVariants()
        {
            foreach (var rot in AllowedRotations)
            {
                yield return new WFCTileVariant(
                    id: $"{Id}_rot{rot}",
                    parentTile: this,
                    rotationDegrees: rot,
                    edges: RotateEdges(BaseEdges, rot),
                    modelAssetName: ModelAssetName
                );
            }
        }

        private static Dictionary<Direction, WFCEdge> RotateEdges(
            Dictionary<Direction, WFCEdge> baseEdges,
            int rotationDegrees)
        {
            int steps = (rotationDegrees / 90) % 4;

            var rotated = new Dictionary<Direction, WFCEdge>();

            foreach (var kvp in baseEdges)
            {
                var newDir = (Direction)(((int)kvp.Key + steps) % 4);
                rotated[newDir] = kvp.Value;
            }

            return rotated;
        }
    }
}

