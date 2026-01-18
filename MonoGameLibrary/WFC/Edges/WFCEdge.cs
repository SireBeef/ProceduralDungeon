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

    // Checks compatibility with another edge
    public bool IsCompatible(WFCEdge other)
    {
        // Overlaps returns true if there
        // is at least one match.
        return Allowed.Overlaps(other.Allowed);
    }
}
