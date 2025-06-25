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
public class TextButton : BaseButton
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

    public TextButton()
    {
        internalUIChildren.Add(_background = new());
        internalUIChildren.Add(_textRenderer = new(default, default));
        internalUIChildren.Add(_childHolder = new());
        OnStyleChanged();
    }

    protected override void OnStyleChanged()
    {
        _background.Color = mouseButtonsHeld > 0 ? Style.Tint : hovered ? Style.ForegroundStrong : Style.ForegroundWeak;
        _textRenderer.TextColor = mouseButtonsHeld > 0 ? Style.ForegroundStrong : Style.Tint;
        _textRenderer.Font = Style.Font;
        _textRenderer.Scale = Style.FontScale;
    }
    
    protected override UIElementSize GetSizeConstraints()
    {
        return _textRenderer.LastSizeConstraints;
    }

    protected override void OnPressed(MouseButton button)
    {
        _background.Color = Style.Tint;
        _textRenderer.TextColor = Style.ForegroundStrong;
        OnButtonPressed?.Invoke(button);
    }

    protected override void OnAnyRelease(MouseButton button)
    {
        OnButtonReleased?.Invoke(button);
    }

    protected override void OnAllButtonsReleased()
    {
        _background.Color = Style.ForegroundStrong;
        _textRenderer.TextColor = Style.Tint;
    }

    protected override void OnMouseEntry()
    {
        OnAllButtonsReleased();
    }

    protected override void OnMouseExit()
    {
        _background.Color = Style.ForegroundWeak;
        _textRenderer.TextColor = Style.Tint;
    }
}