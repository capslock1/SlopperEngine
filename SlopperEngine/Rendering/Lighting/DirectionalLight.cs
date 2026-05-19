using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using OpenTK.Mathematics;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Rendering.Lighting;

/// <summary>
/// A light which radiates from infinitely far with only a direction, and no shadow.
/// </summary>
public class DirectionalLight : SceneObject3D
{
    [DontSerialize] SceneDataHandle _dataHandle;

    /// <summary>
    /// The color of this directional light.
    /// </summary>
    public Vector3 Color = Vector3.One;

    [OnRegister]
    void CreateData()
    {
        _dataHandle = Scene!.RegisterSceneData(this);
    }

    [OnUnregister]
    void RemoveData(Scene? scene)
    {
        scene?.UnregisterSceneData<DirectionalLight>(_dataHandle, this);
        _dataHandle = default;
    }
}
