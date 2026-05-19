using SlopperEngine.Graphics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Graphics.GPUResources.Meshes;
using SlopperEngine.Graphics.DefaultResources;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Rendering;

/// <summary>
/// Renders a mesh using a material.
/// </summary>
public class MeshRenderer : SceneObject3D
{
    [DontSerialize] SceneDataHandle _drawcallIndex;

    /// <summary>
    /// The mesh to render.
    /// </summary>
    public Mesh? Mesh;

    /// <summary>
    /// The material to render the mesh with.
    /// </summary>
    public Material? Material;


    [OnRegister]
    void CreateDrawcall()
    {
        _drawcallIndex = Scene!.RegisterSceneData(this);
    }

    [OnUnregister]
    void RemoveDrawcall(Scene? scene)
    {
        scene?.UnregisterSceneData<MeshRenderer>(_drawcallIndex, this);
        _drawcallIndex = default;
    }
}
