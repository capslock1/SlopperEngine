using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Mathematics;

namespace SlopperEngine.Physics.Colliders;

/// <summary>
/// A collider in the shape of a box.
/// </summary>
/// <param name="mass">How heavy the box is.</param>
/// <param name="dimensions">How large the box is.</param>
public class BoxCollider(float mass, Vector3 dimensions) : Collider(mass)
{
    /// <summary>
    /// Creates a BoxCollider of mass and dimensions 1. 
    /// </summary>
    public BoxCollider() : this(1, Vector3.One) { }

    /// <summary>
    /// The size of the box.
    /// </summary>
    public Vector3 Dimensions
    {
        get => _dimensions;
        set
        {
            _dimensions = value;
            UpdateParentColliders();
        }
    }

    Vector3 _dimensions = dimensions;

    public override void AddColliderTo(RigidPose pose, ref CompoundBuilder builder)
    {
        var shape = new Box(Dimensions.X, Dimensions.Y, Dimensions.Z);
        builder.Add(shape, pose, Mass);
    }
}