using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Graphics;

namespace SlopperEngine.UI;

/// <summary>
/// Shows a texture on the screen.
/// </summary>
public class Image : UIElement
{
    /// <summary>
    /// The texture to show.
    /// </summary>
    public Texture? Texture
    {
        set => _material.Uniforms[_matTextureIndex].Value = value;
        get => _material.Uniforms[_matTextureIndex].Value as Texture2D;
    }

    static SlopperShader? _shader;
    static int _matTextureIndex = -1;
    Material _material;

    public Image(Box2 shape) : base(shape)
    {
        _shader ??= SlopperShader.Create("shaders/UI/Image.sesl");
        _material = Material.Create(_shader);
        if(_matTextureIndex == -1)
            _matTextureIndex = _material.GetUniformIndexFromName("mainTexture");
    }
    public Image() : this(new(0,0,1,1)){}
    public Image(Box2 shape, Texture material) : this(shape)
    {
    }

    protected override Material? GetMaterial()
    {
        return _material ?? Material.MissingMaterial;
    }
}