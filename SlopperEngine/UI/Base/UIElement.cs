using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Graphics;
using SlopperEngine.Rendering;
using SlopperEngine.SceneObjects;
using SlopperEngine.SceneObjects.ChildContainers;
using SlopperEngine.UI.Layout;
using SlopperEngine.UI.Style;

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
    public virtual ChildList<UIElement, UIChildEvents> UIChildren => internalUIChildren;

    /// <summary>
    /// The layout of the UI element. By default, children do not get automatically laid out.
    /// </summary>
    public SingleChild<LayoutHandler> Layout { get; private set; }

    /// <summary>
    /// The style of the element.
    /// </summary>
    public BasicStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            OnStyleChanged();
        }
    }
    BasicStyle _style = BasicStyle.DefaultStyle;

    /// <summary>
    /// The last renderer that used this UIElement.
    /// </summary>
    public UIRenderer? LastRenderer { get; private set; }

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
    /// The last global bounds that were calculated of the UIElement's children. 
    /// </summary>
    public Box2 LastChildrenBounds { get; private set; }


    /// <summary>
    /// A set of hidden UI elements.
    /// </summary>
    protected ChildList<UIElement, UIChildEvents> internalUIChildren { get; private set; }

    SceneDataHandle _UIRootUpdateHandle;
    bool _isUIRoot;
    int _safeIterator;

    public UIElement() : this(new(0, 0, 1, 1)) { }
    public UIElement(Box2 localShape)
    {
        internalUIChildren = new(this, new(this));
        Layout = new(this);
        LocalShape = localShape;
    }

    [OnRegister]
    void Register()
    {
        // this doesnt really work if you accidentally add a UIElement to another UIElement's "Children" childlist. oh well
        if (_isUIRoot = Parent is not UIElement)
        {
            _UIRootUpdateHandle = Scene!.RegisterSceneData<UIRootUpdate>(new()
            {
                UpdateShape = UpdateShape,
                AddRender = Render,
                OnMouse = ReceiveEvent
            });
        }
    }
    [OnUnregister]
    void Unregister(Scene scene)
    {
        if (_isUIRoot)
            scene!.UnregisterSceneData<UIRootUpdate>(_UIRootUpdateHandle, default);
    }

    /// <summary>
    /// Gets the material for this UIElement. Return null to render nothing at all. This function should be pure (does not change any elements).
    /// </summary>
    protected virtual Material? GetMaterial() => null;
    /// <summary>
    /// Gets called after the style of the UIElement gets changed.
    /// </summary>
    protected virtual void OnStyleChanged() { }
    /// <summary>
    /// Gets the size constraints for this UIElement. 
    /// </summary>
    protected virtual UIElementSize GetSizeConstraints() => new(default, default, 0, 0);
    /// <summary>
    /// The region of range 0-1 inside this UIElement where children should be cut out of. This function should be pure (does not change any elements).
    /// </summary>
    protected virtual Box2 GetScissorRegion() => new(Vector2.NegativeInfinity, Vector2.PositiveInfinity);
    /// <summary>
    /// Handles a mouse event. Remember to call e.Use() when using info from the event in any way (or to simply block it).
    /// </summary>
    protected virtual void HandleEvent(ref MouseEvent e) { }

    private void ReceiveEvent(ref MouseEvent e)
    {
        MouseEvent childEvent = e; 
        if (LastChildrenBounds.ContainsInclusive(e.NDCPosition) && LastGlobalScissor.ContainsInclusive(e.NDCPosition))
        {
            for (_safeIterator = internalUIChildren.Count - 1; _safeIterator >= 0; _safeIterator--)
            {
                var ch = internalUIChildren[_safeIterator];
                ch.ReceiveEvent(ref childEvent);
                if (childEvent.Type == MouseEventType.Used)
                {
                    e.Use();
                    return;
                }
                if (childEvent.Type == MouseEventType.Blocked)
                    break;
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

        Box2 childBounds = default;
        bool boundsInitted = false;
        LastGlobalShape = globalShape;

        for (_safeIterator = internalUIChildren.Count - 1; _safeIterator >= 0; _safeIterator--)
        {
            var ch = internalUIChildren[_safeIterator];
            ch.UpdateShape(globalShape, renderer);
            if (!boundsInitted)
            {
                childBounds = ch.LastChildrenBounds;
                childBounds.Extend(ch.LastGlobalShape.Min);
                childBounds.Extend(ch.LastGlobalShape.Max);
                boundsInitted = true;
                continue;
            }
            childBounds.Extend(ch.LastChildrenBounds.Min);
            childBounds.Extend(ch.LastChildrenBounds.Max);
            childBounds.Extend(ch.LastGlobalShape.Min);
            childBounds.Extend(ch.LastGlobalShape.Max);
        }
        var trueParent = (UIElement)UIChildren.Owner;
        var trueGlobal = trueParent.LastGlobalShape;
        var trueSize = trueGlobal.Size;
        if (!(float.IsNaN(trueGlobal.Min.X) ||
            float.IsNaN(trueGlobal.Min.Y) ||
            float.IsNaN(trueGlobal.Max.X) ||
            float.IsNaN(trueGlobal.Max.Y)) && 
            float.Abs(trueSize.X) > 0.00001f &&
            float.Abs(trueSize.Y) > 0.00001f)
            Layout.Value?.LayoutChildren(this, trueGlobal);

        LastChildrenBounds = boundsInitted ? childBounds : new(globalShape.Center, globalShape.Center);

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
        var globalScissorMin = Vector2.Lerp(LastGlobalShape.Min, LastGlobalShape.Max, scissor.Min);
        var globalScissorMax = Vector2.Lerp(LastGlobalShape.Min, LastGlobalShape.Max, scissor.Max);
        globalScissorMin = Vector2.ComponentMax(globalScissorMin, parentScissorRegion.Min);
        globalScissorMax = Vector2.ComponentMin(globalScissorMax, parentScissorRegion.Max);
        bool validScissor = globalScissorMax.X > globalScissorMin.X && globalScissorMax.Y > globalScissorMin.Y;
        LastGlobalScissor = validScissor ? new(globalScissorMin, globalScissorMax) : new(globalScissorMin, globalScissorMin);

        Box2 NDC = new(-1, -1, 1, 1);
        Box2 global = LastGlobalShape;
        if (mat != null &&
            NDC.Min.X <= global.Max.X &&
            NDC.Max.X >= global.Min.X &&
            NDC.Min.Y <= global.Max.Y &&
            NDC.Max.X >= global.Min.Y
            )
            renderer.AddRenderToQueue(LastGlobalShape, mat, parentScissorRegion);

        if (!validScissor)
            return;

        for (int i = 0; i < internalUIChildren.Count; i++)
        {
            var ch = internalUIChildren[i];
            ch.Render(LastGlobalScissor, renderer);
        }
    }

    /// <summary>
    /// Public only out of necessity. Don't use.
    /// </summary>
    public struct UIChildEvents(UIElement owner) : IChildListEvents<UIElement>
    {
        UIElement _owner = owner;

        public void OnChildAdded(UIElement child, int childIndex) { }
        public void OnChildRemoved(UIElement child, int previousIndex)
        {
            if (previousIndex < _owner._safeIterator)
                _owner._safeIterator--;
        }
    }
}
