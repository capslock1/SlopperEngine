using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace SlopperEngine.Rendering.Lighting;

/// <summary>
/// Struct for lights that cast shadows.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct ShadowCasterGLSL
{
    /// <summary>
    /// The view projection matrix of the shadow caster. Should project everything into a 1x1x1 cube.
    /// </summary>
    [FieldOffset(0)]
    public Matrix4 ViewProjection;

    /// <summary>
    /// The color of the light. Alpha is ignored.
    /// </summary>
    [FieldOffset(16*sizeof(float))]
    public Vector4 Color;

    /// <summary>
    /// The indices of textures of the cascades in the shadow caster.
    /// </summary>
    [FieldOffset((16 + 4)*sizeof(float))]
    public Vector4i CascadeIndices;

    /// <summary>
    /// The sizes of every cascade in the shadow caster.
    /// </summary>
    [FieldOffset((16 + 8)*sizeof(float))]
    public Vector4 CascadeSizes;
    
    /// <summary>
    /// Padding to keep this struct within std140 size.
    /// </summary>
    [FieldOffset((16 + 12)*sizeof(float))]
    public Vector4i Padding;
}