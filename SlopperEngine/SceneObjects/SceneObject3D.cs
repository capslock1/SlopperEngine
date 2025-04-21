using OpenTK.Mathematics;

namespace SlopperEngine.SceneObjects;

/// <summary>
/// A SceneObject with a 3-dimensional transform. Describes rotation, scale, and position. Relative to its parent.
/// </summary>
public class SceneObject3D : SceneObject
{
    public Vector3 LocalPosition = Vector3.Zero;
    public Quaternion LocalRotation = Quaternion.Identity;
    public Vector3 LocalScale = Vector3.One;
    
    /// <summary>
    /// Gets the matrix to transform the SceneObject3D's space to the parent's object space.
    /// </summary>
    public Matrix4 LocalMatrix{
        get{
            var mat = Matrix4.CreateScale(LocalScale);
            mat *= Matrix4.CreateFromQuaternion(LocalRotation);
            mat.Row3 = new Vector4(LocalPosition,1);
            return mat;
        }
    }
    
    public override Matrix4 GetGlobalTransform()
    {
        if(Parent is null)
            return LocalMatrix;
        return LocalMatrix*Parent.GetGlobalTransform();
    }
}
