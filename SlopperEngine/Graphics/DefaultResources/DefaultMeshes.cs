using SlopperEngine.Core;
using SlopperEngine.Graphics.GPUResources.Meshes;
using SlopperEngine.Graphics.Loaders;

namespace SlopperEngine.Graphics.DefaultResources;

/// <summary>
/// Contains several "default" meshes for easy use.
/// </summary>
public static class DefaultMeshes
{
    /// <summary>
    /// I forgot what the UVs look like, but this sure is a cube.
    /// </summary>
    public static readonly Mesh Cube;
    /// <summary>
    /// Plane of 2x2 units, centered on the origin.
    /// </summary>
    public static readonly Mesh Plane;
    /// <summary>
    /// Unit sphere with equirectangular UVs.
    /// </summary>
    public static readonly Mesh Sphere;
    /// <summary>
    /// Error model with the messiest UVs you've seen in your life.
    /// </summary>
    public static readonly Mesh Error;

    static DefaultMeshes()
    {
        Asset.TryGetEngineAsset("defaultModels/cube.obj", out Asset? asset);
        Cube = MeshLoader.SimpleFromWavefrontOBJ(asset!.Value);
        Asset.TryGetEngineAsset("defaultModels/plane.obj", out asset);
        Plane = MeshLoader.SimpleFromWavefrontOBJ(asset!.Value);
        Asset.TryGetEngineAsset("defaultModels/sphere.obj", out asset);
        Sphere = MeshLoader.SimpleFromWavefrontOBJ(asset!.Value);
        Asset.TryGetEngineAsset("defaultModels/error.obj", out asset);
        Error = MeshLoader.SimpleFromWavefrontOBJ(asset!.Value);
    }
}