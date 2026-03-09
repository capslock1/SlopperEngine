namespace SlopperEngine.Core.Mods;

/// <summary>
/// Events for when the SlopMod loads.
/// </summary>
public interface ISlopModEvents
{
    /// <summary>
    /// Gets called after all assemblies in the SlopMod get loaded.
    /// </summary>
    public abstract static void OnModLoad();
}