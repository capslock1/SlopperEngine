using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;
using SlopperEngine.SceneObjects.ChildContainers;

namespace SlopperEngine.UI.Interaction;

/// <summary>
/// A UIElement that can slide up and down or back and forth in response to mouse input.
/// </summary>
public class Slider : UIElement
{
    /// <summary>
    /// How far up or down the slider is. 0 for min value (down/left), 1 for max value (up/right). 
    /// </summary>
    public float ScrollValue
    {
        get => _scrollValue;
        set
        {
            float newVal = float.Clamp(value, 0, 1);
            if (newVal != _scrollValue)
            {
                _scrollValue = newVal;
                UpdateBar();
                OnScroll?.Invoke();
            }
        }
    }
    float _scrollValue = 0;

    /// <summary>
    /// The ratio between the content and the container. If set below one, the slider will not be able to move.
    /// </summary>
    public float ContentToContainerRatio
    {
        get => _contentRatio;
        set
        {
            float newRatio = float.Max(value, 1);
            if (newRatio == _contentRatio || float.IsNaN(newRatio))
                return;
            _scrollValue *= _contentRatio / newRatio;
            _scrollValue = float.Clamp(_scrollValue, 0, 1);
            _contentRatio = newRatio;
            UpdateBar();
        }
    }
    float _contentRatio;

    /// <summary>
    /// Whether the slider is vertical. Horizontal if false.
    /// </summary>
    public bool Vertical
    {
        get => _vertical;
        set
        {
            _vertical = value;
            UpdateBar();
        }
    }
    bool _vertical = true;

    public float MinimumBarSize
    {
        get => _minBarSize;
        set
        {
            _minBarSize = value;
            UpdateBar();
        }
    }
    float _minBarSize = 0.1f;

    /// <summary>
    /// Gets called when the slider gets changed.
    /// </summary>
    public event Action? OnScroll;

    public override ChildList<UIElement, UIChildEvents> UIChildren => _childHolder.UIChildren;

    ColorRectangle _background;
    ColorRectangle _bar;
    UIElement _childHolder;
    bool _barHeld = false;
    bool _hovered = false;
    float _mouseBarHeldOffsetNDC = 0;
    float _barSize => float.Clamp(1 / _contentRatio, _minBarSize, 1);

    public Slider(float contentRatio, bool vertical = true, float scrollValue = 1)
    {
        _background = new(new(0, 0, 1, 1), Style.BackgroundWeak);
        _bar = new(new(0, 0.3f, 1, 0.9f), Style.ForegroundWeak);
        internalUIChildren.Add(_background);
        internalUIChildren.Add(_bar);
        internalUIChildren.Add(_childHolder = new());
        _scrollValue = float.Clamp(scrollValue, 0, 1);
        _contentRatio = float.Max(contentRatio, 1);
        _vertical = vertical;
        UpdateBar();
    }

    public Slider() : this(2){}

    [OnRegister]
    void UpdateBar()
    {
        float barSize = _barSize;
        float barPosition = (1 - barSize) * ScrollValue;
        if (Vertical)
            _bar.LocalShape = new(0, barPosition, 1, barPosition + barSize);
        else _bar.LocalShape = new(barPosition, 0, barPosition + barSize, 1);
    }

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
        if (!_barHeld)
        {
            if (_hovered && _bar.LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One))
                _bar.Color = Style.ForegroundStrong;
            else
            {
                _bar.Color = Style.ForegroundWeak;
                _hovered = false;
            }
            return;
        }

        if (args.MouseState.IsButtonReleased(MouseButton.Left))
        {
            _barHeld = false;
            _bar.Color = Style.ForegroundWeak;
            return;
        }

        Vector2 mousePosNDC = args.NormalizedMousePosition;
        mousePosNDC *= 2;
        mousePosNDC -= Vector2.One;
        if (Vertical)
            mousePosNDC.Y += _mouseBarHeldOffsetNDC;
        else mousePosNDC.X += _mouseBarHeldOffsetNDC;
        ScrollValue = MousePosToScrollValue(mousePosNDC);
    }

    float MousePosToScrollValue(Vector2 NDCMousePos)
    {
        float mousePos = Vertical ? NDCMousePos.Y : NDCMousePos.X;
        mousePos -= Vertical ? LastGlobalShape.Min.Y : LastGlobalShape.Min.X;
        mousePos /= Vertical ? LastGlobalShape.Size.Y : LastGlobalShape.Size.X; // range 0-1
        float barSize = _barSize;
        float barRange = 1 - barSize;
        if (barRange <= 0)
            return 0;
        mousePos -= .5f * barSize;
        mousePos /= barRange;
        return mousePos;
    }

    protected override void OnStyleChanged()
    {
        _background.Color = Style.BackgroundWeak;
        _bar.Color = _barHeld ? Style.Tint : _hovered ? Style.ForegroundStrong : Style.ForegroundWeak;
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        _hovered = true;
        if (e.PressedButton != MouseButton.Left)
        {
            e.Block();
            return;
        }

        _mouseBarHeldOffsetNDC = Vertical ? _bar.LastGlobalShape.Center.Y : _bar.LastGlobalShape.Center.X;
        _mouseBarHeldOffsetNDC -= Vertical ? e.NDCPosition.Y : e.NDCPosition.X;
        _barHeld = true;
        _bar.Color = Style.Tint;
        if (!_bar.LastGlobalShape.ContainsInclusive(e.NDCPosition))
        {
            ScrollValue = MousePosToScrollValue(e.NDCPosition);
            _mouseBarHeldOffsetNDC = 0;
        }
        e.Use();
    }
}