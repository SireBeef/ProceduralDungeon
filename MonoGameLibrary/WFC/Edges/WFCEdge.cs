using System.Collections.Generic;

namespace MonoGameLibrary.WFC.Edges;

public class WFCEdge
{
    public HashSet<string> Allowed { get; }

    public WFCEdge(IEnumerable<string> allowed)
    {
        Allowed = new HashSet<string>(allowed);
    }

    public WFCEdge Rotate(int degrees)
    {
        var rotated = new HashSet<string>();

        foreach (var socket in Allowed)
            rotated.Add(SocketRotation.Rotate(socket, degrees));

        return new WFCEdge(rotated);
    }

    // Checks compatibility with another edge (legacy socket overlap check)
    public bool IsCompatible(WFCEdge other)
    {
        // Overlaps returns true if there
        // is at least one match.
        return Allowed.Overlaps(other.Allowed);
    }

    /// <summary>
    /// Checks if this edge accepts the given variant ID.
    /// Matches if:
    /// - The exact variant ID is in the allowed list (e.g., "wall_rot0")
    /// - The base tile ID is in the allowed list (e.g., "wall" matches "wall_rot90")
    /// </summary>
    public bool Accepts(string variantId)
    {
        // Exact match (e.g., allowed has "wall_rot0" and variantId is "wall_rot0")
        if (Allowed.Contains(variantId))
            return true;

        // Base ID match (e.g., allowed has "wall" and variantId is "wall_rot90")
        string baseId = SocketRotation.GetBaseName(variantId);
        return Allowed.Contains(baseId);
    }
}
