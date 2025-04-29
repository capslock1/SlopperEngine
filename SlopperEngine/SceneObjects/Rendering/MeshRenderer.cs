using SlopperEngine.Graphics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;

namespace SlopperEngine.SceneObjects.Rendering;

/// <summary>
/// Renders a mesh using a material.
/// </summary>
public class MeshRenderer : SceneObject3D
{
    SceneDataHandle _drawcallIndex;

    Mesh? _mesh;
    public Mesh? Mesh{
        get{return _mesh;}
        set{
            _mesh = value;
            if(InScene && _drawcallIndex.IsRegistered)
            {
                RemoveDrawcall(Scene);
                CreateDrawcall();
            }
        }
    }

    Material? _mat;
    public Material? Material {
        get{return _mat;} 
        set{
            _mat = value;
            if(InScene && _drawcallIndex.IsRegistered)
            {
                RemoveDrawcall(Scene);
                CreateDrawcall();
            }
        }
    }


    [OnRegister]
    void CreateDrawcall()
    {
        Drawcall res = new(this, DefaultMeshes.Error, Material.MissingMaterial);

        if(_mesh != null)
            res.Model = _mesh;
        
        if(_mat != null) 
            res.Material = _mat;

        _drawcallIndex = Scene!.RegisterSceneData(res);
    }

    [OnUnregister]
    void RemoveDrawcall(Scene? scene)
    {
        scene?.UnregisterSceneData<Drawcall>(_drawcallIndex, new());
        _drawcallIndex = default;
    }
}
