using OpenTK.Mathematics;
using SlopperEngine.Rendering;

namespace SlopperEngine.Graphics.Lighting;

/// <summary>
/// Stores the data of a point light.
/// </summary>
public struct PointLightData
{
    public Vector3 Color;
    public float Radius;
    public float Sharpness;
    public PointLight Object;
}
