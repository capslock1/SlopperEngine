using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Graphics.Loaders;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;

namespace SlopperEngine.UI.Interaction;

/// <summary>
/// A toggle button.
/// </summary>
public class ToggleButton : BaseButton
{

    /// <summary>
    /// How to horizontally align the button. Max (rightward) on default.
    /// </summary>
    public Alignment Horizontal = Alignment.Max;

    /// <summary>
    /// How to vertically align the button. Min (downward) on default.
    /// </summary>
    public Alignment Vertical = Alignment.Min;

    /// <summary>
    /// Whether or not the toggle has been checked.
    /// </summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value)
                return;

            _checked = value;
            if (value)
                _check.Color = Style.Tint;
            else _check.Color = default;

            OnToggle?.Invoke(value);
        }
    }
    bool _checked;

    /// <summary>
    /// Gets called when the button gets toggled in any way.
    /// </summary>
    public event Action<bool>? OnToggle;

    Texture2D _checkImage;
    ImageRectangle _check;
    ColorRectangle _background;

    public ToggleButton(Texture2D texture)
    {
        _checkImage = texture;
        _check = new(new(0, 0, 1, 1), _checkImage, default);
        _background = new(new(0, 0, 1, 1), Style.BackgroundWeak);
        UIChildren.Add(_background);
        UIChildren.Add(_check);
    }
    public ToggleButton() : this(
        TextureLoader.FromFilepath(Assets.GetPath("defaultTextures/checkmark.png", "EngineAssets"))
    ){}

    protected override void OnStyleChanged()
    {
        _background.Color = mouseButtonsHeld > 0 ? Style.Tint : hovered ? Style.ForegroundStrong : Style.ForegroundWeak;
        if (_checked)
            _check.Color = Style.Tint;
    }

    protected override UIElementSize GetSizeConstraints() =>
        new(Horizontal,
            Vertical,
            _checkImage.Width,
            _checkImage.Height,
            _checkImage.Width,
            _checkImage.Height);

    protected override void OnAllButtonsReleased()
    {
        _background.Color = Style.ForegroundStrong;
    }

    protected override void OnAnyRelease(MouseButton button){}

    protected override void OnMouseEntry()
    {
        _background.Color = Style.ForegroundStrong;
    }

    protected override void OnMouseExit()
    {
        _background.Color = Style.ForegroundWeak;
    }

    protected override void OnPressed(MouseButton button)
    {
        _background.Color = Style.Tint;
        Checked = !Checked;
    }
}