using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics.GPUResources.Textures;

/// <summary>
/// Abstract GPU-side texture class. Handles low level functions only.
/// </summary>
public abstract class Texture : GPUResource
{
    public readonly int Handle;
    public readonly int Width;
    public readonly int Height;
    public readonly int Depth;
    public readonly SizedInternalFormat InternalFormat;

    /// <summary>
    /// The filter to use when the texture is magnified.
    /// </summary>
    public TextureMagFilter MagnificationFilter
    {
        get => _currentMagFilter;
        set
        {
            if(value == _currentMagFilter) return;
            UseAt64();
            GL.TexParameter(GetTarget(), TextureParameterName.TextureMagFilter, (int)value);
            _currentMagFilter = value;
        }
    }

    /// <summary>
    /// The filter to use when the texture is not magnified.
    /// </summary>
    public TextureMinFilter MinificationFilter
    {
        get => _currentMinFilter;
        set
        {
            if(value == _currentMinFilter) return;
            UseAt64();
            GL.TexParameter(GetTarget(), TextureParameterName.TextureMinFilter, (int)value);
            _currentMinFilter = value;
        }
    }

    /// <summary>
    /// How to sample the texture when the horizontal component of the UV is too large/negative.
    /// </summary>
    public TextureWrapMode HorizontalWrap
    {
        get => _currentHorizontalWrap;
        set
        {
            if(value == _currentHorizontalWrap) return;
            UseAt64();
            GL.TexParameter(GetTarget(), TextureParameterName.TextureWrapS, (int)value);
            _currentHorizontalWrap = value;
        }
    }
    /// <summary>
    /// How to sample the texture when the vertical component of the UV is too large/negative. 
    /// </summary>
    public TextureWrapMode VerticalWrap
    {
        get => _currentVerticalWrap;
        set
        {
            if(value == _currentVerticalWrap) return;
            UseAt64();
            GL.TexParameter(GetTarget(), TextureParameterName.TextureWrapT, (int)value);
            _currentVerticalWrap = value;
        }
    }
    /// <summary>
    /// How to sample the texture when the depth component of the UVW is too large/negative. Only applies to 3D textures.
    /// </summary>
    public TextureWrapMode DepthWrap
    {
        get => _currentDepthWrap;
        set
        {
            if(value == _currentDepthWrap) return;
            UseAt64();
            GL.TexParameter(GetTarget(), TextureParameterName.TextureWrapR, (int)value);
            _currentDepthWrap = value;
        }
    }

    TextureMagFilter _currentMagFilter;
    TextureMinFilter _currentMinFilter;
    
    TextureWrapMode _currentVerticalWrap;
    TextureWrapMode _currentHorizontalWrap;
    TextureWrapMode _currentDepthWrap;

    static readonly Texture?[] _boundTextures = new Texture[16];
    static readonly Texture?[] _boundImages = new Texture[8];
    static Texture? _usedAt64;
    
    protected Texture(int handle, int width, int height, int depth, SizedInternalFormat pixelFormat)
    {
        Handle = handle;
        Width = width;
        Height = height;
        Depth = depth;
        InternalFormat = pixelFormat;
    }

    protected abstract TextureTarget GetTarget();

    /// <summary>
    /// Uses the texture in a certain "unit", one of 16 adresses available in GLSL at a time.
    /// </summary>
    /// <param name="unit">The unit to use the texture at. Must be between TextureUnit.Texture0 and TextureUnit.Texture15 inclusive.</param>
    /// <exception cref="Exception"></exception>
    public void Use(TextureUnit unit)
    {
        int U = (int)unit - (int)TextureUnit.Texture0;
        if(U < 0 || U > 15)
            throw new Exception("Whoah there! lets not try something funny with the textureUnits. max accepted is .Texture15, sorry");
        if(_boundTextures[U] == this) return;

        GL.ActiveTexture(unit);
        _boundTextures[U] = this; 
        GL.BindTexture(GetTarget(), Handle);
    }

    /// <summary>
    /// Uses the texture as an image in a unit, allowing a program to arbitrarily read/write to it.
    /// </summary>
    /// <param name="unit">The unit to bind to. Must be between 0 and 7 inclusive.</param>
    /// <exception cref="Exception"></exception>
    public void UseAsImage(int unit)
    {
        if(unit < 0 || unit > 7) 
            throw new Exception("Whoah there! lets not try something funny with image units. max accepted is 7... sowwzy,,,");
        if(_boundImages[unit] == this) return;

        GL.BindImageTexture(unit, Handle, 0, false, 0, TextureAccess.ReadWrite, InternalFormat);
        _boundImages[unit] = this;
    }

    protected void UseAt64()
    {
        if(_usedAt64 == this) return;
        GL.ActiveTexture(TextureUnit.Texture0 + 64);
        GL.BindTexture(GetTarget(), Handle);
        _usedAt64 = this;
    }

    protected override ResourceData GetResourceData() => new TextureResourceData(){handle = this.Handle};
    protected class TextureResourceData : ResourceData
    {
        public int handle;
        public override void Clear()
        {
            GL.DeleteTexture(handle);
        }
    } 
}
