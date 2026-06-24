
namespace SlopperEngine.Rendering.Lighting;

public static class InfiniteMappedShadow
{
    /// <summary>
    /// Function used for mapping a single cascade to an infinite area.
    /// </summary>
    public const string GLSLInfiniteMapFunction = 
    // function on desmos: \frac{\left(\sqrt{x+dd}-d\right)}{\sqrt{dd+1}-d}\
    // where x is viewPosition (only one axis)
    // where d is a slider (meaning infMapSteepness)
    // and proper negative handling is excluded
@"
vec2 SL_ShadowInfiniteMap(vec2 viewPosition)
{
    const float infMapSteepness = 0.05;
    vec2 posSigns = sign(viewPosition);
    // viewPosition = (vec2(-1) + sqrt(abs(viewPosition * 50)+vec2(1)))*infMapSteepness;
    viewPosition = (sqrt(abs(viewPosition) + infMapSteepness * infMapSteepness) - infMapSteepness) / (sqrt(infMapSteepness*infMapSteepness + 1) - infMapSteepness);
    return posSigns * viewPosition;
}
";
}