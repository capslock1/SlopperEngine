using OpenTK.Mathematics;
using SlopperEngine.Rendering;

namespace SlopperEngine.UI;

/// <summary>
/// Contains text.
/// </summary>
public class TextBox : UIElement
{
    /// <summary>
    /// The text to display on this box. May resize the textbox.
    /// </summary>
    public string Text{
        get => _currentText;
        set
        {
            if(_currentText == value) return;
            _currentText = value;
            _texture?.Dispose();
            _texture = null;
        }
    }

    /// <summary>
    /// The font to use in the box. May resize this textbox.
    /// </summary>
    public RasterFont Font{
        get => _currentFont;
        set
        {
            if(_currentFont == value) return;
            _currentFont = value;
            _texture?.Dispose();
            _texture = null;
        }
    }

    public Vector4 TextColor{
        get => _textColor;
        set
        {
            if(_textColor == value) return;
            _textColor = value;
            _material.Uniforms[_matTextColIndex].Value = value;
        }
    }
    public Vector4 BackgroundColor{
        get => _backgroundColor;
        set
        {
            if(_backgroundColor == value) return;
            _backgroundColor = value;
            _material.Uniforms[_matBackgroundColIndex].Value = value;
        }
    }

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

    string _currentText = string.Empty;
    RasterFont _currentFont = RasterFont.EightXSixteen;
    Texture2D? _texture;
    Material _material;
    Vector4 _textColor;
    Vector4 _backgroundColor;
    
    static SlopperShader? _shader;
    static int _materialTexIndex = -1;
    static int _matBackgroundColIndex = -1;
    static int _matTextColIndex = -1; 

    public TextBox(Vector4 textColor, Vector4 backgroundColor = default) : base()
    {
        _shader ??= SlopperShader.Create("shaders/UI/TextBox.sesl");
        _material = Material.Create(_shader);
        if(_materialTexIndex == -1)
        {
            _materialTexIndex = _material.GetUniformIndexFromName("TextTexture");
            _matBackgroundColIndex = _material.GetUniformIndexFromName("BackgroundColor");
            _matTextColIndex = _material.GetUniformIndexFromName("TextColor");
        }
        TextColor = textColor;
        BackgroundColor = backgroundColor;
    }
    public TextBox() : this(new(1,1,1,1), default){}
    public TextBox(string text) : this()
    {
        Text = text;
    }
    public TextBox(string text, Vector4 textColor, Vector4 backgroundColor = default) : this(textColor, backgroundColor)
    {
        Text = text;
    }

    protected override Material? GetMaterial() => _material;
    protected override UIElementSize GetSizeConstraints()
    {
        if(_texture == null) 
        {
            _texture = RasterFontWriter.WriteToTexture2D(Text, Font);
            _material.Uniforms[_materialTexIndex].Value = _texture;
        }
        UIElementSize res = new(Horizontal, Vertical, 
            _texture?.Width*Scale ?? 0, 
            _texture?.Height*Scale ?? RasterFont.FourXEight.CharacterSize.Y, 
            _texture?.Width*Scale ?? 0, 
            _texture?.Height*Scale ?? RasterFont.FourXEight.CharacterSize.Y);
        return res;
    }
}