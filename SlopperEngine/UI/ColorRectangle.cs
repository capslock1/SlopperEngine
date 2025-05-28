using OpenTK.Mathematics;
using SlopperEngine.Graphics;

namespace SlopperEngine.UI;

/// <summary>
/// Shows a color on the screen.
/// </summary>
public class ColorRectangle : UIElement
{
    /// <summary>
    /// The color of this element.
    /// </summary>
    public Color4 Color
    {
        get => _color;
        set
        {
            _color = value;
            _material.Uniforms[_matTextureIndex].Value = (Vector4)value;
        }
    }

    static SlopperShader? _shader;
    static int _matTextureIndex = -1;

    Material _material;
    Color4 _color;

    public ColorRectangle(Box2 localShape, Color4 color) : base(localShape)
    {
        _shader ??= SlopperShader.Create("shaders/UI/Color.sesl");
        _material = Material.Create(_shader);
        if (_matTextureIndex == -1)
            _matTextureIndex = _material.GetUniformIndexFromName("mainColor");

        _color = color;
        _material.Uniforms[_matTextureIndex].Value = (Vector4)color;
    }
    public ColorRectangle() : this(new(0,0,1,1), Color4.White){}

    protected override Material? GetMaterial()
    {
        return _material ?? Material.MissingMaterial;
    }
}