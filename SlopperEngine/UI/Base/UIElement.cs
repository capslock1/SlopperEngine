using System.Collections.Generic;
using System.Reflection.Metadata;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
    /// The layout of the UI element. If null, children do not get automatically laid out.
    /// </summary>
    public virtual SingleChild<LayoutHandler> Layout => internalLayout;

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
    public Box2 LastGlobalShape => _globalShape;
    private Box2 _globalShape;
    private LayoutControlState _globalShapeSetByLayout;

    /// <summary>
    /// The last size constraints of this UIElement.
    /// </summary>
    public UIElementSize LastSizeConstraints { get; private set; }

    /// <summary>
    /// The last global bounds that were calculated of the UIElement's children. 
    /// </summary>
    public Box2 LastChildrenBounds { get; private set; }

    /// <summary>
    /// A set of hidden UI elements.
    /// </summary>
    protected ChildList<UIElement, UIChildEvents> internalUIChildren { get; private set; }
    /// <summary>
    /// The hidden internal layout.
    /// </summary>
    protected SingleChild<LayoutHandler> internalLayout { get; private set; }

    SceneDataHandle _UIRootUpdateHandle;
    bool _isUIRoot;
    int _safeIterator;
    HoverState _hoverState;

    public UIElement() : this(new(0, 0, 1, 1)) { }
    public UIElement(Box2 localShape)
    {
        internalUIChildren = new(this, new(this));
        internalLayout = new(this);
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
                OnMouse = PassEvent,
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
    /// Handles a mouse event. Remember to call e.Use() when using info from the event in any way, and to also override BlocksMouse() if applicable.
    /// </summary>
    protected virtual void HandleEvent(ref MouseEvent e) { }
    /// <summary>
    /// Whether or not UIElements rendering behind this one are blocked from receiving mouse events.
    /// </summary>
    protected virtual bool BlocksMouse() => false;
    /// <summary>
    /// Whether or not this element should have its bounds included in ChildBounds.
    /// </summary>
    protected virtual bool IncludeInChildbounds() => true;


    /// <summary>
    /// Gets the global scissor of just this UIElement.
    /// </summary>
    public Box2 GetGlobalScissor()
    {
        var scissor = GetScissorRegion();
        return new(
            Vector2.Lerp(LastGlobalShape.Min, LastGlobalShape.Max, scissor.Min),
            Vector2.Lerp(LastGlobalShape.Min, LastGlobalShape.Max, scissor.Max));
    }

    /// <summary>
    /// Gets the true global scissor of the UIElement, and whether or not it renders at all.
    /// </summary>
    /// <param name="parentScissorRegion">The parent's global scissor.</param>
    /// <param name="globalScissor">The result.</param>
    /// <returns>Whether or not the global scissor has any area.</returns>
    public bool GetGlobalScissor(Box2 parentScissorRegion, out Box2 globalScissor)
    {
        globalScissor = GetGlobalScissor();
        var globalScissorMin = Vector2.ComponentMax(globalScissor.Min, parentScissorRegion.Min);
        var globalScissorMax = Vector2.ComponentMin(globalScissor.Max, parentScissorRegion.Max);
        bool validScissor = globalScissorMax.X > globalScissorMin.X && globalScissorMax.Y > globalScissorMin.Y;
        globalScissor = validScissor ? new(globalScissorMin, globalScissorMax) : new(globalScissorMin, globalScissorMin);
        return validScissor;
    }

    private void PassEvent(ref MouseEvent e) 
    {
        if(ReceiveEvent(ref e)) 
            e.Use();
    }
    private bool ReceiveEvent(ref MouseEvent e)
    {
        if(e.Type == MouseEventType.Used)
            return false;

        MouseEvent childEvent = e;

        bool childBlocks = false;
        if (LastChildrenBounds.ContainsInclusive(e.NDCPosition) || e.Type == MouseEventType.OnEndHover)
        {
            for (_safeIterator = internalUIChildren.Count - 1; _safeIterator >= 0; _safeIterator--)
            {
                var ch = internalUIChildren[_safeIterator];
                if(ch.ReceiveEvent(ref childEvent) || (ch.BlocksMouse() && ch.LastGlobalShape.ContainsInclusive(e.NDCPosition)))
                {
                    childBlocks = true;
                    break;
                }

                if (childEvent.Type == MouseEventType.Used)
                    break;
            }
        }

        if(childEvent.Type == MouseEventType.Used)
        {
            e.Use();
            return false;
        } 
        if (LastGlobalShape.ContainsInclusive(e.NDCPosition))
        {
            HandleEvent(ref e);
        }

        return childBlocks;
    }

    private void UpdateShape(Box2 parentShape, UIRenderer renderer)
    {
        LastRenderer = renderer;

        switch(_globalShapeSetByLayout)
        {
            default: // uncontrolled
            Box2 globalShape = new(
                Vector2.Lerp(parentShape.Min, parentShape.Max, LocalShape.Min),
                Vector2.Lerp(parentShape.Min, parentShape.Max, LocalShape.Max)
            );

            LastSizeConstraints = GetSizeConstraints();
            ApplySizeConstraints(ref globalShape, LastSizeConstraints, renderer);
            _globalShape = globalShape;
            break;

            case LayoutControlState.Resolving:
            _globalShapeSetByLayout = LayoutControlState.Resolved;
            break;

            case LayoutControlState.Resolved: return;
        }

        Box2 childBounds = default;
        bool boundsInitted = false;

        var trueParent = (UIElement)UIChildren.Owner;
        var trueGlobal = trueParent.LastGlobalShape;
        var trueSize = trueGlobal.Size;

        if (!(float.IsNaN(trueGlobal.Min.X) ||
            float.IsNaN(trueGlobal.Min.Y) ||
            float.IsNaN(trueGlobal.Max.X) ||
            float.IsNaN(trueGlobal.Max.Y)) &&
            float.Abs(trueSize.X) > 0.00001f &&
            float.Abs(trueSize.Y) > 0.00001f)
        {
            GlobalShapeKey key = new(this);
            internalLayout.Value?.LayoutChildren(this, ref key, trueGlobal, renderer);
        }

        for (_safeIterator = internalUIChildren.Count - 1; _safeIterator >= 0; _safeIterator--)
        {
            var ch = internalUIChildren[_safeIterator];
            ch.UpdateShape(_globalShape, renderer);
            if (!ch.IncludeInChildbounds())
                continue;

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
        var scissor = GetGlobalScissor();
        childBounds =  new(
                Vector2.ComponentMax(scissor.Min, childBounds.Min),
                Vector2.ComponentMin(scissor.Max, childBounds.Max));

        LastChildrenBounds = boundsInitted ? childBounds : new(_globalShape.Center, _globalShape.Center);
    }
    
    static void ApplySizeConstraints(ref Box2 currentGlobalShape, UIElementSize constraints, UIRenderer renderer)
    {
        var screenScale = renderer.GetPixelScale();
        float minSizeX = constraints.MinimumSizeX * screenScale.X;
        float maxSizeX = constraints.MaximumSizeX * screenScale.X;
        float minSizeY = constraints.MinimumSizeY * screenScale.Y;
        float maxSizeY = constraints.MaximumSizeY * screenScale.Y;
        var (minX, maxX) = ResizeAligned(currentGlobalShape.Min.X, currentGlobalShape.Max.X, minSizeX, maxSizeX, constraints.GrowX);
        var (minY, maxY) = ResizeAligned(currentGlobalShape.Min.Y, currentGlobalShape.Max.Y, minSizeY, maxSizeY, constraints.GrowY);

        currentGlobalShape = new(minX, minY, maxX, maxY);

        static (float min, float max) ResizeAligned(float min, float max, in float minSize, in float maxSize, in Alignment direction)
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

        Box2 NDC = new(-1, -1, 1, 1);
        Box2 global = LastGlobalShape;
        if (mat != null &&
            NDC.Min.X <= global.Max.X &&
            NDC.Max.X >= global.Min.X &&
            NDC.Min.Y <= global.Max.Y &&
            NDC.Max.X >= global.Min.Y
            )
            renderer.AddRenderToQueue(LastGlobalShape, mat, parentScissorRegion);

        if (!GetGlobalScissor(parentScissorRegion, out var trueScissor))
            return;

        for (int i = 0; i < internalUIChildren.Count; i++)
        {
            var ch = internalUIChildren[i];
            ch.Render(trueScissor, renderer);
        }
    }

    /// <summary>
    /// Allows a LayoutHandler to change a UIElement's childrens' global shapes.
    /// </summary>
    public interface IGlobalShapeKey
    {
        /// <summary>
        /// Sets the child's global shape, and applies its size constraints.
        /// </summary>
        /// <param name="childIndex">The child to set the global shape of.</param>
        /// <param name="globalShape">The shape to set.</param>
        /// <param name="renderer">The UIRenderer.</param>
        public void SetGlobalShape(int childIndex, ref Box2 globalShape, UIRenderer renderer);
    }

    ref struct GlobalShapeKey(UIElement _owner) : IGlobalShapeKey
    {
        /// <summary>
        /// Sets the child's global shape, and applies its size constraints.
        /// </summary>
        /// <param name="childIndex">The child to set the global shape of.</param>
        /// <param name="globalShape">The shape to set.</param>
        /// <param name="renderer">The UIRenderer.</param>
        public void SetGlobalShape(int childIndex, ref Box2 globalShape, UIRenderer renderer)
        {
            var element = _owner.UIChildren[childIndex];
            element.LastSizeConstraints = element.GetSizeConstraints();
            ApplySizeConstraints(ref globalShape, element.LastSizeConstraints, renderer);
            element._globalShape = globalShape;
            element._globalShapeSetByLayout = LayoutControlState.Resolving;
            element.UpdateShape((element.Parent as UIElement)?.LastGlobalShape ?? new(-1,-1,1,1), renderer);
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

    enum LayoutControlState
    {
        Uncontrolled = 0,
        Resolving, Resolved
    }

    enum HoverState
    {
        NotHovered = 0,
        Hovered,
        Unhovered
    }
}
