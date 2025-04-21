using SlopperEngine.SceneObjects;

namespace SlopperEngine.Engine.SceneComponents;

/// <summary>
/// Part of the scene, decides how objects within it should behave - analogous to the singleton pattern.
/// </summary>
public abstract class SceneComponent : SceneObject
{
    /// <summary>
    /// Updates the component.
    /// </summary>
    public abstract void FrameUpdate(FrameUpdateArgs input);

    /// <summary>
    /// Updates the component's user input.
    /// </summary>
    public abstract void InputUpdate(InputUpdateArgs input);

    [OnRegister] void Register() => Scene!.CheckCachedComponents();
    [OnUnregister] void Unregister(Scene scene) => scene.CheckCachedComponents();
}