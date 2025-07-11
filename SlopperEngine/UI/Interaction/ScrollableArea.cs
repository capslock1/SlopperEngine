using OpenTK.Mathematics;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;
using SlopperEngine.SceneObjects.ChildContainers;

namespace SlopperEngine.UI.Interaction;

/// <summary>
/// A UIElement that automatically adds a scrollbar when its children exceed its bounds.
/// </summary>
public class ScrollableArea : UIElement
{
    // TODO:  
    // middle click scroll gizmo. low priority
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

    /// <summary>
    /// How many pixels per scroll bump the area should move. I don't think this is a remotely accurate description but it works well enough.
    /// </summary>
    public float ScrollSensitivity = 10;

    /// <summary>
    /// How to display the horizontal slider.
    /// </summary>
    public ScrollBarDisplayMode HorizontalDisplay
    {
        get => _horizontalDisplay;
        set
        {
            _horizontalDisplay = value;
            UpdateSliderShapes();
        }
    }
    ScrollBarDisplayMode _horizontalDisplay = ScrollBarDisplayMode.Min;

    /// <summary>
    /// How to display the vertical slider.
    /// </summary>
    public ScrollBarDisplayMode VerticalDisplay
    {
        get => _verticalDisplay;
        set
        {
            _verticalDisplay = value;
            UpdateSliderShapes();
        }
    }
    ScrollBarDisplayMode _verticalDisplay = ScrollBarDisplayMode.Min;

    /// <summary>
    /// The size of the scrollbars. 
    /// </summary>
    public UISize ScrollbarSize
    {
        get => _preferredScrollbarSize;
        set
        {
            _preferredScrollbarSize = value;
            UpdateSliderShapes();
        }
    }
    UISize _preferredScrollbarSize = UISize.FromPixels(new(10,10));

    readonly ContentArea _contentArea;
    readonly UIElement _movingArea;
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
            {
                _horizontalSlider.Remove();
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
                    internalUIChildren.Add(_verticalSlider);
            }
            else if (_verticalSlider.InScene)
            {
                _verticalSlider.Remove();
            }
            UpdateSliderShapes();
        }
    }

    public ScrollableArea(Box2 shape) : base(shape)
    {
        internalUIChildren.Add(Background = new());

        var scrollbarSize = new Vector2(0.03f, 0.03f);

        internalUIChildren.Add(_contentArea = new());
        _contentArea.LocalShape = new(scrollbarSize, Vector2.One);
        _contentArea.UIChildren.Add(_movingArea = new());

        _horizontalSlider = new(1, false, 0);
        _horizontalSlider.LocalShape = new(scrollbarSize.X, 0, 1, scrollbarSize.Y);
        _horizontalSlider.OnScroll += UpdatePosition;

        _verticalSlider = new(1);
        _verticalSlider.LocalShape = new(0, scrollbarSize.Y, scrollbarSize.X, 1);
        _verticalSlider.OnScroll += UpdatePosition;

        _sliderCorner = new(new(Vector2.Zero, scrollbarSize), Style.BackgroundStrong);
        internalUIChildren.Add(_sliderCorner);
        UpdateSliderShapes();
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
            var chBounds = _movingArea.LastChildrenBounds;
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
            var chBounds = _movingArea.LastChildrenBounds;
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
        Vector2 contentStart = default;
        Vector2 contentEnd = default;
        Vector2 sliderStart = default;
        Vector2 sliderEnd = default;
        Vector2 scrollbarSize = _preferredScrollbarSize.GetLocalSize(LastGlobalShape, LastRenderer);
        if (!_horizontalSliderVisible)
            scrollbarSize.Y = 0;
        if (!_verticalSliderVisible)
            scrollbarSize.X = 0;

        SetShapeBounds(_verticalDisplay, ref contentStart.X, ref contentEnd.X, ref sliderStart.X, ref sliderEnd.X, scrollbarSize.X);
        SetShapeBounds(_horizontalDisplay, ref contentStart.Y, ref contentEnd.Y, ref sliderStart.Y, ref sliderEnd.Y, scrollbarSize.Y);
        void SetShapeBounds(ScrollBarDisplayMode mode, ref float cStart, ref float cEnd, ref float sStart, ref float sEnd, float scrollbarSize)
        {
            switch (mode)
            {
                default:
                    cStart = 0;
                    cEnd = 1;
                    break;
                case ScrollBarDisplayMode.Max:
                    cStart = 0;
                    cEnd = 1 - scrollbarSize;
                    sStart = cEnd;
                    sEnd = 1;
                    break;
                case ScrollBarDisplayMode.Min:
                    sStart = 0;
                    sEnd = scrollbarSize;
                    cStart = scrollbarSize;
                    cEnd = 1;
                    break;
            }
        }
        _horizontalSlider.LocalShape = new(contentStart.X, sliderStart.Y, contentEnd.X, sliderEnd.Y);
        _verticalSlider.LocalShape = new(sliderStart.X, contentStart.Y, sliderEnd.X, contentEnd.Y);
        _sliderCorner.LocalShape = new(sliderStart, sliderEnd);
        _contentArea.LocalShape = new(contentStart, contentEnd);
        UpdatePosition();
    }

    void UpdateContentRatio()
    {
        Vector2 inverseSize = Vector2.One / _movingArea.LastGlobalShape.Size;
        Vector2 contentRatio = _movingArea.LastChildrenBounds.Size * inverseSize;
        if (contentRatio == _currentContentRatio)
            return;

        _horizontalSlider.ContentToContainerRatio = contentRatio.X;
        _verticalSlider.ContentToContainerRatio = contentRatio.Y;
        _currentContentRatio = contentRatio;
        _movingAreaOffset = (_movingArea.LastChildrenBounds.Max - _movingArea.LastGlobalShape.Max) * inverseSize;

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
        UpdateSliderShapes();
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
            Vector2 direction = e.ScrollDelta;
            if (e.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl))
                direction = direction.Yx;

            Vector2 screenSize = LastRenderer?.GetScreenSize() ?? new(100);
            direction *= ScrollSensitivity;
            direction /= _movingArea.LastChildrenBounds.Size * screenSize;
            _horizontalSlider.ScrollValue -= float.IsNaN(direction.X) ? 0 : direction.X;
            _verticalSlider.ScrollValue += float.IsNaN(direction.Y) ? 0 : direction.Y;
            e.Use();
            return;
        }
        if (e.Type == MouseEventType.PressedButton && e.PressedButton == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
        {
            // the cool gizmo (we using a texture for this?)
            e.Use();
            return;
        }
        if (e.Type == MouseEventType.ReleasedButton && e.ReleasedButton == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle)
        {
            // release the gizmo
            e.Use();
            return;
        }
        e.Block();
    }

    protected override void OnStyleChanged()
    {
        _horizontalSlider.Style = Style;
        _verticalSlider.Style = Style;
        _sliderCorner.Color = Style.BackgroundStrong;
    }

    /// <summary>
    /// How the scrollbar should display.
    /// </summary>
    public enum ScrollBarDisplayMode
    {
        DontShow,
        Max,
        Min
    }

    class ContentArea : UIElement
    {
        protected override Box2 GetScissorRegion() => new(0, 0, 1, 1);
    }
}