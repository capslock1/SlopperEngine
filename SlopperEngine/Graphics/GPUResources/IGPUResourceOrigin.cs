namespace SlopperEngine.Graphics.GPUResources;

/// <summary>
/// An interface for serializing GPUResources. Should be able to approximately reconstruct a resource.
/// </summary>
public interface IGPUResourceOrigin
{
    /// <summary>
    /// Recreates the original resource.
    /// </summary>
    public GPUResource CreateResource();
}