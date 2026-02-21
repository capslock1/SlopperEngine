using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace SlopperEngine.Core.Mods;

/// <summary>
/// Contains important information about a slopmod.
/// </summary>
public sealed class SlopModInfo
{
    static readonly Dictionary<string, SlopModInfo> _modAtPath = new();
    static readonly Dictionary<Assembly, SlopModInfo> _modOfAssembly = new();
    static SlopModInfo? _engineSlopModInfo;

    /// <summary>
    /// The SlopModInfo belonging to the engine.
    /// </summary>
    public static SlopModInfo EngineInfo
    {
        get
        {
            InitializeMods();
            return _engineSlopModInfo!;
        }
    }

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

    /// <summary>
    /// Whether or not this SlopModInfo belongs to SlopperEngine.
    /// </summary>
    public bool IsSlopperEngine => this == _engineSlopModInfo;

    readonly List<Assembly> _assembliesInMod = new();

    SlopModInfo(string fullFilePath, ModPermissionFlags permissions, Assembly? slopperEngineAssembly = null)
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
        var pathToModFolder = fullFilePath.Substring(fullFilePath.Length - fileNameLength, fileNameLength);
        AssetFolderPath = Path.GetFullPath(pathToModFolder, modSettings[2]);
        if(!AssetFolderPath.StartsWith(pathToModFolder))
            throw new Exception($"Slopmod's asset folder ({modSettings[2]}) reaches outside of the mod folder.");
        
        // this mod is slopperengine - so it has its assembly hardcoded and does not need to load any code.
        if(slopperEngineAssembly != null)
        {
            _assembliesInMod.Add(slopperEngineAssembly);
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
    /// <returns>Whether or not the mod could be loaded. False likely means the caller lacks permissions.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetOrLoadMod(string filePath, ModPermissionFlags permissions, [NotNullWhen(true)] out SlopModInfo? result)
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
            GetOrLoadMod(filePath, permissions, out result);
            return true;
        }
        catch(Exception e)
        {
            System.Console.WriteLine($"SlopModInfo: Error loading SlopMod at '{filePath}' due to: "+e.Message);
        }
        return false;
    }

    static void GetOrLoadMod(string filePath, ModPermissionFlags permissions, out SlopModInfo result)
    {
        filePath = Path.GetFullPath(filePath);
        lock(_modAtPath)
            if(_modAtPath.TryGetValue(filePath, out result!))
                return;

        var info = new SlopModInfo(filePath, permissions);

        lock(_modAtPath)
            _modAtPath[filePath] = info;

        lock(_modOfAssembly)
            foreach(var assembly in info._assembliesInMod)
                _modOfAssembly[assembly] = info;
        
        result = info;
        return;
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
        if(_engineSlopModInfo == null) // weird if this function gets called twice, but not a problem at least...
            return; 

        var startDirectory = Directory.GetCurrentDirectory();
        while (true)
        {
            if (startDirectory == null)
                throw new Exception($"Could not find the SlopperEngine.slopmod file. SlopperEngine will not be able to run.");

            if(File.Exists(Path.Combine(startDirectory, "SlopperEngine.slopmod")))
                break;

            startDirectory = Directory.GetParent(startDirectory)?.FullName;
        }
        
        // little jank, but i see no reason not to do this like this
        // the engine is basically registered as a SlopMod to make life easy
        GetOrLoadMod(Path.Combine(startDirectory, "SlopperEngine.slopmod"), ModPermissionFlags.All, out _engineSlopModInfo);

        var loadedMods = new List<SlopModInfo>();
        var trustedMods = File.ReadAllLines(Path.Combine(startDirectory, "TrustedModsDONTREPLACE"), Encoding.UTF8);
        if(trustedMods.Length < 2)
            throw new Exception("SlopperEngine won't run, as there were no mods to load.");

        for(int i = 0; i < trustedMods.Length; i += 2)
        {
            try
            {
                var perm = (ModPermissionFlags)long.Parse(trustedMods[i]);
                GetOrLoadMod(trustedMods[i+1], perm, out var mod);
                loadedMods.Add(mod);
            }
            catch(Exception e)
            {
                if(i + 2 >= trustedMods.Length && loadedMods.Count == 0)
                    throw; // rethrow if not a single mod could load successfully. if ANYTHING loaded, we can trust it to do... uh... something. for sure
                
                if(trustedMods.Length < i+1)
                    System.Console.WriteLine($"Failed to load trusted mod ({trustedMods[i+1]}) due to unexpected error: {e.Message}");
            }
        }

        foreach(var mod in loadedMods)
            try
            {
                // call IMod.OnLoad here! for every assembly!
            }
            catch(Exception e)
            {
                System.Console.WriteLine($"'{mod.ShortName}' had an exception while loading: {e.Message}");
            }
    }
}