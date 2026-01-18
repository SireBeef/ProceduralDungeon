using System.Text.RegularExpressions;

namespace MonoGameLibrary.WFC.Edges;

public static class SocketRotation
{
    private static readonly Regex RotationPattern = new(@"^(.+)_rot(\d+)$", RegexOptions.Compiled);

    /// <summary>
    /// Rotates a socket string by the specified degrees.
    /// Sockets with a _rot{N} suffix have their rotation advanced.
    /// Sockets without a rotation suffix are returned unchanged.
    /// </summary>
    public static string Rotate(string socket, int degrees)
    {
        var match = RotationPattern.Match(socket);

        if (!match.Success)
        {
            // No rotation suffix (e.g., "empty", "floor") - return unchanged
            return socket;
        }

        var baseName = match.Groups[1].Value;
        var currentRotation = int.Parse(match.Groups[2].Value);
        var newRotation = (currentRotation + degrees) % 360;

        return $"{baseName}_rot{newRotation}";
    }
}
