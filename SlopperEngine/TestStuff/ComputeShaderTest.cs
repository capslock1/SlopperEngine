using SlopperEngine.Core;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics;
using SlopperEngine.SceneObjects.Rendering;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Graphics.GPUResources.Shaders;
using SlopperEngine.Graphics.DefaultResources;
using OpenTK.Mathematics;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.TestStuff;

/// <summary>
/// Testing compute shader functionality. 
/// </summary>
public class ComputeShaderTest : SceneObject
{
    [DontSerialize] SingleChild<MeshRenderer> _texQuad;
    public ComputeShaderTest()
    {
        _texQuad = new(this);
    }

    //how do we cope with wanting to use materials for compute shaders? are we doing sesl compute shaders now?
    //for now i suppose im leaving that out, as with the rest of the general sesl rework
    //diy handling compute shaders for now is a-okay

    [OnRegister]
    void Register()
    {
        if(_texQuad.Value != null) return;

        var computeGenTex = Texture2D.Create(1024,1024, SizedInternalFormat.Rgba8);
        var quadshader = SlopperShader.Create("shaders/unlitTextured.sesl");
        var quadMat = Material.Create(quadshader);
        quadMat.Uniforms[quadMat.GetUniformIndexFromName("mainTexture")].Value = computeGenTex;
        
        string computeGenSource = File.ReadAllText(Assets.GetPath("shaders/ComputeTest.compute"));
        using(ComputeShader shad = ComputeShader.Create(computeGenSource))
        {
            //var computeMat = Material.Create(shad);
            //computeMat.uniforms[computeMat.GetUniformIndexFromName("mainTexture")].value = computeGenTex;
            //computeMat.Use();
            shad.Use();
            shad.SetUniform(shad.GetUniformLocation("mainTexture"), 1);
            computeGenTex.UseAsImage(1);
            shad.Dispatch(32,32,1);
        }
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        MeshRenderer texQuad = new()
        {
            LocalPosition = new Vector3(-3,0,0),
            Material = quadMat,
            Mesh = DefaultMeshes.Plane,
        };
        _texQuad.Value = texQuad;
    }
}