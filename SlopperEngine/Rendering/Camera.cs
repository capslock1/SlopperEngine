using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Rendering;

/// <summary>
/// Renders the scene.
/// </summary>
public class Camera : SceneObject3D
{
    public Matrix4 Projection = Matrix4.Identity;

    public Camera(){}
    public Camera(Matrix4 projection)
    {
        Projection = projection;
    }

    [OnRegister]
    void OnAdd()
    {
        Scene?.SceneRenderer?.AddCamera(this);
    }

    [OnUnregister]
    void OnRemove(Scene? scene)
    {
        scene?.SceneRenderer?.RemoveCamera(this);
    }
}
