using OpenTK.Mathematics;
using SlopperEngine.Rendering;
using SlopperEngine.SceneObjects;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Layout;

/// <summary>
/// Base class for UI layout handlers.
/// </summary>
public abstract class LayoutHandler : SceneObject
{
    /// <summary>
    /// Gets called when the owner starts laying out its children.
    /// </summary>
    public abstract void LayoutChildren(UIElement owner, Box2 parentShape);

    /// <summary>
    /// Computes the global shape of a given child.
    /// </summary>
    public virtual Box2 ComputeGlobalShape(Box2 parentShape, Box2 localShape)
    {
        return new(
            Vector2.Lerp(parentShape.Min, parentShape.Max, localShape.Min),
            Vector2.Lerp(parentShape.Min, parentShape.Max, localShape.Max)
        );
    }
}