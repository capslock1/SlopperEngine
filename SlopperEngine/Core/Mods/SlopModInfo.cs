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
    static readonly Dictionary<string, SlopModInfo> _modAtPath = new();
    static readonly Dictionary<Assembly, SlopModInfo> _modOfAssembly = new();

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
    /// The permissions this SlopMod has.
    /// </summary>
    public readonly ModPermissionFlags Permissions;

    readonly List<Assembly> _assembliesInMod = new();

    SlopModInfo(string fullFilePath, ModPermissionFlags permissions, Assembly? overridingAssembly = null)
    {
        FullFilePath = fullFilePath;
        var modSettings = File.ReadAllLines(fullFilePath);
        Permissions = permissions;

        // before i get proper manual serialization down, 
        // slopmods are in the following format:
        // - short name
        // - version number
        // - asset folder name

        ShortName = modSettings[0];

        int fileNameLength = Path.GetFileName(fullFilePath.AsSpan()).Length;
        AssetFolderPath = Path.GetFullPath(fullFilePath.Substring(fullFilePath.Length - fileNameLength, fileNameLength), modSettings[2]);
        
        lock(_modAtPath)
            _modAtPath.Add(fullFilePath, this);

        if(overridingAssembly != null)
        {
            lock(_assembliesInMod)
                _assembliesInMod.Add(overridingAssembly);
            return;
        }

        if(permissions == ModPermissionFlags.None) // i... can't do anything!
            return;
    }

    /// <summary>
    /// Loads a mod at a certain filepath (or just gets the mod that was already loaded).
    /// </summary>
    /// <param name="filePath">The filepath to load the mod at.</param>
    /// <param name="permissions">The permissions of the mod.</param>
    /// <param name="result">The ModInfo instance. Null if the function returns false.</param>
    /// <returns>Whether or not the mod could be loaded.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetorLoadMod(string filePath, ModPermissionFlags permissions, [NotNullWhen(true)] out SlopModInfo? result)
    {
        result = null;
        try
        {
            if(!TryGetInfo(Assembly.GetCallingAssembly(), out var callingMod))
            {
                System.Console.WriteLine($"SlopModInfo: Unknown assembly {Assembly.GetCallingAssembly()} trying to load modinfo at {filePath}");
                return false;
            }
            if(!ModPermissionHelper.HasPermissions(callingMod.Permissions, permissions))
            {
                System.Console.WriteLine($"SlopModInfo: Mod {callingMod.ShortName} did not have adequate permissions to load modinfo at {filePath}");
                return false;
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
            System.Console.WriteLine($"SlopModInfo: Error loading SlopMod at '{filePath}' due to: "+e.Message);
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
        var startDirectory = Directory.GetCurrentDirectory();
        while (true)
        {
            try
            {
                if (startDirectory == null)
                    throw new Exception($"Catastrophic error! Could not find the SlopperEngine.slopmod file. SlopperEngine will not be able to run.");

                if(File.Exists(Path.Combine(startDirectory, "SlopperEngine.slopmod")))
                    break;

                startDirectory = Directory.GetParent(startDirectory)?.FullName;
            }
            catch(Exception e)
            {
                throw new Exception($"Catastrophic error! Could not find the SlopperEngine.slopmod file due to an unexpected error: {e.Message}");
            }
        }

        var engineModInfo = new SlopModInfo(Path.Combine(startDirectory, "SlopperEngine.slopmod"), ModPermissionFlags.All, Assembly.GetExecutingAssembly());
    }
}