using OpenTK.Mathematics;
using SlopperEngine.Rendering;

namespace SlopperEngine.Graphics.Lighting;

/// <summary>
/// Stores the data of a directional light.
/// </summary>
public struct DirectionalLightData
{
    public Vector3 Color;
    public DirectionalLight Object;
}
