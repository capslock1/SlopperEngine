using System;
using System.Collections.Generic;
using System.IO;

namespace SlopperEngine.Core;

/// <summary>
/// Helper class to find assets. Assumes asset folders do not change during the program's runtime.
/// </summary>
public class Assets
{
    /// <summary>
    /// The assetloader to use. Can be overridden when necessary (some asset folder is not located in a usual location, for example).
    /// </summary>
    public static Assets Instance
    {
        get
        {
            lock (_instance)
            {
                return _instance;
            }
        }
        set
        {
            lock (_instance)
            {
                _instance = value;
            }
        }
    }
    volatile static Assets _instance = new();

    readonly Dictionary<(string, string?), string> _foundFolderPaths = new();

    /// <summary>
    /// Gets the filepath to an asset relative to a specific assets folder.
    /// </summary>
    /// <param name="relativePath">The relative path. Should look like "myTextures/Image.png".</param>
    /// <param name="assetFolderName">The name of the assets folder. "Assets" on default.</param>
    /// <returns>A path to the asset.</returns>
    /// <exception cref="Exception"></exception>
    public static string GetPath(string relativePath, string assetFolderName = "Assets")
    {
        lock (_instance)
            return _instance.GetPathInternal(relativePath, assetFolderName);
    }

    /// <summary>
    /// Refer to static string GetPath(,).
    /// </summary>
    protected virtual string GetPathInternal(string relativePath, string assetFolderName)
    {
        var path = FindPathToFolder(assetFolderName);
        return Path.Combine(path, relativePath);
    }

    /// <summary>
    /// Searches upwards for a folder with a specific name.
    /// </summary>
    /// <param name="folderName">The folder to find.</param>
    /// <param name="startDirectory">The directory to start the search at. If null, the program's directory will be used.</param>
    /// <returns>The full path to the folder.</returns>
    protected string FindPathToFolder(string folderName = "Assets", string? startDirectory = null)
    {
        if (_foundFolderPaths.TryGetValue((folderName, startDirectory), out var res))
            return res;

        startDirectory ??= Directory.GetCurrentDirectory();
        string[]? assetPath;
        while (true)
        {
            if (startDirectory == null)
                throw new Exception($"Could not find the {folderName} folder.");

            assetPath = Directory.GetDirectories(startDirectory, folderName);
            if (assetPath.Length != 0)
                break;

            startDirectory = Directory.GetParent(startDirectory)?.FullName;
        }
        _foundFolderPaths[(folderName, startDirectory)] = assetPath[0];
        return assetPath[0];
    }
}
