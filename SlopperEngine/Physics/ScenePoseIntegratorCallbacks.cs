
using System.Numerics;
using BepuUtilities;
using BepuPhysics;
using SlopperEngine.Physics;
using SlopperEngine.Core.SceneComponents;

namespace SlopperEngine.Physics;
/// <summary>
/// Slopperengine's callbacks for Bepu's PoseIntegrator.
/// </summary>
public struct ScenePoseIntegratorCallbacks(PhysicsHandler owner) : IPoseIntegratorCallbacks
{
    readonly PhysicsHandler _owner = owner;
    public void Initialize(Simulation simulation){}
    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;
    public readonly bool IntegrateVelocityForKinematics => false;

    Vector3Wide gravityWideDt;

    public void PrepareForIntegration(float dt)
    {
        gravityWideDt = Vector3Wide.Broadcast(_owner.Gravity.ToSN() * dt);
    }

    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
    {
        velocity.Linear += gravityWideDt;
    }
}