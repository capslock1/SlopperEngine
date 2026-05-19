using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using OpenTK.Mathematics;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Rendering.Lighting;

/// <summary>
/// A light which radiates a spherical area around it, and casts no shadow.
/// </summary>
public class PointLight : SceneObject3D
{
    [DontSerialize] SceneDataHandle _dataHandle;

    /// <summary>
    /// The radius of this point light.
    /// </summary>
    public float Radius = 5;

    /// <summary>
    /// The color of this point light.
    /// </summary>
    public Vector3 Color = Vector3.One;

    /// <summary>
    /// How sharply this point light falls off.
    /// </summary>
    public float Sharpness = 1;

    [OnRegister]
    void CreateData()
    {
        _dataHandle = Scene!.RegisterSceneData(this);
    }

    [OnUnregister]
    void RemoveData(Scene? scene)
    {
        scene?.UnregisterSceneData<PointLight>(_dataHandle, this);
        _dataHandle = default;
    }
}
