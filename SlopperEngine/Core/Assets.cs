
namespace SlopperEngine.Core;

/// <summary>
/// Helper class to find assets.
/// </summary>
public static class Assets
{
    /// <summary>
    /// Contains
    /// </summary>
    static readonly Dictionary<string, string> _assetFolders = new();

    /// <summary>
    /// Gets the filepath to an asset relative to a specific assets folder.
    /// </summary>
    /// <param name="relativePath">The relative path. Should look like "myTextures/Image.png".</param>
    /// <param name="assetFolderName">The name of the assets folder. "Assets" on default.</param>
    /// <returns>A path to the asset.</returns>
    /// <exception cref="Exception"></exception>
    public static string GetPath(string relativePath, string assetFolderName = "Assets")
    {
        if (_assetFolders.TryGetValue(assetFolderName, out string? path))
            return Path.Combine(path, relativePath);

        path = Directory.GetCurrentDirectory();
        string[]? assetPath;
        while (true)
        {
            if (path == null)
                throw new Exception("Could not find the Assets folder.");

            assetPath = Directory.GetDirectories(path, assetFolderName);
            if (assetPath.Length != 0)
                break;

            path = Directory.GetParent(path)?.FullName;
        }
        _assetFolders[assetFolderName] = assetPath[0];
        return Path.Combine(assetPath[0], relativePath);
    }
}
