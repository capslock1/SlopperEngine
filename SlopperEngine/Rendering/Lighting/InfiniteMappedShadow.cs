
namespace SlopperEngine.Rendering.Lighting;

public static class InfiniteMappedShadow
{
    /// <summary>
    /// Function used for mapping a single cascade to an infinite area.
    /// </summary>
    public const string GLSLInfiniteMapFunction = 
    // function on desmos: -\left(\frac{1}{dx+1}-1\right)
    // where d is a slider (meaning infMapSteepness)
    // and proper negative handling is excluded
@"
vec2 SL_ShadowInfiniteMap(vec2 viewPosition)
{
    const float infMapSteepness = 2;
    viewPosition *= infMapSteepness;
    vec2 posSigns = sign(viewPosition);
    viewPosition = vec2(-1) / (vec2(1) + abs(viewPosition)) + vec2(1);
    return posSigns * viewPosition;
}
";
}