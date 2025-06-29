using OpenTK.Mathematics;
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
    public abstract void LayoutChildren(UIElement owner, Box2 ownerShape);
}