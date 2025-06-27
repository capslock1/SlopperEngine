using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Graphics;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.UI.Display;

/// <summary>
/// Shows a texture on the screen.
/// </summary>
public sealed class ImageRectangle : DisplayElement
{
    /// <summary>
    /// The texture to show.
    /// </summary>
    public Texture? Texture
    {
        set
        {
            _material.Uniforms[_matTextureIndex].Value = value;
            _texture = value;
        }
        get => _texture;
    }
    Texture? _texture;

    /// <summary>
    /// The color to multiply the element by.
    /// </summary>
    public Color4 Color
    {
        set
        {
            _material.Uniforms[_matColorIndex].Value = (Vector4)value;
            _color = value;
        }
        get => _color;
    }
    Color4 _color = Color4.White;

    static SlopperShader? _shader;
    static int _matTextureIndex = -1;
    static int _matColorIndex = 1;
    Material _material;

    public ImageRectangle() : this(new(0, 0, 1, 1)) { }
    public ImageRectangle(Box2 shape) : this(shape, null) { }
    public ImageRectangle(Box2 shape, Texture? texture) : this(shape, texture, Color4.White) { }
#pragma warning disable CS8618
    public ImageRectangle(Box2 shape, Texture? texture, Color4 color) : base(shape)
#pragma warning restore CS8618
    {
        Init();
        Texture = texture;
        Color = color;
    }

    [OnSerialize]
    void OnSerialize(SerializedObjectTree.CustomSerializer serializer)
    {
        if (serializer.IsWriter)
        {
            Init();
            Color = _color;
            Texture = _texture;
        }
    }

    void Init()
    {
        _shader ??= SlopperShader.Create("shaders/UI/Image.sesl");
        _material = Material.Create(_shader);
        if (_matTextureIndex == -1)
        {
            _matTextureIndex = _material.GetUniformIndexFromName("mainTexture");
            _matColorIndex = _material.GetUniformIndexFromName("mainColor");
        }
    }


    protected override Material? GetMaterial()
    {
        if (_color.A <= 0) return null;
        return _material ?? Material.MissingMaterial;
    }
}