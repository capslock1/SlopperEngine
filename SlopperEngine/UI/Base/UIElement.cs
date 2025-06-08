using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Graphics;
using SlopperEngine.Graphics.Renderers;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.UI.Base;

/// <summary>
/// Describes an element of the UI, with a texture and an AABB shape.
/// </summary>
public class UIElement : SceneObject
{
    /// <summary>
    /// The size of the UIElement relative to the container.
    /// </summary>
    public Box2 LocalShape;

    /// <summary>
    /// The children of the UIElement to consider part of the UI.
    /// </summary>
    public virtual ChildList<UIElement> UIChildren => internalUIChildren;

    /// <summary>
    /// The last renderer that used this UIElement.
    /// </summary>
    public UIRenderer? LastRenderer {get; private set;}

    /// <summary>
    /// The last global shape that was calculated of this UIElement, in normalized device coordinates.
    /// </summary>
    public Box2 LastGlobalShape { get; private set; }

    /// <summary>
    /// The last size constraints of this UIElement.
    /// </summary>
    public UIElementSize LastSizeConstraints { get; private set; }

    /// <summary>
    /// The last global scissor region that was calculated of this UIElement, in normalized device coordinates.
    /// </summary>
    public Box2 LastGlobalScissor { get; private set; }
    
    /// <summary>
    /// A set of hidden UI elements.
    /// </summary>
    protected ChildList<UIElement> internalUIChildren { get; private set; }

    /// <summary>
    /// The last global bounds that were calculated of the UIElement's children. 
    /// </summary>
    protected Box2 lastChildrenIncludedBounds { get; private set; }

    SceneDataHandle _UIRootUpdateHandle;
    bool _isUIRoot;

    public UIElement() : this(new(0, 0, 1, 1)) { }
    public UIElement(Box2 localShape)
    {
        internalUIChildren = new(this);
        LocalShape = localShape;
    }

    [OnRegister] void Register()
    {
        // this doesnt really work if you accidentally add a UIElement to another UIElement's "Children" childlist. oh well
        if(_isUIRoot = Parent is not UIElement)
        {
            _UIRootUpdateHandle = Scene!.RegisterSceneData<UIRootUpdate>(new()
            {
                UpdateShape = UpdateShape,
                AddRender = Render,
                OnMouse = ReceiveEvent
                });
        }
    } 
    [OnUnregister] void Unregister(Scene scene)
    {
        if(_isUIRoot)
            scene!.UnregisterSceneData<UIRootUpdate>(_UIRootUpdateHandle, default);
    }

    /// <summary>
    /// Gets the material for this UIElement. Return null to render nothing at all.
    /// </summary>
    protected virtual Material? GetMaterial() => null;
    /// <summary>
    /// Gets the size constraints for this UIElement.
    /// </summary>
    protected virtual UIElementSize GetSizeConstraints() => new(default, default, 0, 0);
    /// <summary>
    /// The region of range 0-1 inside this UIElement where children should be cut out of.
    /// </summary>
    protected virtual Box2 GetScissorRegion() => new(Vector2.NegativeInfinity, Vector2.PositiveInfinity);
    /// <summary>
    /// Handles a mouse event. Remember to call e.Use() when using info from the event in any way (or to simply block it);
    /// </summary>
    protected virtual void HandleEvent(ref MouseEvent e) { }

    private void ReceiveEvent(ref MouseEvent e)
    {
        if (!lastChildrenIncludedBounds.ContainsInclusive(e.NDCPosition))
            return;

        if (LastGlobalScissor.ContainsInclusive(e.NDCPosition))
        {
            foreach (var ch in internalUIChildren.All)
            {
                ch.ReceiveEvent(ref e);
                if (e.Used)
                    return;
            }
        }
        if (LastGlobalShape.ContainsInclusive(e.NDCPosition))
            HandleEvent(ref e);
    }

    private void UpdateShape(Box2 parentShape, UIRenderer renderer)
    {
        LastRenderer = renderer;

        Box2 globalShape = new(
            Vector2.Lerp(parentShape.Min, parentShape.Max, LocalShape.Min),
            Vector2.Lerp(parentShape.Min, parentShape.Max, LocalShape.Max)
        );

        var screenScale = renderer.GetPixelScale();
        LastSizeConstraints = GetSizeConstraints();
        float minSizeX = LastSizeConstraints.MinimumSizeX * screenScale.X;
        float maxSizeX = LastSizeConstraints.MaximumSizeX * screenScale.X;
        float minSizeY = LastSizeConstraints.MinimumSizeY * screenScale.Y;
        float maxSizeY = LastSizeConstraints.MaximumSizeY * screenScale.Y;
        var (minX, maxX) = Resize(globalShape.Min.X, globalShape.Max.X, minSizeX, maxSizeX, LastSizeConstraints.GrowX);
        var (minY, maxY) = Resize(globalShape.Min.Y, globalShape.Max.Y, minSizeY, maxSizeY, LastSizeConstraints.GrowY);
        globalShape = new(minX, minY, maxX, maxY);

        var childBounds = globalShape;
        LastGlobalShape = globalShape;

        foreach (var ch in internalUIChildren.All)
        {
            ch.UpdateShape(globalShape, renderer);
            childBounds.Extend(ch.lastChildrenIncludedBounds.Min);
            childBounds.Extend(ch.lastChildrenIncludedBounds.Max);
        }

        lastChildrenIncludedBounds = childBounds;

        static (float min, float max) Resize(float min, float max, in float minSize, in float maxSize, in Alignment direction)
        {
            float size = max - min;
            float difference = 0;
            if (size > maxSize)
                difference = maxSize - size;
            if (size < minSize)
                difference = minSize - size;
            switch (direction)
            {
                case Alignment.Middle:
                    min -= difference * .5f;
                    max += difference * .5f;
                    break;
                case Alignment.Min:
                    min -= difference;
                    break;
                case Alignment.Max:
                    max += difference;
                    break;
            }
            return (min, max);
        }
    }
    private void Render(Box2 parentScissorRegion, UIRenderer renderer)
    {
        var mat = GetMaterial();
        var scissor = GetScissorRegion();
        Box2 globalScissor = new(
            Vector2.Lerp(LastGlobalShape.Min, LastGlobalShape.Max, scissor.Min),
            Vector2.Lerp(LastGlobalShape.Min, LastGlobalShape.Max, scissor.Max));
        globalScissor.Min = Vector2.ComponentMax(globalScissor.Min, parentScissorRegion.Min);
        globalScissor.Max = Vector2.ComponentMin(globalScissor.Max, parentScissorRegion.Max);
        LastGlobalScissor = globalScissor;

        Box2 NDC = new(-1, -1, 1, 1);
        Box2 global = LastGlobalShape;
        if (mat != null &&
            NDC.Min.X <= global.Max.X &&
            NDC.Max.X >= global.Min.X &&
            NDC.Min.Y <= global.Max.Y &&
            NDC.Max.X >= global.Min.Y
            )
            renderer.AddRenderToQueue(LastGlobalShape, mat, parentScissorRegion);

        if (globalScissor.Size.X <= 0 || globalScissor.Size.Y <= 0)
            return;

        foreach (var ch in internalUIChildren.All)
            ch.Render(globalScissor, renderer);
    }
}