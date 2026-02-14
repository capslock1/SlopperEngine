using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SlopperEngine.Core.Mods;

/// <summary>
/// Contains important information about a slopmod.
/// </summary>
public sealed class SlopModInfo
{
    static Dictionary<string, SlopModInfo> _modAtPath = new();
    static Dictionary<Assembly, SlopModInfo> _modOfAssembly = new();

    /// <summary>
    /// The full file path to this SlopMod.
    /// </summary>
    public readonly string FullFilePath;

    /// <summary>
    /// The full file path to this SlopMod's asset folder.
    /// </summary>
    public readonly string AssetFolderPath;

    /// <summary>
    /// The name of this SlopMod
    /// </summary>
    public readonly string ShortName;

    /// <summary>
    /// The permission this SlopMod has.
    /// </summary>
    public readonly ModPermissionFlags Permissions;

    readonly List<Assembly> _assembliesInMod = new();

    SlopModInfo(string fullFilePath, ModPermissionFlags permissions, bool overrideLoadAssembly = false)
    {
        FullFilePath = fullFilePath;
        int fileNameLength = Path.GetFileName(fullFilePath.AsSpan()).Length;
        AssetFolderPath = fullFilePath.Substring(fullFilePath.Length - fileNameLength, fileNameLength);
        var modSettings = File.ReadAllLines(fullFilePath);

        // before i get proper manual serialization down, 
        // slopmods are in the following format:
        // - short name
        // - version number
        // - asset folder

        ShortName = modSettings[0];
    }

    /// <summary>
    /// Loads a mod at a certain filepath (or just gets the mod that was already loaded).
    /// </summary>
    /// <param name="filePath">The filepath to load the mod at.</param>
    /// <param name="permissions">The permissions of the mod.</param>
    /// <param name="result">The ModInfo instance. Null if the function returns false.</param>
    /// <returns>Whether or not the mod could be loaded.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetInfo(string filePath, ModPermissionFlags permissions, [NotNullWhen(true)] out SlopModInfo? result)
    {
        result = null;
        try
        {
            if(TryGetInfo(Assembly.GetCallingAssembly(), out var callingMod))
            {
                // if the assembly is not from a sloppermod i guess we don't check for permissions?
                // this feels wrong though

                if(!ModPermissionHelper.HasPermissions(callingMod.Permissions, permissions))
                {
                    System.Console.WriteLine($"Mod {callingMod.ShortName} did not have adequate permissions to get modinfo at {filePath}");
                    return false;
                }
            }

            filePath = Path.GetFullPath(filePath);
            lock(_modAtPath)
                if(_modAtPath.TryGetValue(filePath, out result))
                    return true;

            var info = new SlopModInfo(filePath, permissions);

            lock(_modAtPath)
                _modAtPath[filePath] = info;

            lock(_modOfAssembly)
                foreach(var assembly in info._assembliesInMod)
                    _modOfAssembly[assembly] = info;
            
            result = info;
            return true;
        }
        catch(Exception? e)
        {
            System.Console.WriteLine($"Error loading SlopMod at '{filePath}' due to: "+e.Message);
        }
        return false;
    }

    /// <summary>
    /// Tries to get the SlopModInfo associated with an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get the SlopModInfo of.</param>
    /// <param name="result">The SlopMod from which the assembly originated. Null if this function returns false.</param>
    /// <returns>Whether or not the assembly comes from a SlopMod in the first place.</returns>
    public static bool TryGetInfo(Assembly assembly, [NotNullWhen(true)] out SlopModInfo? result)
    {
        lock(_modOfAssembly)
            return _modOfAssembly.TryGetValue(assembly, out result);
    }

    /// <summary>
    /// Finds all trusted mods and loads them.
    /// </summary>
    public static void InitializeMods()
    {
        
    }
}