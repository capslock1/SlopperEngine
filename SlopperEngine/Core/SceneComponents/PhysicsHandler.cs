using BepuPhysics;
using BepuUtilities;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Physics;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.Core.SceneComponents;

/// <summary>
/// Calls every methodHolder that's been added to it, with no respect for order.
/// </summary>
public class PhysicsHandler : SceneComponent
{
    public OpenTK.Mathematics.Vector3 Gravity = (0,-5,0);
    public readonly float PhysicsDeltaTime = .02f;
    public float NormalizedPhysicsFrameTime {get; private set;} = 0; 
    public readonly bool SlowDownAtLowFPS = false;
    private ThreadDispatcher physThreadDispatcher;
    [DontSerialize] public Simulation Simulator;

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
            new SolveDescription(1,8)
            );
    }

    [OnSerialize] void OnSerialize(SerializedObjectTree.CustomSerializer serializer)
    {
        if(serializer.IsWriter)
            Init();
    }

    [OnRegister]
    void Register()
    {
        if(Scene!.Components.AllOfType<PhysicsHandler>().Count > 1)
        {
            Destroy();
            System.Console.WriteLine("There can only be one PhysicsHandler in the scene.");
            return;
        }
        physThreadDispatcher = new ThreadDispatcher(Environment.ProcessorCount-2);
        Scene.CheckCachedComponents();
        _lateStart = true;
    }
    
    /// <summary>
    /// Updates all objects in the scene with this particular EngineMethodHolder.
    /// </summary>
    /// <param name="input">The input parameter to the EngineMethod.</param>
    public override void FrameUpdate(FrameUpdateArgs input)
    {
        
        if(_lateStart)
        {
            Scene!.CheckCachedComponents();
            foreach(var rb in Scene.GetDataContainerEnumerable<Rigidbody.RigidBodyData>())
                rb.Rigidbody.UpdateColliders();
            _lateStart = false;
        }

        _timeSinceLastUpdate += float.Min(input.DeltaTime, SlowDownAtLowFPS ? PhysicsDeltaTime : float.PositiveInfinity);
        if(_timeSinceLastUpdate < PhysicsDeltaTime)
        {
            NormalizedPhysicsFrameTime = _timeSinceLastUpdate/PhysicsDeltaTime;
            return;  
        } 
        NormalizedPhysicsFrameTime = 1;

        if(SlowDownAtLowFPS)
        {
            _timeSinceLastUpdate -= PhysicsDeltaTime;
            PhysicsUpdate(new(PhysicsDeltaTime));
            return;
        }

        while(_timeSinceLastUpdate > PhysicsDeltaTime)
        {
            _timeSinceLastUpdate -= PhysicsDeltaTime;
            PhysicsUpdate(new(PhysicsDeltaTime));
        }
    }

    void PhysicsUpdate(PhysicsUpdateArgs args)
    {
        Simulator.Timestep(PhysicsDeltaTime, physThreadDispatcher);
        foreach(var u in Scene!.GetDataContainerEnumerable<OnPhysicsUpdate>())
        {
            u.Invoke(args);
            Scene.UpdateRegister();
        }
    }

    public override void InputUpdate(InputUpdateArgs input){}
}
