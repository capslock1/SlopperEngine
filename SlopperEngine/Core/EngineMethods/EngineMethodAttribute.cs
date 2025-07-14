using System.Reflection;
using SlopperEngine.Core.SceneData;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Core;

/// <summary>
/// Abstract attribute for methods. Will ensure a method is registered to a scene, if there is an appropriate MethodHandler.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public abstract class EngineMethodAttribute : Attribute
{
    public EngineMethodAttribute(){}

    /// <summary>
    /// Creates a usable EngineMethodAttribute that can register objects to the scene.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public abstract EngineMethodAttribute CreateUsableFromMethodInfo(MethodInfo info);

    /// <summary>
    /// Registers this EngineMethod to the scene. Only valid when created using CreateUsableFromMethodInfo().
    /// </summary>
    /// <param name="scene">The scene to register to.</param>
    /// <param name="obj">The SceneObject to register it to.</param>
    public abstract SceneDataHandle RegisterToScene(Scene scene, SceneObject obj);

    /// <summary>
    /// Unregisters this EngineMethod from the scene. Only valid when created using CreateUsableFromMethodInfo().
    /// </summary>
    /// <param name="scene">The scene to unregister from.</param>
    /// <param name="handle">The handle of the registered method.</param>
    /// <param name="obj">The SceneObject to unregister.</param>
    public abstract void UnregisterFromScene(Scene scene, SceneDataHandle handle, SceneObject obj);
}