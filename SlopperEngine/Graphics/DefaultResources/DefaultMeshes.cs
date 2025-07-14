using SlopperEngine.Core;
using SlopperEngine.Graphics.GPUResources.Meshes;
using SlopperEngine.Graphics.Loaders;

namespace SlopperEngine.Graphics.DefaultResources;

/// <summary>
/// Contains several "default" meshes for easy use.
/// </summary>
public static class DefaultMeshes
{
    public static readonly Mesh Cube = MeshLoader.SimpleFromWavefrontOBJ(Assets.GetPath("defaultModels/cube.obj", "EngineAssets"));
    public static readonly Mesh Plane = MeshLoader.SimpleFromWavefrontOBJ(Assets.GetPath("defaultModels/plane.obj", "EngineAssets"));
    public static readonly Mesh Sphere = MeshLoader.SimpleFromWavefrontOBJ(Assets.GetPath("defaultModels/sphere.obj", "EngineAssets"));
    public static readonly Mesh Error = MeshLoader.SimpleFromWavefrontOBJ(Assets.GetPath("defaultModels/error.obj", "EngineAssets"));

}