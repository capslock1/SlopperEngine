using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace SlopperEngine.Graphics.Lighting;

/// <summary>
/// Struct for how shadowless lights are represented in SlopperEngine.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct LightGLSL
{
    /// <summary>
    /// The color of the light and how far it affects objects. If range is negative, it will instead be interpreted as a directional light.
    /// </summary>
    [FieldOffset(0)]
    public Vector4 ColorRange;

    /// <summary>
    /// The position and sharpness of the light - unless it is a directional light, in which case it stores only direction.
    /// </summary>
    [FieldOffset(16)]
    public Vector4 PositionSharp;
}