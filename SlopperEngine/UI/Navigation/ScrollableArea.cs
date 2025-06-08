using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Navigation;

/// <summary>
/// A UIElement that automatically adds a scrollbar when its children exceed its bounds.
/// </summary>
public class ScrollableArea : UIElement
{
    // TODO: horizontal slider, configure slider colors, configure slider alignment, configure slider size
    public override ChildList<UIElement> UIChildren => _movingArea.UIChildren;

    readonly MovingArea _movingArea;
    readonly Slider _verticalSlider;

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
        _movingArea.LocalShape.Center = new(
            0.5f,
            0.5f + _verticalSlider.ContentToContainerRatio * _verticalSlider.ScrollValue);
    }

    void UpdateContentRatio()
    {
        float contentRatioVert = _movingArea.ChildrenIncludedBounds.Size.Y / _movingArea.LastGlobalShape.Size.Y;
        _verticalSlider.ContentToContainerRatio = contentRatioVert;
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
        public Box2 ChildrenIncludedBounds => lastChildrenIncludedBounds;
    }
}