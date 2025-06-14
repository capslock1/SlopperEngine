using System.Collections;
using OpenTK.Audio.OpenAL;
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
    // take scroll input
    public override ChildList<UIElement, UIChildEvents> UIChildren => _movingArea.UIChildren;

    /// <summary>
    /// How to horizontally align the content when the content is thinner than the container.
    /// </summary>
    public Alignment HorizontalContentAlignment = Alignment.Min;
    /// <summary>
    /// How to vertically align the content when the content is shorter than the container.
    /// </summary>
    public Alignment VerticalContentAlignment = Alignment.Max;

    /// <summary>
    /// The background of this scrollable area. Scrollbars and the content will show over this.
    /// </summary>
    public readonly UIElement Background;

    readonly ContentArea _contentArea;
    readonly MovingArea _movingArea;
    readonly Slider _horizontalSlider;
    readonly Slider _verticalSlider;
    readonly ColorRectangle _sliderCorner;

    Vector2 _currentContentRatio;
    Vector2 _movingAreaOffset;
    Vector2 _scrollValues
    {
        get
        {
            Vector2 res = new(_horizontalSlider.ScrollValue, _verticalSlider.ScrollValue);
            if (!_horizontalSliderVisible) res.X = 0;
            if (!_verticalSliderVisible) res.Y = 1;
            return res;
        }
    }
    Vector2 _scrollbarSize = new(0.05f,0.05f);
    Vector2 _preferredScrollbarSize = new(0.05f,0.05f);

    bool _horizontalSliderVisible
    {
        get => _horizontalSlider.InScene;
        set
        {
            if (value)
            {
                if (!_horizontalSlider.InScene)
                {
                    internalUIChildren.Add(_horizontalSlider);
                    _scrollbarSize.Y = _preferredScrollbarSize.Y;
                }
            }
            else if (_horizontalSlider.InScene)
            {
                _horizontalSlider.Remove();
                _scrollbarSize.Y = 0;
            }
            UpdateSliderShapes();
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
                {
                    internalUIChildren.Add(_verticalSlider);
                    _scrollbarSize.X = _preferredScrollbarSize.X;
                }
            }
            else if (_verticalSlider.InScene)
            {
                _verticalSlider.Remove();
                _scrollbarSize.X = 0;
            }
            UpdateSliderShapes();
        }
    }

    public ScrollableArea(Box2 shape) : base(shape)
    {
        internalUIChildren.Add(Background = new());
        Background.UIChildren.Add(new ColorRectangle(new(0, 0, 1, 1), new(1, 0, 0, 0.1f)));

        internalUIChildren.Add(_contentArea = new());
        _contentArea.LocalShape = new(_scrollbarSize, Vector2.One);
        _contentArea.UIChildren.Add(_movingArea = new());

        _horizontalSlider = new(new(0, 0, 0, 0.5f), Color4.White, 1, false, 0);
        _horizontalSlider.LocalShape = new(_scrollbarSize.X, 0, 1, _scrollbarSize.Y);
        _horizontalSlider.OnScroll += UpdatePosition;

        _verticalSlider = new(new(0, 0, 0, 0.5f), Color4.White, 1);
        _verticalSlider.LocalShape = new(0, _scrollbarSize.Y, _scrollbarSize.X, 1);
        _verticalSlider.OnScroll += UpdatePosition;

        _sliderCorner = new(new(Vector2.Zero, _scrollbarSize), new(0, 0, 0, 0.35f));
        internalUIChildren.Add(_sliderCorner);

    }

    void UpdatePosition()
    {
        Vector2 center = new Vector2(.5f) - _movingAreaOffset;
        Vector2 contentToContainer = new(_horizontalSlider.ContentToContainerRatio, _verticalSlider.ContentToContainerRatio);
        center +=
            contentToContainer *
            (Vector2.One - _scrollValues) *
            (Vector2.One - Vector2.One / contentToContainer);

        if (!_horizontalSliderVisible)
        {
            var chBounds = _movingArea.ChildrenIncludedBounds;
            var conBounds = _contentArea.LastGlobalShape;
            float offset;
            switch (HorizontalContentAlignment)
            {
                default:
                    offset = conBounds.Center.X - chBounds.Center.X;
                    break;
                case Alignment.Min:
                    offset = conBounds.Min.X - chBounds.Min.X;
                    break;
                case Alignment.Max:
                    offset = conBounds.Max.X - chBounds.Max.X;
                    break;
            }
            offset /= _movingArea.LastGlobalShape.Size.X;
            center.X = _movingArea.LocalShape.Center.X + offset;
        }
        if (!_verticalSliderVisible)
        {
            var chBounds = _movingArea.ChildrenIncludedBounds;
            var conBounds = _contentArea.LastGlobalShape;
            float offset;
            switch (VerticalContentAlignment)
            {
                default:
                    offset = conBounds.Center.Y - chBounds.Center.Y;
                    break;
                case Alignment.Min:
                    offset = conBounds.Min.Y - chBounds.Min.Y;
                    break;
                case Alignment.Max:
                    offset = conBounds.Max.Y - chBounds.Max.Y;
                    break;
            }
            offset /= _movingArea.LastGlobalShape.Size.Y;
            center.Y = _movingArea.LocalShape.Center.Y + offset;
        }
        if (float.IsNaN(center.X) || float.IsNaN(center.Y))
                return;

        _movingArea.LocalShape.Center = center;
    }

    void UpdateSliderShapes()
    {
        _horizontalSlider.LocalShape = new(_scrollbarSize.X, 0, 1, _scrollbarSize.Y);
        _verticalSlider.LocalShape = new(0, _scrollbarSize.Y, _scrollbarSize.X, 1);
        _sliderCorner.LocalShape = new(Vector2.Zero, _scrollbarSize);
        _contentArea.LocalShape = new(_scrollbarSize, Vector2.One);
        UpdatePosition();
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
        if (_horizontalSlider.ContentToContainerRatio == 1 && _horizontalSliderVisible)
        {
            _horizontalSlider.ScrollValue = 0;
            _horizontalSliderVisible = false;
        }

        if (_verticalSlider.ContentToContainerRatio > 1 && !_verticalSliderVisible)
        {
            _verticalSlider.ScrollValue = 1;
            _verticalSliderVisible = true;
        }
        if (_verticalSlider.ContentToContainerRatio == 1 && _verticalSliderVisible)
        {
            _verticalSlider.ScrollValue = 1;
            _verticalSliderVisible = false;
        }
        UpdatePosition();
    }

    protected override UIElementSize GetSizeConstraints()
    {
        UpdateContentRatio();
        return base.GetSizeConstraints();
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        if (e.Type == MouseEventType.Scroll)
        {
            // obvious code, but how do we detect if its a ctrl+scroll (for horizontal scrolling)?
        }
        if (e.Type == MouseEventType.PressedButton && e.PressedButton == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
        {
            // the cool gizmo (we using a texture for this?)
        }
        if (e.Type == MouseEventType.ReleasedButton && e.ReleasedButton == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
        {
            // release the gizmo
        }
    }

    class ContentArea : UIElement
    {
        protected override Box2 GetScissorRegion() => new(0, 0, 1, 1);
    }
    class MovingArea : UIElement
    {
        public Box2 ChildrenIncludedBounds => lastChildrenBounds;
    }
}