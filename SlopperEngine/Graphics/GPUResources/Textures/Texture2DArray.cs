using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics.GPUResources.Textures;

/// <summary>
/// A 2D texture for use on the GPU.
/// </summary>
public class Texture2DArray : Texture
{
    protected Texture2DArray(int handle, int width, int height, int layers, SizedInternalFormat format) : base(handle, width, height, layers, format){}

    /// <summary>
    /// Creates a 2D texture on the GPU.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="internalFormat">The format of pixels on the GPU. Directly correlates to VRAM usage.</param>
    /// <param name="sentFormat">The format in which pixels are sent to the GPU. I don't exactly know what this is for.</param>
    /// <param name="data">The data to initialize the texture with. Defaults to a black texture.</param>
    /// <returns>A new Texture2D instance.</returns>
    public static Texture2DArray Create(int width, int height, int layers,
        SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8,
        PixelFormat sentFormat = PixelFormat.Rgba,
        byte[]? data = null,
        TextureMagFilter magFilter = TextureMagFilter.Nearest,
        TextureMinFilter minFilter = TextureMinFilter.Nearest,
        TextureWrapMode wrapMode = TextureWrapMode.Repeat)
    {
        int handle = GL.GenTexture();
        var res = new Texture2DArray(handle, width, height, layers, internalFormat);

        res.MagnificationFilter = magFilter;
        res.MinificationFilter = minFilter;

        res.HorizontalWrap = wrapMode;
        res.VerticalWrap = wrapMode;
        res.DepthWrap = wrapMode;

        GL.TexImage3D(
            TextureTarget.Texture2DArray, 
            0, 
            (PixelInternalFormat)internalFormat, 
            width, 
            height, 
            layers, 0,
            sentFormat, 
            PixelType.UnsignedByte, 
            data);
        return res;
    }

    protected override IGPUResourceOrigin GetOrigin() => new Tex2DArrOrigin(Width, Height, Depth, InternalFormat, MagnificationFilter, MinificationFilter, HorizontalWrap, VerticalWrap);
    protected class Tex2DArrOrigin(int width, int height, int layers, SizedInternalFormat format, TextureMagFilter magFilter, TextureMinFilter minFilter, TextureWrapMode wrapModeH, TextureWrapMode wrapModeV) : IGPUResourceOrigin
    {
        int width = width;
        int height = height;
        int layers = layers;
        SizedInternalFormat format = format;
        TextureMagFilter magFilter = magFilter;
        TextureMinFilter minFilter = minFilter;
        TextureWrapMode wrapModeV = wrapModeV;
        TextureWrapMode wrapModeH = wrapModeH;
        public GPUResource CreateResource() 
        {
            var res = Create(width, height, layers, format, magFilter:magFilter, minFilter:minFilter);
            res.HorizontalWrap = wrapModeH;
            res.VerticalWrap = wrapModeV;
            return res;
        }
        public override string ToString() => $"Procedural Texture2DArray (w{width},h{height},d{layers})";
    }

    protected override TextureTarget GetTarget() => TextureTarget.Texture2DArray;
}