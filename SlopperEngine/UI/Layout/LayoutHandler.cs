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
    public abstract void LayoutChildren<TLayoutKey>(UIElement owner, ref TLayoutKey layoutKey, Box2 parentShape, UIRenderer renderer) where TLayoutKey : UIElement.IGlobalShapeKey, allows ref struct;

    /// <summary>
    /// Whether or not the LayoutHandler adds padding.
    /// </summary>
    /// <param name="currentlyAddedPadding">How much padding is added by the layout in NDC.</param>
    public abstract bool AddsPadding(Box2 parentShape, UIRenderer renderer, out Vector2 currentlyAddedPadding);
}