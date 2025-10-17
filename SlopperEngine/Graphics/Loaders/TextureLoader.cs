using System.IO;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Core.Collections;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Textures;
using StbImageSharp;

namespace SlopperEngine.Graphics.Loaders;

/// <summary>
/// Loads a texture into VRAM from the disk.
/// </summary>
public static class TextureLoader
{
    static Cache<string, Texture2D> _textureCache = new();
    /// <summary>
    /// Gets a 2D texture from the filepath with the desired format.
    /// </summary>
    /// <param name="filepath">The path to the texture file. NOT relative - use Core.Assets to get a full path.</param>
    /// <param name="format">The format on the GPU of the texture.</param>
    /// <returns>A new Texture2D instance, or an instance from the cache.</returns>
    public static Texture2D FromFilepath(string filepath, SizedInternalFormat format = SizedInternalFormat.Rgba8)
    {
        Texture2D? res = _textureCache.Get(filepath);
        if(res != null)
            return res;
        
        StbImage.stbi_set_flip_vertically_on_load(1);

        ImageResult image = ImageResult.FromStream(File.OpenRead(filepath), ColorComponents.RedGreenBlueAlpha);
        res = Texture2D.Create(image.Width, image.Height, format, PixelFormat.Rgba, image.Data);

        _textureCache.Set(filepath, res);
        res.OverrideOrigin = new LoadedTextureOrigin(filepath);
        
        return res;
    }

    class LoadedTextureOrigin(string filepath) : IGPUResourceOrigin
    {
        string filepath = filepath;
        public GPUResource CreateResource() => FromFilepath(filepath);
        public override string ToString() => $"Texture from filepath: '{filepath}'";
    }
}