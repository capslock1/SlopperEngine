using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources.Textures;
using StbImageSharp;

namespace SlopperEngine.Graphics.CPUResources;

/// <summary>
/// A texture on CPU's memory, with 8 bit RGBA color.
/// </summary>
public class RGBA8Texture
{
    /// <summary>
    /// The texture on VRAM.
    /// </summary>
    public Texture2D? GPUTexture { get; private set; }

    /// <summary>
    /// The width in pixels.
    /// </summary>
    public readonly int Width;

    /// <summary>
    /// The height in pixels.
    /// </summary>
    public readonly int Height;

    readonly byte[] _pixels;
    bool _gpuUpToDate;

    /// <summary>
    /// Creates a new RGBA8Texture from width and height. Is initialized to transparent black.
    /// </summary>
    public RGBA8Texture(int width, int height) : this(width, height, new byte[width * height * 4]) { }

    RGBA8Texture(int width, int height, byte[] pixels)
    {
        _pixels = pixels;
        Width = width;
        Height = height;

        if (pixels.Length != width * height * 4)
            Console.WriteLine($"RGBA8Texture received {width}*{height} as dimensions, but only received enough bytes for {pixels.Length / 4} pixels.");
    }

    /// <summary>
    /// Loads a new RGBA8Texture from the disk.
    /// </summary>
    /// <param name="filepath">The full filepath to the texture.</param>
    /// <returns>A new RGBA8Texture. This is not cached.</returns>
    public static RGBA8Texture Load(string filepath)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);

        ImageResult image = ImageResult.FromStream(File.OpenRead(filepath), ColorComponents.RedGreenBlueAlpha);
        var res = new RGBA8Texture(image.Width, image.Height, image.Data);

        return res;
    }

    /// <summary>
    /// Sets the GPUTexture to be up to date to the CPU side texture.
    /// </summary>
    public void UpdateGPU()
    {
        if (_gpuUpToDate)
            return;

        _gpuUpToDate = true;

        if (GPUTexture == null)
        {
            GPUTexture = Texture2D.Create(
                Width,
                Height,
                SizedInternalFormat.Rgba8,
                PixelFormat.Rgba,
                _pixels);
            return;
        }

        GL.TextureSubImage2D(GPUTexture.Handle, 0, 0, 0, Width, Height, PixelFormat.Rgba, PixelType.UnsignedByte, _pixels);
    }

    /// <summary>
    /// Tries to set a certain pixel to a certain color.
    /// </summary>
    /// <param name="position">The pixel to change.</param>
    /// <param name="color">The color to set the color as.</param>
    /// <returns>Whether or not the pixel was in bounds (and could be written).</returns>
    public bool TrySetPixel(Vector2i position, Color4 color)
    {
        if ((uint)position.X > Width) return false;
        if ((uint)position.Y > Height) return false;

        _gpuUpToDate = false;
        var col = (Vector4)color;
        col *= 255;
        int pos = (position.X + Width * position.Y) * 4;
        _pixels[pos] = ToByte(col.X);
        _pixels[pos + 1] = ToByte(col.Y);
        _pixels[pos + 2] = ToByte(col.Z);
        _pixels[pos + 3] = ToByte(col.W);

        byte ToByte(float val)
        {
            return (val < 0) ? (byte)0 : (val >= 255) ? (byte)255 : (byte)val;
        }

        return true;
    }

    /// <summary>
    /// Tries to read a pixel on the texture.
    /// </summary>
    /// <param name="position">The pixel to read.</param>
    /// <param name="color">The color at the point.</param>
    /// <returns>Whether or not the pixel was in bounds (and could be read).</returns>
    public bool TryGetPixel(Vector2i position, out Color4 color)
    {
        color = default;

        if ((uint)position.X > Width) return false;
        if ((uint)position.Y > Height) return false;

        int pos = (position.X + Width * position.Y) * 4;
        color = new(_pixels[pos], _pixels[pos + 1], _pixels[pos + 2], _pixels[pos + 3]);

        return true;
    }
}