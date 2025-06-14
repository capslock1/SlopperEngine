using System.Drawing;
using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Navigation;

/// <summary>
/// A UIElement that automatically adds a scrollbar when its children exceed its bounds.
/// </summary>
public class ScrollableArea : UIElement
{
    // TODO:  
    // configure slider colors, 
    // configure slider alignment, 
    // configure slider size, 
    // make moving area not show below slider
    // take scroll input
    public override ChildList<UIElement, UIChildEvents> UIChildren => _movingArea.UIChildren;

    readonly MovingArea _movingArea;
    readonly Slider _horizontalSlider;
    readonly Slider _verticalSlider;

    Vector2 _currentContentRatio;
    Vector2 _movingAreaOffset;

    bool _horizontalSliderVisible
    {
        get => _horizontalSlider.InScene;
        set
        {
            if (value)
            {
                if (!_horizontalSlider.InScene)
                    internalUIChildren.Add(_horizontalSlider);
            }
            else if (_horizontalSlider.InScene)
                _horizontalSlider.Remove();
        }
    }
    bool _verticalSliderVisible
    {
        get => _verticalSlider.InScene;
        set
        {
            if (value)
            {
                if (!_verticalSlider.InScene)
                    internalUIChildren.Add(_verticalSlider);
            }
            else if (_verticalSlider.InScene)
                _verticalSlider.Remove();
        }
    }

    public ScrollableArea(Box2 shape) : base(shape)
    {
        internalUIChildren.Add(_movingArea = new());

        _horizontalSlider = new(new(0, 0, 0, 0.5f), Color.White, 1, false, 0);
        _horizontalSlider.LocalShape = new(0, 0, 1, 0.05f);
        _horizontalSlider.OnScroll += OnScroll;

        _verticalSlider = new(new(0, 0, 0, 0.5f), Color4.White, 1);
        _verticalSlider.LocalShape = new(0, 0, 0.05f, 1);
        _verticalSlider.OnScroll += OnScroll;
    }

    void OnScroll()
    {
        Vector2 center = new Vector2(.5f) - _movingAreaOffset;
        Vector2 contentToContainer = new(_horizontalSlider.ContentToContainerRatio, _verticalSlider.ContentToContainerRatio);
        center += contentToContainer * new Vector2(1 - _horizontalSlider.ScrollValue, 1 - _verticalSlider.ScrollValue) * (Vector2.One - Vector2.One / contentToContainer);
        _movingArea.LocalShape.Center = center;
    }

    void UpdateContentRatio()
    {
        Vector2 inverseSize = Vector2.One / _movingArea.LastGlobalShape.Size;
        Vector2 contentRatio = _movingArea.ChildrenIncludedBounds.Size * inverseSize;
        if (contentRatio == _currentContentRatio)
            return;

        _horizontalSlider.ContentToContainerRatio = contentRatio.X;
        _verticalSlider.ContentToContainerRatio = contentRatio.Y;
        _currentContentRatio = contentRatio;
        _movingAreaOffset = (_movingArea.ChildrenIncludedBounds.Max - _movingArea.LastGlobalShape.Max) * inverseSize;

        if (_horizontalSlider.ContentToContainerRatio > 1 && !_horizontalSliderVisible)
        {
            _horizontalSlider.ScrollValue = 0;
            _horizontalSliderVisible = true;
        }
        if (_verticalSlider.ContentToContainerRatio > 1 && !_verticalSliderVisible)
        {
            _verticalSlider.ScrollValue = 1;
            _verticalSliderVisible = true;
        }
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