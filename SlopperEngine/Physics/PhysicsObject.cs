
using OpenTK.Mathematics;
using SlopperEngine.Engine.SceneComponents;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Physics;

/// <summary>
/// A SceneObject that can be transformed and rotated, but not scaled.
/// </summary>
public class PhysicsObject : SceneObject
{
    /// <summary>
    /// The position of this PhysicsObject. Usually global position. Gets interpolated between physics frames.
    /// </summary>
    public Vector3 Position
    {
        get => Scene?.PhysicsHandler != null ? Vector3.Lerp(_lastPhysFramePosition, _currentPosition, Scene!.PhysicsHandler!.NormalizedPhysicsFrameTime) : _currentPosition;
        set 
        {
            _currentPosition = value;
            _lastPhysFramePosition = value;
            OnPositionChange(value);
        }
    }
    Vector3 _currentPosition = Vector3.Zero;
    Vector3 _lastPhysFramePosition = Vector3.Zero;

    /// <summary>
    /// The rotation of this PhysicsObject. Usually global rotation. Gets interpolated between physics frames.
    /// </summary>
    public Quaternion Rotation
    {
        get => Scene?.PhysicsHandler != null ? Quaternion.Slerp(_lastPhysFrameRotation, _currentRotation, Scene!.PhysicsHandler!.NormalizedPhysicsFrameTime) : _currentRotation;
        set
        {
            _currentRotation = value;
            _lastPhysFrameRotation = value;
            OnRotationChange(value);
        }
    }
    Quaternion _currentRotation = Quaternion.Identity;
    Quaternion _lastPhysFrameRotation = Quaternion.Identity;
    
    /// <summary>
    /// Gets the matrix to transform the SceneObject3D's space to the parent's object space.
    /// </summary>
    public Matrix4 LocalMatrix{
        get{
            var mat = Matrix4.CreateFromQuaternion(Rotation);
            mat.Row3 = new Vector4(Position,1);
            return mat;
        }
    }
    
    public override Matrix4 GetGlobalTransform() => LocalMatrix;

    /// <summary>
    /// Gets called when the position is externally set. Should handle physics engine calls in overriding classes.
    /// </summary>
    protected virtual void OnPositionChange(Vector3 newPosition){}
    protected void UpdateCurrentPosition(Vector3 newPosition)
    {
        _lastPhysFramePosition = _currentPosition;
        _currentPosition = newPosition;
    }

    /// <summary>
    /// Gets called when the rotation is externally set. Should handle physics engine calls in overriding classes.
    /// </summary>
    protected virtual void OnRotationChange(Quaternion newRotation){}
    protected void UpdateCurrentRotation(Quaternion newRotation)
    {
        _lastPhysFrameRotation = _currentRotation;
        _currentRotation = newRotation;
    }
}