using System;
using BepuPhysics;
using BepuUtilities;
using SlopperEngine.Core.Collections;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Physics;
using SlopperEngine.SceneObjects;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.Core.SceneComponents;

/// <summary>
/// Calls every methodHolder that's been added to it, with no respect for order.
/// </summary>
public class PhysicsHandler : SceneComponent
{
    public OpenTK.Mathematics.Vector3 Gravity = (0, -5, 0);
    public readonly float PhysicsDeltaTime = .02f;
    public float NormalizedPhysicsFrameTime { get; private set; } = 0;
    public readonly bool SlowDownAtLowFPS = false;
    [DontSerialize] public Simulation Simulator;

    [DontSerialize] private ThreadDispatcher _physThreadDispatcher;

    float _timeSinceLastUpdate;
    bool _lateStart = true;

#pragma warning disable CS8618
    public PhysicsHandler()
#pragma warning restore CS8618
    {
        Init();
    }

    void Init()
    {
        Simulator = Simulation.Create(
            new BepuUtilities.Memory.BufferPool(),
            new SceneNarrowPhaseCallbacks(this),
            new ScenePoseIntegratorCallbacks(this),
            new SolveDescription(1, 8)
            );
    }

    [OnSerialize]
    void OnSerialize(OnSerializeArgs serializer)
    {
        if (serializer.IsWriter)
            Init();
    }

    [OnRegister]
    void Register()
    {
        if (Scene!.Components.AllOfType<PhysicsHandler>().Count > 1)
        {
            Destroy();
            System.Console.WriteLine("There can only be one PhysicsHandler in the scene.");
            return;
        }
        _physThreadDispatcher = new ThreadDispatcher(int.Max(1, Environment.ProcessorCount - 2));
        Scene.CheckCachedComponents();
        _lateStart = true;
    }

    /// <summary>
    /// Updates all objects in the scene with this particular EngineMethodHolder.
    /// </summary>
    /// <param name="input">The input parameter to the EngineMethod.</param>
    public override void FrameUpdate(FrameUpdateArgs input)
    {

        if (_lateStart)
        {
            Scene!.CheckCachedComponents();
            var collUpdater = new UpdateColliders();
            Scene.GetDataContainerEnumerable<Rigidbody.RigidBodyData>().Enumerate(ref collUpdater);
            _lateStart = false;
        }

        _timeSinceLastUpdate += float.Min(input.DeltaTime, SlowDownAtLowFPS ? PhysicsDeltaTime : float.PositiveInfinity);
        if (_timeSinceLastUpdate < PhysicsDeltaTime)
        {
            NormalizedPhysicsFrameTime = _timeSinceLastUpdate / PhysicsDeltaTime;
            return;
        }
        NormalizedPhysicsFrameTime = 1;

        if (SlowDownAtLowFPS)
        {
            _timeSinceLastUpdate -= PhysicsDeltaTime;
            PhysicsUpdate(new(PhysicsDeltaTime));
            return;
        }

        while (_timeSinceLastUpdate > PhysicsDeltaTime)
        {
            _timeSinceLastUpdate -= PhysicsDeltaTime;
            PhysicsUpdate(new(PhysicsDeltaTime));
        }
    }
    struct UpdateColliders : IRefEnumerator<Rigidbody.RigidBodyData>
    {
        public void Next(ref Rigidbody.RigidBodyData value)
        {
            value.Rigidbody.UpdateColliders();
        }
    }

    void PhysicsUpdate(PhysicsUpdateArgs args)
    {
        Simulator.Timestep(PhysicsDeltaTime, _physThreadDispatcher);
        CallPhysicsUpdate updater = new(args, Scene!);
        Scene!.GetDataContainerEnumerable<OnPhysicsUpdate>().Enumerate(ref updater);
    }
    struct CallPhysicsUpdate(PhysicsUpdateArgs args, Scene scene) : IRefEnumerator<OnPhysicsUpdate>
    {
        PhysicsUpdateArgs _args = args;
        Scene _scene = scene;
        public void Next(ref OnPhysicsUpdate value)
        {
            value.Invoke(_args);
            _scene.UpdateRegister();
        }
    }

    public override void InputUpdate(InputUpdateArgs input) { }
}
