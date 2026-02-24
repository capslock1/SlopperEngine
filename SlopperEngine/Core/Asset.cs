using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using SlopperEngine.Core.Mods;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.Core;

/// <summary>
/// Helper class to find assets. Assumes asset folders do not change during the program's runtime.
/// </summary>
public readonly struct Asset : ISerializableFromKey<(string?, string?, AssetLoadSettings)>
{
    /// <summary>
    /// The filepath this asset came from. Useful for caching files.
    /// </summary>
    public readonly string? FullFilePath;

    /// <summary>
    /// The filepath this asset came from, relative to the SlopMod of origin.
    /// </summary>
    public readonly string? RelativeFilePath;

    /// <summary>
    /// The SlopMod this asset came from. 
    /// </summary>
    public readonly SlopModInfo? ModOfOrigin;

    readonly AssetLoadSettings _access;

    Asset(AssetLoadSettings access, string? fullFilePath, string? relativeFilePath, SlopModInfo? modInfo)
    {
        _access = access;
        FullFilePath = fullFilePath;
        RelativeFilePath = relativeFilePath;
        ModOfOrigin = modInfo;
    }

    /// <summary>
    /// Whether or not this Asset has non-null fields.
    /// </summary>
    public bool AssetExists => FullFilePath != null && 
    (
        _access.Mode == FileMode.CreateNew || 
        _access.Mode == FileMode.Create || 
        _access.Mode == FileMode.OpenOrCreate || 
        _access.Mode == FileMode.Append || File.Exists(FullFilePath));

    /// <summary>
    /// Gets the stream associated with the asset.
    /// </summary>
    public FileStream GetStream()
    {
        // if this errors its your own fault for not checking AssetExists
        return File.Open(FullFilePath!, _access.Mode, _access.Access, _access.Share);
    }

    /// <summary>
    /// Reads all lines of text in the file.
    /// </summary>
    /// <param name="encoding">The encoding to read the file with. UTF8 on default.</param>
    /// <returns>A string array containing the lines of the text.</returns>
    public string[] ReadAllLines(System.Text.Encoding? encoding = null)
    {
        if(encoding == null)
            return File.ReadAllLines(FullFilePath!);
        return File.ReadAllLines(FullFilePath!, encoding);
    }

    /// <summary>
    /// Reads all bytes in the file.
    /// </summary>
    public byte[] ReadAllBytes() => File.ReadAllBytes(FullFilePath!);

    /// <summary>
    /// Reads all text in the file into a string.
    /// </summary>
    public string ReadAllText(System.Text.Encoding? encoding = null)
    {
        if(encoding == null)
            return File.ReadAllText(FullFilePath!);
        return File.ReadAllText(FullFilePath!, encoding);
    }
    public override int GetHashCode()
    {
        return FullFilePath?.GetHashCode() ?? 0;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if(obj is Asset asset)
            return asset.FullFilePath == FullFilePath;
        return false;
    }

    /// <summary>
    /// Gets an asset at a certain pat. Returns default if the file did not load successfully.
    /// </summary>
    /// <param name="path">The path to get the file from. This is relative to the SlopperMod's asset folder.</param>
    /// <param name="mode">The mode to open the file with. Opens on default.</param>
    /// <param name="access">The mode to access the file with.</param>
    /// <param name="share">The mode to share the file with.</param>
    public static Asset GetFile(string path, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
    {
        TryGetFile(path, out var res, mode, access, share);
        return res.GetValueOrDefault();
    }

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
    public static bool TryGetFile(string path, [NotNullWhen(true)] out Asset? file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
    {
        file = null;
        try
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            if(SlopModInfo.TryGetInfo(callingAssembly, out var info))
                return TryGetFileFromMod(path, info, out file, mode, access, share);
        }
        catch(Exception e)
        {
            System.Console.WriteLine($"Assets: error while trying to load '{path}' ({mode},{access},{share}) - {e.Message}");
        }
        return false;
    }

    /// <summary>
    /// Tries to get a file at a certain path, relative to a given SlopperMod.
    /// </summary>
    /// <param name="path">The path to get the file from.</param>
    /// <param name="mod">The mod to load the file from.</param>
    /// <param name="file">The retrieved file. Null if this function returns false - this means the file could not be found.</param>
    /// <param name="mode">The mode to open the file with. Opens on default.</param>
    /// <param name="access">The mode to access the file with.</param>
    /// <param name="share">The mode to share the file with.</param>
    /// <returns>Whether or not the file could be found.</returns>
    [RequiresPermission(ModPermissionFlags.ManageOtherModFiles)]
    public static bool TryGetFileFromMod(string path, SlopModInfo mod, [NotNullWhen(true)] out Asset? file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
    {
        file = null;
        try
        {
            string fullPath = Path.GetFullPath(mod.AssetFolderPath, path);
            if(!fullPath.StartsWith(mod.AssetFolderPath))
            {
                System.Console.WriteLine($"Assets: didn't load '{path}' from {mod.ShortName} because it's an invalid filepath. Nice try buddy (:");
                return false;
            }

            AssetLoadSettings assetAccess = new(mode, access, share);
            file = new(assetAccess, fullPath, path, mod);
            return (File.Exists(fullPath) && mode != FileMode.CreateNew) || mode == FileMode.Create || mode == FileMode.OpenOrCreate || mode == FileMode.Append;
        }
        catch(Exception e)
        {
            System.Console.WriteLine($"Assets: error while trying to load '{path}' ({mode},{access},{share}) - {e.Message}");
        }
        return false;
    }

    /// <summary>
    /// Gets an engine asset at a certain path (read only). Returns default if the file did not load successfully.
    /// </summary>
    /// <param name="path">The path to get the file from. This is relative to the "EngineAssets" folder.</param>
    public static Asset GetEngineAsset(string path)
    {
        TryGetEngineAsset(path, out var res);
        return res.GetValueOrDefault();
    }

    /// <summary>
    /// Tries to get an engine asset at a certain path (read only).
    /// </summary>
    /// <param name="path">The path to get the file from. This is relative to the "EngineAssets" folder.</param>
    /// <param name="file">The retrieved file. Null if this function returns false - this means the file could not be found.</param>
    /// <returns>Whether or not the file could be found.</returns>
    public static bool TryGetEngineAsset(string path, [NotNullWhen(true)] out Asset? file)
    {
        return TryGetFile(path, out file);
    }
    
    (string?, string?, AssetLoadSettings) ISerializableFromKey<(string?, string?, AssetLoadSettings)>.Serialize()
    {
        return (ModOfOrigin?.ShortName, RelativeFilePath, _access);
    }

    static object? ISerializableFromKey<(string?, string?, AssetLoadSettings)>.Deserialize((string?, string?, AssetLoadSettings) key)
    {
        if(key.Item1 == null || key.Item2 == null) 
            return null;
        if(!SlopModInfo.TryGetModByName(key.Item1, out var mod))
            throw new Exception($"Asset: mod {key.Item1} has not been loaded");

        TryGetFileFromMod(key.Item2, mod, out var res, key.Item3.Mode, key.Item3.Access, key.Item3.Share);
        return res;
    }

    public static bool operator ==(Asset left, Asset right)
    {
        return left.FullFilePath == right.FullFilePath;
    }

    public static bool operator !=(Asset left, Asset right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"Asset '{RelativeFilePath}' from {ModOfOrigin?.ShortName}";
    }
}

[RequiresPermission(ModPermissionFlags.Unrestricted)] // requires high access to circumvent someone misusing ISerializableFromKey without permission
internal record struct AssetLoadSettings(FileMode Mode, FileAccess Access, FileShare Share){}