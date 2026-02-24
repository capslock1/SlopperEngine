using System.IO;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Core;
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
    /// Gets a 2D texture from the asset with the desired format.
    /// </summary>
    /// <param name="asset">The asset to the texture file.</param>
    /// <param name="format">The format on the GPU of the texture.</param>
    /// <returns>A new Texture2D instance, or an instance from the cache.</returns>
    public static Texture2D FromAsset(Asset asset, SizedInternalFormat format = SizedInternalFormat.Rgba8)
    {
        if(!asset.AssetExists)
            throw new System.Exception($"TextureLoader: Asset {asset} is missing.");

        Texture2D? res = _textureCache.Get(asset.FullFilePath!);
        if(res != null)
            return res;
        
        StbImage.stbi_set_flip_vertically_on_load(1);

        ImageResult image = ImageResult.FromStream(asset.GetStream(), ColorComponents.RedGreenBlueAlpha);
        res = Texture2D.Create(image.Width, image.Height, format, PixelFormat.Rgba, image.Data);

        _textureCache.Set(asset.FullFilePath!, res);
        res.OverrideOrigin = new LoadedTextureOrigin(asset);
        
        return res;
    }

    class LoadedTextureOrigin(Asset file) : IGPUResourceOrigin
    {
        public GPUResource CreateResource() => FromAsset(file);
        public override string ToString() => $"Texture from file: '{file}'";
    }
}