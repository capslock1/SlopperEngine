using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using OpenTK.Mathematics;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects;
using System;

namespace SlopperEngine.Rendering.Lighting;

/// <summary>
/// A light which radiates from infinitely far with only a direction, and no shadow.
/// </summary>
public class DirectionalLight : SceneObject3D
{
    /// <summary>
    /// The color of this directional light.
    /// </summary>
    public Vector3 Color = Vector3.One;

    /// <summary>
    /// Whether or not this directional light casts shadows.
    /// </summary>
    public bool CastsShadows = false;

    /// <summary>
    /// The cascades this directional light has. Every value in the array represents the size of the cascade in units. Maximum amount of cascades is four.
    /// </summary>
    public float[]? Cascades = null;

    /// <summary>
    /// How far the near and far plane of the light's shadow projection should be.
    /// </summary>
    public float PlaneDistance = 10000;

    /// <summary>
    /// The resolution of the shadow map. Really cannot be bothered making this variable right now.
    /// </summary>
    public const int ShadowResolutionPixels = 1024;

    public static readonly ReadOnlyMemory<float> DefaultCascades = new float[]{32, 128, 128*4}; 

    [DontSerialize] SceneDataHandle _dataHandle;

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
