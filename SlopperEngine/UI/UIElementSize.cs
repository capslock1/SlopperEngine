
namespace SlopperEngine.UI;

/// <summary>
/// Describes the scaling of a UI element.
/// </summary>
/// <param name="GrowX">The way the element should grow along the X axis.</param>
/// <param name="GrowY">The way the element should grow along the Y axis.</param>
/// <param name="MinimumSizeX">The minimum size in pixels along the X axis.</param>
/// <param name="MinimumSizeY">The minimum size in pixels along the Y axis.</param>
/// <param name="MaximumSizeX">The maximum size in pixels along the X axis.</param>
/// <param name="MaximumSizeY">The maximum size in pixels along the Y axis.</param>
public record struct UIElementSize(Alignment GrowX, Alignment GrowY, int MinimumSizeX, int MinimumSizeY, int MaximumSizeX = int.MaxValue, int MaximumSizeY = int.MaxValue)
{
}