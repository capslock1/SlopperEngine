namespace SlopperEngine.Physics;

/// <summary>
/// Extends methods to help in physics.
/// </summary>
public static class PhysicsMethodExtensions
{
    public static System.Numerics.Vector3 ToSN(this OpenTK.Mathematics.Vector3 vec) => new(vec.X, vec.Y, vec.Z);
    public static OpenTK.Mathematics.Vector3 ToOTK(this System.Numerics.Vector3 vec) => new(vec.X, vec.Y, vec.Z);

    public static System.Numerics.Quaternion ToSN(this OpenTK.Mathematics.Quaternion quat) => new(quat.X, quat.Y, quat.Z, quat.W);
    public static OpenTK.Mathematics.Quaternion ToOTK(this System.Numerics.Quaternion quat) => new(quat.X, quat.Y, quat.Z, quat.W);
}