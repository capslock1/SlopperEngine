namespace SlopperEngine.Rendering;

/// <summary>
/// Contains several "default" meshes for easy use.
/// </summary>
public static class DefaultMeshes
{
    public static readonly Mesh Cube = MeshLoader.SimpleFromWavefrontOBJ("defaultModels/cube.obj");
    public static readonly Mesh Plane = MeshLoader.SimpleFromWavefrontOBJ("defaultModels/plane.obj");
    public static readonly Mesh Sphere = MeshLoader.SimpleFromWavefrontOBJ("defaultModels/sphere.obj");
    public static readonly Mesh Error = MeshLoader.SimpleFromWavefrontOBJ("defaultModels/error.obj");

}