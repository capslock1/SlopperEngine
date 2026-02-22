using System;

namespace SlopperEngine.Core.Mods;

/// <summary>
/// Rules for what a SlopperMod's code is allowed to do.
/// </summary>
[RequiresPermission(ManageMods)]
[Flags] 
public enum ModPermissionFlags : byte
{
    /// <summary>
    /// The mod can't do anything. No code will be loaded.
    /// </summary>
    None = 0,

    /// <summary>
    /// The mod is allowed to read and write its own files, modify existing scenes, and add its own code.
    /// </summary>
    Default = 1,

    /// <summary>
    /// The mod is allowed to create, activate, and deactivate scenes.
    /// </summary>
    ManageScenes = 1 << 1,

    /// <summary>
    /// The mod is allowed to read and write files of other SlopperMods (if they have less or equal permissions).
    /// </summary>
    ManageOtherModFiles = 1 << 2,

    /// <summary>
    /// The mod is allowed to access the internet.
    /// </summary>
    AccessNetwork = 1 << 3,

    /// <summary>
    /// The mod is allowed to access code from mods (with less or equal permissions).
    /// </summary>
    AccessOtherMods = 1 << 4,

    /// <summary>
    /// The mod is allowed to load and unload mods (with less or equal permissions).
    /// </summary>
    ManageMods = 1 << 5,

    /// <summary>
    /// The mod has complete and unrestricted access to any code and the engine.
    /// </summary>
    Unrestricted = 1 << 6,

    /// <summary>
    /// All flags set to true.
    /// </summary>
    All = 0xFF,
}