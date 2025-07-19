using BepuPhysics;
using BepuPhysics.Collidables;
using SlopperEngine.Physics;

namespace SlopperEngine.Physics.Colliders;

/// <summary>
/// A simple spherical collider.
/// </summary>
/// <param name="mass">The mass of the sphere.</param>
/// <param name="radius">The size of the sphere.</param>
public class SphereCollider(float mass, float radius) : Collider(mass)
{
    /// <summary>
    /// Creates a SphereCollider with mass and radius 1.
    /// </summary>
    public SphereCollider() : this(1, 1) { }

    /// <summary>
    /// The radius of this collider.
    /// </summary>
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            UpdateParentColliders();
        }
    }

    float _radius = radius;

    public override void AddColliderTo(RigidPose pose, ref CompoundBuilder builder)
    {
        var shape = new Sphere(Radius);
        builder.Add(shape, pose, Mass);
    }
}