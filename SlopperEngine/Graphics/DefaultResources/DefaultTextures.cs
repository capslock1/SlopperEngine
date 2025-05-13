using SlopperEngine.Graphics.Loaders;
using SlopperEngine.Graphics.GPUResources.Textures;

namespace SlopperEngine.Graphics.DefaultResources;

/// <summary>
/// Contains several "default" textures for easy use.
/// </summary>
public static class DefaultTextures
{
    public static readonly Texture2D Error = TextureLoader.FromFilepath("defaultTextures/error.png");
}