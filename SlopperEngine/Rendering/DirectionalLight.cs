using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Graphics.Lighting;
using OpenTK.Mathematics;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Rendering;

/// <summary>
/// A light which radiates from infinitely far with only a direction, and no shadow.
/// </summary>
public class DirectionalLight : SceneObject3D
{
    [DontSerialize] SceneDataHandle _dataHandle;

    Vector3 _color = Vector3.One;
    /// <summary>
    /// The color of this directional light.
    /// </summary>
    public Vector3 Color
    {
        get => _color;
        set
        {
            _color = value;
            if(InScene && _dataHandle.IsRegistered)
            {
                RemoveData(Scene);
                CreateData();
            }
        }
    }

    [OnRegister]
    void CreateData()
    {
        DirectionalLightData res = default;
        res.Color = _color;
        res.Object = this;
        _dataHandle = Scene!.RegisterSceneData(res);
    }

    [OnUnregister]
    void RemoveData(Scene? scene)
    {
        scene?.UnregisterSceneData<DirectionalLightData>(_dataHandle, new());
        _dataHandle = default;
    }
}
