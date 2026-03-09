using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Graphics;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.UI.Display;

/// <summary>
/// Shows a color on the screen.
/// </summary>
public sealed class ColorRectangle : DisplayElement
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
    Color4 _color;

    static SlopperShader? _shader;
    static int _matTextureIndex = -1;

    Material _material;

    public ColorRectangle(Box2 localShape, Color4 color) : base(localShape)
    {
        Init();
        _color = color;
        _material!.Uniforms[_matTextureIndex].Value = (Vector4)color;
    }
    public ColorRectangle() : this(new(0,0,1,1), Color4.White){}


    [OnSerialize]
    void OnSerialize(OnSerializeArgs serializer)
    {
        if (serializer.IsWriter)
        {
            Init();
            Color = _color;
        }
    }

    void Init()
    {
        _shader ??= SlopperShader.Create(Asset.GetEngineAsset("shaders/UI/Color.sesl"));
        _material = Material.Create(_shader);
        if (_matTextureIndex == -1)
            _matTextureIndex = _material.GetUniformIndexFromName("mainColor");
    }

    protected override Material? GetMaterial()
    {
        if (_color.A <= 0) return null; // dont render if alpha is zero. waste of resources
        return _material ?? Material.MissingMaterial;
    }
}