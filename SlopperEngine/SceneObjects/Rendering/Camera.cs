using OpenTK.Mathematics;
using SlopperEngine.Core;

namespace SlopperEngine.SceneObjects.Rendering;

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
        Scene?.RenderHandler?.AddCamera(this);
    }

    [OnUnregister]
    void OnRemove(Scene? scene)
    {
        scene?.RenderHandler?.RemoveCamera(this);
    }
}