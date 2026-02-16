using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using SlopperEngine.Core.Mods;

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
    /// Tries to get a file at a certain path, relative to each SlopperMod.
    /// </summary>
    /// <param name="path">The path to get the file from. This is relative to the SlopperMod's asset folder.</param>
    /// <param name="file">The retrieved file. Null if this function returns false - this means the file could not be found.</param>
    /// <param name="mode">The mode to open the file with. Opens on default.</param>
    /// <param name="access">The mode to access the file with.</param>
    /// <param name="share">The mode to share the file with.</param>
    /// <returns>Whether or not the file could be found.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetFile(string path, [NotNullWhen(true)] out Stream? file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
    {
        file = null;
        try
        {
            var callingAssembly = Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
            if(SlopModInfo.TryGetInfo(callingAssembly, out var info))
            {
                string fullPath = Path.GetFullPath(info.AssetFolderPath, path);
                if(!fullPath.StartsWith(info.AssetFolderPath))
                {
                    System.Console.WriteLine($"Assets: didn't load '{path}' from {info.ShortName} because it's an invalid filepath. Nice try buddy (:");
                    return false;
                }
                file = File.Open(fullPath, mode, access, share);
                return true;
            }
        }
        catch(Exception e)
        {
            System.Console.WriteLine($"Assets: error while trying to load {path} ({mode},{access},{share}) - {e.Message}");
        }
        return false;
    }

    static bool GetModAssetpath(Assembly mod, out string? assetPath)
    {
        assetPath = null;
        return false;
    }

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
