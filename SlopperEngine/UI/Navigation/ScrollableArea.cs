using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Navigation;

/// <summary>
/// A UIElement that automatically adds a scrollbar when its children exceed its bounds.
/// </summary>
public class ScrollableArea : UIElement
{
    // TODO: horizontal slider, 
    // configure slider colors, 
    // configure slider alignment, 
    // configure slider size, 
    // make moving area not show below slider
    public override ChildList<UIElement, UIChildEvents> UIChildren => _movingArea.UIChildren;

    readonly MovingArea _movingArea;
    readonly Slider _verticalSlider;

    Vector2 _movingAreaOffset;

    public ScrollableArea(Box2 shape) : base(shape)
    {
        internalUIChildren.Add(_movingArea = new());

        _verticalSlider = new(new(0, 0, 0, 0.5f), Color4.White, 1);
        _verticalSlider.LocalShape = new(0, 0, 0.05f, 1);
        _verticalSlider.OnScroll += OnScroll;
        internalUIChildren.Add(_verticalSlider);
    }

    void OnScroll()
    {
        Vector2 center = new Vector2(.5f) - _movingAreaOffset;
        center.Y += _verticalSlider.ContentToContainerRatio * (1 - _verticalSlider.ScrollValue) * (1 - 1 / _verticalSlider.ContentToContainerRatio);
        _movingArea.LocalShape.Center = center;
    }

    void UpdateContentRatio()
    {
        float inverseSize = 1 / _movingArea.LastGlobalShape.Size.Y;
        float contentRatioVert = _movingArea.ChildrenIncludedBounds.Size.Y * inverseSize;
        _verticalSlider.ContentToContainerRatio = contentRatioVert;
        _movingAreaOffset.Y = (_movingArea.ChildrenIncludedBounds.Max.Y - _movingArea.LastGlobalShape.Max.Y) * inverseSize;
    }

    protected override UIElementSize GetSizeConstraints()
    {
        UpdateContentRatio();
        return base.GetSizeConstraints();
    }

    protected override Box2 GetScissorRegion() => new(0, 0, 1, 1);

    protected override void HandleEvent(ref MouseEvent e)
    {
        // scroll using scroll wheel here
    }

    class MovingArea : UIElement
    {
        public Box2 ChildrenIncludedBounds => lastChildrenBounds;
    }
}