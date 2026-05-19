using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Core.SceneData;

namespace SlopperEngine.Rendering;

/// <summary>
/// Renders the scene.
/// </summary>
public class Camera : SceneObject3D
{
    public Matrix4 Projection = Matrix4.Identity;
    
    [DontSerialize] SceneDataHandle _dataHandle;

    public Camera(){}
    public Camera(Matrix4 projection)
    {
        Projection = projection;
    }

    [OnRegister]
    void OnAdd()
    {
        _dataHandle = Scene!.RegisterSceneData<Camera>(this);
    }

    [OnUnregister]
    void OnRemove(Scene? scene)
    {
        scene?.UnregisterSceneData<Camera>(_dataHandle, this);
        _dataHandle = default;
    }
}
