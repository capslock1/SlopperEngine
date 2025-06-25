using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;
using SlopperEngine.UI.Text;

namespace SlopperEngine.UI.Interaction;

/// <summary>
/// A button with text on it.
/// </summary>
public class TextButton : UIElement
{
    /// <summary>
    /// Gets called when the text button gets pressed.
    /// </summary>
    public event Action<MouseButton>? OnButtonPressed;
    /// <summary>
    /// Gets called when the text button gets released.
    /// </summary>
    public event Action<MouseButton>? OnButtonReleased;

    /// <summary>
    /// The text to display on this button.
    /// </summary>
    public string Text
    {
        get => _textRenderer.Text;
        set => _textRenderer.Text = value;
    }

    public override ChildList<UIElement, UIChildEvents> UIChildren => _childHolder.UIChildren;

    readonly TextBox _textRenderer;
    readonly ColorRectangle _background;
    readonly UIElement _childHolder;
    bool _hovered;
    int _mouseButtonsHeld;

    public TextButton()
    {
        internalUIChildren.Add(_background = new());
        internalUIChildren.Add(_textRenderer = new(default, default));
        internalUIChildren.Add(_childHolder = new());
        OnStyleChanged();
    }

    [OnInputUpdate]
    void InputUpdate(InputUpdateArgs args)
    {
        if (_hovered)
        {
            if (LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One))
                _background.Color = Style.ForegroundStrong;
            else
            {
                _hovered = false;
                _background.Color = Style.ForegroundWeak;
                _mouseButtonsHeld = 0;
                _textRenderer.TextColor = Style.Tint;
            }
        }

        if (_mouseButtonsHeld > 0)
        {
            _background.Color = Style.Tint;
            _textRenderer.TextColor = Style.ForegroundStrong;
        }
    }

    protected override void OnStyleChanged()
    {
        _background.Color = _mouseButtonsHeld > 0 ? Style.Tint : _hovered ? Style.ForegroundStrong : Style.ForegroundWeak;
        _textRenderer.TextColor = _mouseButtonsHeld > 0 ? Style.ForegroundStrong : Style.Tint;
        _textRenderer.Font = Style.Font;
        _textRenderer.Scale = Style.FontScale;
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        _hovered = true;
        if (e.Type == MouseEventType.PressedButton)
        {
            _mouseButtonsHeld++;
            OnButtonPressed?.Invoke(e.PressedButton);
            _background.Color = Style.Tint;
            _textRenderer.TextColor = Style.ForegroundStrong;
        }
        if (e.Type == MouseEventType.ReleasedButton && _mouseButtonsHeld>0)
        {
            _mouseButtonsHeld--;
            OnButtonReleased?.Invoke(e.ReleasedButton);
            if (_mouseButtonsHeld == 0)
            {
                _background.Color = Style.ForegroundStrong;
                _textRenderer.TextColor = Style.Tint;
            }
        }
    }
    
    protected override UIElementSize GetSizeConstraints()
    {
        return _textRenderer.LastSizeConstraints;
    }
}