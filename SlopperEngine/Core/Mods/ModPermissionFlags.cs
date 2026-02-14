using System;

namespace SlopperEngine.Core.Mods;

/// <summary>
/// Rules for what a SlopperMod's code is allowed to do.
/// </summary>
[Flags]
public enum ModPermissionFlags
{
    /// <summary>
    /// The mod can't do anything. No code will be loaded.
    /// </summary>
    None = 0,

    /// <summary>
    /// The mod is allowed to read and write its own files, modify existing scenes, and add its own code.
    /// </summary>
    Default = 0b1,

    /// <summary>
    /// The mod is allowed to create, activate, and deactivate scenes.
    /// </summary>
    ManageScenes = 0b10,

    /// <summary>
    /// The mod is allowed to read and write files of other SlopperMods (if they have less permissions).
    /// </summary>
    ManageOtherModFiles = 0b100,

    /// <summary>
    /// The mod is allowed to access the internet.
    /// </summary>
    AccessNetwork = 0b1000,

    /// <summary>
    /// The mod is allowed to load and unload mods (with less or equal permissions).
    /// </summary>
    ManageMods = 0b10000,

    /// <summary>
    /// The mod has complete and unrestricted access to any code and the engine.
    /// </summary>
    Unrestricted = 0b100000,
}