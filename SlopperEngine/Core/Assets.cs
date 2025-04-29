
namespace SlopperEngine.Core;

/// <summary>
/// Helper class to find assets.
/// </summary>
public static class Assets
{
    /// <summary>
    /// The path to the Assets folder.
    /// </summary>
    public static readonly string Path;
    
    static Assets()
    {
        string? path = System.IO.Path.GetFullPath("../");
        string[]? AssetPath;
        while(true)
        {
            if(path == null)
                throw new Exception("Could not find the Assets folder.");

            AssetPath = Directory.GetDirectories(path, "Assets");
            if(AssetPath.Length != 0)
                break;

            path = Directory.GetParent(path)?.FullName;
        }
        Path = AssetPath[0];
    }

    /// <summary>
    /// Gets a full path relative to the Assets folder.
    /// </summary>
    /// <param name="relativePath">The path from the Assets folder. Should look like: "Textures/image.png"</param>
    public static string GetPath(string relativePath) => System.IO.Path.Combine(Path, relativePath);
}