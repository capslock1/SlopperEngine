
namespace SlopperEngine.Core.Mods;

/// <summary>
/// Interface for assembly loading events.
/// </summary>
public interface IMod
{
    /// <summary>
    /// Gets called once the SlopMod gets loaded.
    /// </summary>
    public static abstract void OnModLoad();
}