using SlopperEngine.SceneObjects;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using SlopperEngine.Core;
using SlopperEngine.Graphics;

namespace SlopperEngine.TestStuff;

/// <summary>
/// A standalone implementation of physics, just to get the hang of it.
/// </summary>
public class StandalonePhysTest : SceneObject
{
    Simulation _sim = Simulation.Create(new BepuUtilities.Memory.BufferPool(), new OtherCallbacks(), new Callbacks(new System.Numerics.Vector3(0,-17,0)), new SolveDescription(1, 8));
    System.Random _rand = new();
    List<MeshRenderer> _renderers = new();
    List<BodyHandle> _handles = new();
    ThreadDispatcher _dispatcher = new ThreadDispatcher(1);
    Material _material = Material.Create(SlopperShader.Create("shaders/phongShader.sesl"));

    public StandalonePhysTest()
    {
        var shape = new Box(10, 1, 10);
        var boxDesc = BodyDescription.CreateKinematic(new RigidPose(new System.Numerics.Vector3(0,-2,0)), _sim.Shapes.Add(shape), new BodyActivityDescription());
        _sim.Bodies.Add(boxDesc);

        var boxRenderer = new MeshRenderer(){Mesh = DefaultMeshes.Cube, LocalScale = (5, .5f, 5), Material = _material, LocalPosition = (0,-2,0)};
        Children.Add(boxRenderer);
    }

    [OnFrameUpdate]
    void Update(FrameUpdateArgs args)
    {
        _sim.Timestep(args.DeltaTime, _dispatcher);
        for(int i = 0; i<_renderers.Count; i++)
        {
            var simPos = _sim.Bodies[_handles[i]].Pose.Position;
            _renderers[i].LocalPosition = (simPos.X, simPos.Y, simPos.Z);
            var simRot = _sim.Bodies[_handles[i]].Pose.Orientation;
            _renderers[i].LocalRotation = new OpenTK.Mathematics.Quaternion(simRot.X, simRot.Y, simRot.Z, simRot.W);
        }
    }

    [OnInputUpdate]
    void InputUpdate(InputUpdateArgs args)
    {
        if(args.KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.N))
            SpawnRandomSphere();
    }

    void SpawnRandomSphere()
    {
        float size = .5f+_rand.NextSingle();
        var shape = new Box(size, size, size);
        var ballDescription = BodyDescription.CreateDynamic(RigidPose.Identity, shape.ComputeInertia(size*size*size), _sim.Shapes.Add(shape), .01f);
        ballDescription.Pose.Position = new System.Numerics.Vector3(2.5f-_rand.NextSingle()*5, 3+_rand.NextSingle()*5, 2.5f-_rand.NextSingle()*5);
        
        var handle = _sim.Bodies.Add(ballDescription);
        _handles.Add(handle);
        _sim.Awakener.AwakenBody(handle);

        size *= .5f;
        var mesh = new MeshRenderer(){Mesh = DefaultMeshes.Cube, LocalScale = (size, size, size), Material = _material};
        _renderers.Add(mesh);
        Children.Add(mesh);
    }
}

struct Callbacks : IPoseIntegratorCallbacks
{
    public System.Numerics.Vector3 Gravity;
    public float LinearDamping;
    public float AngularDamping;


    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

    public readonly bool AllowSubstepsForUnconstrainedBodies => false;

    public readonly bool IntegrateVelocityForKinematics => false;

    public void Initialize(Simulation simulation)
    {
    }
    public Callbacks(System.Numerics.Vector3 gravity, float linearDamping = .03f, float angularDamping = .03f) : this()
    {
        Gravity = gravity;
        LinearDamping = linearDamping;
        AngularDamping = angularDamping;
    }

    Vector3Wide gravityWideDt;
    System.Numerics.Vector<float> linearDampingDt;
    System.Numerics.Vector<float> angularDampingDt;

    public void PrepareForIntegration(float dt)
    {
        linearDampingDt = new System.Numerics.Vector<float>(MathF.Pow(MathHelper.Clamp(1 - LinearDamping, 0, 1), dt));
        angularDampingDt = new System.Numerics.Vector<float>(MathF.Pow(MathHelper.Clamp(1 - AngularDamping, 0, 1), dt));
        gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
    }

    public void IntegrateVelocity(System.Numerics.Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, System.Numerics.Vector<int> integrationMask, int workerIndex, System.Numerics.Vector<float> dt, ref BodyVelocityWide velocity)
    {
        velocity.Linear = (velocity.Linear + gravityWideDt) * linearDampingDt;
        velocity.Angular = velocity.Angular * angularDampingDt;
    }
}

struct OtherCallbacks : INarrowPhaseCallbacks
{
    public SpringSettings ContactSpringiness;
    public float MaximumRecoveryVelocity;
    public float FrictionCoefficient;

    public OtherCallbacks(SpringSettings contactSpringiness, float maximumRecoveryVelocity = 2f, float frictionCoefficient = 1f)
    {
        ContactSpringiness = contactSpringiness;
        MaximumRecoveryVelocity = maximumRecoveryVelocity;
        FrictionCoefficient = frictionCoefficient;
    }

    public void Initialize(Simulation simulation)
    {
        //Use a default if the springiness value wasn't initialized... at least until struct field initializers are supported outside of previews.
        if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
        {
            ContactSpringiness = new(30, 1);
            MaximumRecoveryVelocity = 2f;
            FrictionCoefficient = 1f;
        }
    }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        //While the engine won't even try creating pairs between statics at all, it will ask about kinematic-kinematic pairs.
        //Those pairs cannot emit constraints since both involved bodies have infinite inertia. Since most of the demos don't need
        //to collect information about kinematic-kinematic pairs, we'll require that at least one of the bodies needs to be dynamic.
        return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
    }

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial.FrictionCoefficient = FrictionCoefficient;
        pairMaterial.MaximumRecoveryVelocity = MaximumRecoveryVelocity;
        pairMaterial.SpringSettings = ContactSpringiness;
        return true;
    }

    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
    {
        return true;
    }

    public void Dispose()
    {
    }
}