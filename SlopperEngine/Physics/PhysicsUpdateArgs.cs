namespace SlopperEngine.Physics;

public class PhysicsUpdateArgs
{
    public readonly float DeltaTime;
    public PhysicsUpdateArgs(float dt)
    {
        DeltaTime = dt;
    }
}