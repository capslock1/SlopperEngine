using SlopperEngine.SceneObjects;

namespace TestProgram;

/// <summary>
/// Interface for a TestProgram demonstration scene.
/// </summary>
public interface IDemo
{
    /// <summary>
    /// Creates a demo scenes and returns it. When this scene is destroyed, the program assumes the demo is over. The demo is expected to make its own window.
    /// </summary>
    public abstract static Scene CreateDemoScene();

    /// <summary>
    /// Gets the name of this demo. If null or not overridden, the name of the type is used.
    /// </summary>
    public virtual static string? GetName() => null;

    /// <summary>
    /// Gets the description of this demo. If null or not overridden, no description is given.
    /// </summary>
    public virtual static string? GetDescription() => null; 
}