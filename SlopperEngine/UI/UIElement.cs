using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Graphics;
using SlopperEngine.Graphics.Renderers;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.UI;

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
    public ChildList<UIElement> UIChildren;

    /// <summary>
    /// The last renderer that used this UIElement.
    /// </summary>
    protected UIRenderer? lastRenderer {get; private set;}
    /// <summary>
    /// The last global shape that was calculated of this UIElement, in normalized device coordinates.
    /// </summary>
    protected Box2 lastGlobalShape {get; private set;}

    bool _isUIRoot;
    SceneDataHandle _UIRootUpdateHandle;

    public UIElement() : this(new(0,0,1,1)){}
    public UIElement(Box2 localShape)
    {
        UIChildren = new(this);
        LocalShape = localShape;
    }

    [OnRegister] void Register()
    {
        //this doesnt really work if you accidentally add a UIElement to another UIElement's "Children" childlist. oh well
        if(_isUIRoot = Parent is not UIElement)
        {
            _UIRootUpdateHandle = Scene!.RegisterSceneData<UIRootUpdate>(new(){
                UpdateShape = UpdateShape, 
                AddRender = Render
                });
        }
    } 
    [OnUnregister] void Unregister(Scene scene)
    {
        if(_isUIRoot)
            scene!.UnregisterSceneData<UIRootUpdate>(_UIRootUpdateHandle, default);
    }

    protected virtual Material? GetMaterial() => null;
    protected virtual UIElementSize GetSizeConstraints() => new(default, default, 0, 0);
    private void UpdateShape(Box2 parentShape, UIRenderer renderer)
    {
        lastRenderer = renderer;

        Box2 globalShape = new(
            float.Lerp(parentShape.Min.X, parentShape.Max.X, LocalShape.Min.X),
            float.Lerp(parentShape.Min.Y, parentShape.Max.Y, LocalShape.Min.Y),
            float.Lerp(parentShape.Min.X, parentShape.Max.X, LocalShape.Max.X),
            float.Lerp(parentShape.Min.Y, parentShape.Max.Y, LocalShape.Max.Y)
        );

        var screenScale = renderer.GetPixelScale();
        UIElementSize size = GetSizeConstraints();
        float minSizeX = size.MinimumSizeX * screenScale.X;
        float maxSizeX = size.MaximumSizeX * screenScale.X;
        float minSizeY = size.MinimumSizeY * screenScale.Y;
        float maxSizeY = size.MaximumSizeY * screenScale.Y;
        var (minX, maxX) = Resize(globalShape.Min.X, globalShape.Max.X, minSizeX, maxSizeX, size.GrowX);
        var (minY, maxY) = Resize(globalShape.Min.Y, globalShape.Max.Y, minSizeY, maxSizeY, size.GrowY);
        globalShape = new(minX,minY,maxX,maxY);

        foreach(var ch in UIChildren.All)
            ch.UpdateShape(globalShape, renderer);

        lastGlobalShape = globalShape;

        static (float min, float max) Resize(float min, float max, in float minSize, in float maxSize, in Alignment direction)
        {
            float size = max - min;
            float difference = 0;
            if(size > maxSize)
                difference = maxSize - size; 
            if(size < minSize)
                difference = minSize - size;
            switch(direction)
            {
                case Alignment.Middle:
                min -= difference*.5f;
                max += difference*.5f;
                break;
                case Alignment.Min:
                min -= difference;
                break;
                case Alignment.Max:
                max += difference;
                break;
            }
            return (min,max);
        }
    }
    private void Render(UIRenderer renderer)
    {
        var tex = GetMaterial();
        if(tex != null)
        renderer.AddRenderToQueue(lastGlobalShape, tex);
        foreach(var ch in UIChildren.All)
            ch.Render(renderer);
    }

    protected struct UIElementSize(Alignment growX, Alignment growY, int minimumSizeX, int minimumSizeY, int maximumSizeX = int.MaxValue, int maximumSizeY = int.MaxValue)
    {
        public Alignment GrowX = growX;
        public Alignment GrowY = growY;
        public int MinimumSizeX = minimumSizeX;
        public int MinimumSizeY = minimumSizeY;
        public int MaximumSizeX = maximumSizeX;
        public int MaximumSizeY = maximumSizeY;
    }
}