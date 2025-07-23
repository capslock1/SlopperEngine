using System.Drawing;
using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Graphics;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Text;

/// <summary>
/// Contains text.
/// </summary>
public class TextBox : UIElement
{
    /// <summary>
    /// The text to display on this box. May resize the textbox.
    /// </summary>
    public string Text
    {
        get => _currentText;
        set
        {
            if (_currentText == value) return;
            _currentText = value;
            _invalidateTexture = true;
        }
    }
    string _currentText = string.Empty;

    /// <summary>
    /// The font to use in the box. May resize this textbox.
    /// </summary>
    public RasterFont Font
    {
        get => _currentFont;
        set
        {
            if (_currentFont == value) return;
            _currentFont = value;
            _invalidateTexture = true;
        }
    }
    RasterFont _currentFont = RasterFont.EightXSixteen;

    /// <summary>
    /// The color of the text.
    /// </summary>
    public Color4 TextColor
    {
        get => (Color4)_textColor;
        set
        {
            var val = (Vector4)value;
            if (_textColor == val) return;
            _textColor = val;
            _material.Uniforms[_matTextColIndex].Value = val;
        }
    }
    Vector4 _textColor;

    /// <summary>
    /// The color of the background.
    /// </summary>
    public Color4 BackgroundColor
    {
        get => (Color4)_backgroundColor;
        set
        {
            var val = (Vector4)value;
            if (_backgroundColor == val) return;
            _backgroundColor = val;
            _material.Uniforms[_matBackgroundColIndex].Value = val;
        }
    }
    Vector4 _backgroundColor;

    /// <summary>
    /// The scale of the text box. 
    /// </summary>
    public int Scale = 2;

    /// <summary>
    /// How to horizontally align the box. Max (rightward) on default.
    /// </summary>
    public Alignment Horizontal = Alignment.Max;

    /// <summary>
    /// How to vertically align the box. Min (downward) on default.
    /// </summary>
    public Alignment Vertical = Alignment.Min;

    [DontSerialize] Texture2D? _texture;
    [DontSerialize] Material _material;
    bool _invalidateTexture = true;

    static SlopperShader? _shader;
    static int _materialTexIndex = -1;
    static int _matBackgroundColIndex = -1;
    static int _matTextColIndex = -1;

#pragma warning disable CS8618
    public TextBox(Color4 textColor, Color4 backgroundColor = default) : base()
#pragma warning restore CS8618
    {
        Init();
        TextColor = textColor;
        BackgroundColor = backgroundColor;
    }
    public TextBox() : this(Color.White, default) { }
    public TextBox(string text) : this()
    {
        Text = text;
    }
    public TextBox(string text, Color4 textColor, Color4 backgroundColor = default) : this(textColor, backgroundColor)
    {
        Text = text;
    }

    [OnSerialize]
    void OnSerialize(OnSerializeArgs serializer)
    {
        if (serializer.IsWriter)
        {
            Init();
            Text = _currentText;
            Font = _currentFont;
            BackgroundColor = (Color4)_backgroundColor;
            TextColor = (Color4)_textColor;
        }
    }

    void Init()
    {
        _shader ??= SlopperShader.Create(Assets.GetPath("shaders/UI/TextBox.sesl", "EngineAssets"));
        _material = Material.Create(_shader);
        if (_materialTexIndex == -1)
        {
            _materialTexIndex = _material.GetUniformIndexFromName("TextTexture");
            _matBackgroundColIndex = _material.GetUniformIndexFromName("BackgroundColor");
            _matTextColIndex = _material.GetUniformIndexFromName("TextColor");
        }
    }

    protected override Material? GetMaterial() => _material;
    protected override UIElementSize GetSizeConstraints()
    {
        if (_invalidateTexture)
        {
            _invalidateTexture = false;
            _texture?.Dispose();
            _texture = RasterFontWriter.WriteToTexture2D(Text, Font);
            _material.Uniforms[_materialTexIndex].Value = _texture;
        }
        UIElementSize res = new(Horizontal, Vertical,
            _texture?.Width * Scale ?? 0,
            _texture?.Height * Scale ?? Font.CharacterSize.Y,
            _texture?.Width * Scale ?? 0,
            _texture?.Height * Scale ?? Font.CharacterSize.Y);
        return res;
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        _texture?.Dispose();
    }
}