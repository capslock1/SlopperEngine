using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Graphics.Lighting;
using OpenTK.Mathematics;

namespace SlopperEngine.SceneObjects.Rendering;

/// <summary>
/// A light which radiates a spherical area around it, and casts no shadow.
/// </summary>
public class PointLight : SceneObject3D
{
    SceneDataHandle _dataHandle;

    float _radius = 5;
    /// <summary>
    /// The radius of this point light.
    /// </summary>
    public float Radius
    {
        get => _radius;
        set 
        {
            _radius = value;
            if(InScene && _dataHandle.IsRegistered)
            {
                RemoveData(Scene);
                CreateData();
            }
        }
    }

    Vector3 _color = Vector3.One;
    /// <summary>
    /// The color of this point light.
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

    float _sharpness = 1;
    /// <summary>
    /// How sharply this point light falls off.
    /// </summary>
    public float Sharpness
    {
        get => _sharpness;
        set
        {
            _sharpness = value;
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
        PointLightData res = default;
        res.Radius = _radius;
        res.Color = _color;
        res.Sharpness = _sharpness;
        res.Object = this;
        _dataHandle = Scene!.RegisterSceneData(res);
    }

    [OnUnregister]
    void RemoveData(Scene? scene)
    {
        scene?.UnregisterSceneData<PointLightData>(_dataHandle, new());
        _dataHandle = default;
    }
}