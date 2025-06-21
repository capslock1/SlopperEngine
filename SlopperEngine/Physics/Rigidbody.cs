
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.Physics;

/// <summary>
/// An object with physics simulation. Requires a collider object as child.
/// </summary>
public class Rigidbody : PhysicsObject
{
    /// <summary>
    /// The colliders the RigidBody is composed of.
    /// </summary>
    public ChildList<Collider> Colliders;

    /// <summary>
    /// Whether or not forces affect this RigidBody.
    /// </summary>
    public bool IsKinematic
    {
        get => _kinematic;
        set
        {
            _kinematic = value;
            UpdateColliders();
        }
    }

    /// <summary>
    /// Whether or not to recenter all the colliders, so the center of mass is properly placed.
    /// </summary>
    public bool RecenterColliders
    {
        get => _recenterColliders;
        set
        {
            if(_recenterColliders == value) return;
            _recenterColliders = value;
            if(value)
                UpdateColliders();
        }
    }
    bool _recenterColliders = true;

    /// <summary>
    /// The linear velocity of the RigidBody in units per second.
    /// </summary>
    public OpenTK.Mathematics.Vector3 Velocity
    {
        get
        {
            if (_current == null)
                return _lastKnownVelocity;
            return _current.Simulator.Bodies[_bodyHandle].Velocity.Linear.ToOTK();
        }
        set
        {
            if(_current == null)
                _lastKnownVelocity = value;
            else _current.Simulator.Bodies[_bodyHandle].Velocity.Linear = value.ToSN();
        }
    }
    protected OpenTK.Mathematics.Vector3 _lastKnownVelocity = OpenTK.Mathematics.Vector3.Zero;

    /// <summary>
    /// The angular velocity of the RigidBody, seemingly in exponential map axis-angle representation.
    /// </summary>
    public OpenTK.Mathematics.Vector3 AngularVelocity
    {
        get
        {
            if (_current == null)
                return _lastKnownAngularVelocity;
            return _current.Simulator.Bodies[_bodyHandle].Velocity.Angular.ToOTK();
        }
        set
        {
            if(_current == null)
                _lastKnownAngularVelocity = value;
            else _current.Simulator.Bodies[_bodyHandle].Velocity.Angular = value.ToSN();
        }
    }
    protected OpenTK.Mathematics.Vector3 _lastKnownAngularVelocity = OpenTK.Mathematics.Vector3.Zero;

    [DontSerialize] BodyHandle _bodyHandle;
    [DontSerialize] SceneDataHandle _sceneHandle;
    [DontSerialize] PhysicsHandler? _current;
    bool _kinematic = false;
    List<(Vector3 impulse, Vector3 offset)> _queuedImpulses = new(1);
    List<Vector3> _queuedAngularImpulses = new(1);

    public Rigidbody()
    {
        Colliders = new(this);
    }

    protected override void OnPositionChange(OpenTK.Mathematics.Vector3 newPosition)
    {
        if(_current == null) return;
        var bod = _current.Simulator.Bodies[_bodyHandle];
        bod.Pose.Position = newPosition.ToSN();
    }
    protected override void OnRotationChange(OpenTK.Mathematics.Quaternion newRotation)
    {
        if(_current == null) return;
        var bod = _current.Simulator.Bodies[_bodyHandle];
        bod.Pose.Orientation = newRotation.ToSN();
    }

    /// <summary>
    /// Adds a impulse to the RigidBody.
    /// </summary>
    /// <param name="impulse">The impulse to apply to the RigidBody.</param>
    /// <param name="offset">The offset to apply the force at.</param>
    public void AddImpulse(OpenTK.Mathematics.Vector3 impulse, OpenTK.Mathematics.Vector3 offset = default)
    {
        if(_current == null)
        {
            _queuedImpulses.Add((impulse.ToSN(), offset.ToSN()));
            return;
        }
        var bod = _current.Simulator.Bodies[_bodyHandle];
        bod.ApplyImpulse(impulse.ToSN(), offset.ToSN());
    }

    /// <summary>
    /// Adds an angular impulse to the RigidBody.
    /// </summary>
    /// <param name="impulse">I really don't know what this does to be honest.</param>
    public void AddAngularImpulse(OpenTK.Mathematics.Vector3 impulse)
    {
        if(_current == null)
        {
            _queuedAngularImpulses.Add(impulse.ToSN());
            return;
        }
        var bod = _current.Simulator.Bodies[_bodyHandle];
        bod.ApplyAngularImpulse(impulse.ToSN());
    }

    /// <summary>
    /// Updates the RigidBody's shape.
    /// </summary>
    public void UpdateColliders()
    {
        var physHandler = Scene?.PhysicsHandler;
        if(physHandler == null) return;
        AddBody(physHandler!);
    }

    [OnRegister]
    void Register()
    {
        _sceneHandle = Scene!.RegisterSceneData(new RigidBodyData(this));
        UpdateColliders();
    }

    [OnUnregister]
    void Unregister(Scene scene)
    {
        if (_current != null)
        {
            _lastKnownAngularVelocity = AngularVelocity;
            _lastKnownVelocity = Velocity;
            _current.Simulator.Bodies.Remove(_bodyHandle);
        }
        scene?.UnregisterSceneData<RigidBodyData>(_sceneHandle, default);
        _current = null;
    }

    [OnPhysicsUpdate]
    void PhysicsUpdate(PhysicsUpdateArgs args)
    {
        if(_current == null) return;
        var pose = _current.Simulator.Bodies[_bodyHandle].Pose;
        UpdateCurrentPosition(pose.Position.ToOTK());
        UpdateCurrentRotation(pose.Orientation.ToOTK());
    }

    //naturally, stolen right from https://github.com/bepu/bepuphysics2/blob/master/Demos/Demos/CompoundDemo.cs
    void AddBody(PhysicsHandler handler)
    {
        if (_currentlyAddingBody) return;
        //remove existing body
        _current?.Simulator.Bodies.Remove(_bodyHandle);
        _current = handler;

        //add new body
        CompoundBuilder builder = new(_current.Simulator.BufferPool, _current.Simulator.Shapes, Colliders.Count);
        foreach(var col in Colliders.All)
        {
            var pose = new RigidPose(col.Position.ToSN(), col.Rotation.ToSN());
            col.AddColliderTo(pose, ref builder);
        }

        BepuUtilities.Memory.Buffer<CompoundChild> compoundChildren;
        BodyInertia compoundInertia;
        Vector3 compoundCenter = Vector3.Zero;
        if(_recenterColliders)
        {
            _currentlyAddingBody = true;
            builder.BuildDynamicCompound(out compoundChildren, out compoundInertia, out compoundCenter);
            foreach(var col in Colliders.All)
                col.Position -= compoundCenter.ToOTK();
            _currentlyAddingBody = false;
        }
        else builder.BuildDynamicCompound(out compoundChildren, out compoundInertia);

        var body = BodyDescription.CreateDynamic(
            new(Position.ToSN() + compoundCenter, Rotation.ToSN()), 
            _kinematic ? default : compoundInertia, 
            _current.Simulator.Shapes.Add(new Compound(compoundChildren)), 
            .01f);
        body.Velocity = new(_lastKnownVelocity.ToSN(), _lastKnownAngularVelocity.ToSN());
        _bodyHandle = _current.Simulator.Bodies.Add(body);

        if(_queuedImpulses.Count > 0)
        {
            var bod = _current.Simulator.Bodies[_bodyHandle];
            foreach(var force in _queuedImpulses)
                bod.ApplyImpulse(force.impulse, force.offset);
            foreach(var anglular in _queuedAngularImpulses)
                bod.ApplyAngularImpulse(anglular);
            _queuedImpulses.Clear();
            _queuedAngularImpulses.Clear();
        }
        
        builder.Dispose();
    }
    bool _currentlyAddingBody = false;

    public struct RigidBodyData(Rigidbody rigidbody) : IEquatable<RigidBodyData>
    {
        public Rigidbody Rigidbody = rigidbody;

        public bool Equals(RigidBodyData other) => Rigidbody == other.Rigidbody;
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is RigidBodyData rb && rb.Equals(this);
        public override int GetHashCode() => Rigidbody.GetHashCode();
    }
}
