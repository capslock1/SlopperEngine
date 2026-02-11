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
    List<Assembly> _assembliesInMod = new();

    SlopModInfo(string fullFilePath)
    {
        FullFilePath = fullFilePath;
        var modSettings = File.ReadAllLines(fullFilePath);
    }

    /// <summary>
    /// Loads a mod at a certain filepath (or just gets the mod that was already loaded).
    /// </summary>
    /// <param name="filePath">The filepath to load the mod at.</param>
    /// <param name="result">The ModInfo instance. Null if the function returns false.</param>
    /// <returns>Whether or not the mod could be loaded.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetInfo(string filePath, [NotNullWhen(true)] SlopModInfo? result)
    {
        result = null;
        // get calling assembly and verify whether it has the permission to load? 
        // for now, just exclude this from allowed function calls
        try
        {
            filePath = Path.GetFullPath(filePath);
            lock(_modAtPath)
                if(_modAtPath.TryGetValue(filePath, out result))
                    return true;

            var info = new SlopModInfo(filePath);

            lock(_modAtPath)
                _modAtPath[filePath] = info;

            lock(_modOfAssembly)
                foreach(var assembly in info._assembliesInMod)
                    _modOfAssembly[assembly] = info;
            
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
    public static bool TryGetInfo(Assembly assembly, [NotNullWhen(true)] SlopModInfo? result)
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