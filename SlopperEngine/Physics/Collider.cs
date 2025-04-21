using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Mathematics;
using SlopperEngine.Engine;
using SlopperEngine.Engine.SceneComponents;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Physics;

/// <summary>
/// Abstract base class for collidable objects.
/// </summary>
/// <param name="mass">The initial mass of the collider.</param>
public abstract class Collider(float mass) : PhysicsObject
{
    Rigidbody? _parent;

    /// <summary>
    /// The mass of the collider.
    /// </summary>
    public float Mass
    {
        get => _mass;
        set
        {
            _mass = value;
            UpdateParentColliders();
        }
    }
    private float _mass = mass;

    /// <summary>
    /// Helper function for convenience.
    /// </summary>
    [OnRegister] 
    protected void UpdateParentColliders()
    {
        if(Parent == null)
            return;

        if(Parent is Rigidbody _parent)
            _parent.UpdateColliders();
        else 
        System.Console.WriteLine($"Collider {this} expects to be a child of a RigidBody. It will not be added to the scene.");
    }

    [OnUnregister]
    void Unregister(Scene _)
    {
        _parent?.UpdateColliders();
        _parent = null;
    }

    protected override void OnPositionChange(Vector3 newPosition) => UpdateParentColliders();
    protected override void OnRotationChange(Quaternion newRotation) => UpdateParentColliders();

    /// <summary>
    /// Adds this collider to the compound shape.
    /// </summary>
    /// <param name="pose">The offset of the collider.</param>
    /// <param name="builder">The builder to add to.</param>
    public abstract void AddColliderTo(RigidPose pose, ref CompoundBuilder builder);

    public override Matrix4 GetGlobalTransform()
    {
        if(Parent is PhysicsObject obj)
            return LocalMatrix*obj.GetGlobalTransform();
        return LocalMatrix;
    }
}