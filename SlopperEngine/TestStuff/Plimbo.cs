using SlopperEngine.Core;
using SlopperEngine.Graphics;
using SlopperEngine.Rendering;
using OpenTK.Mathematics;
using SlopperEngine.Physics;
using SlopperEngine.Physics.Colliders;
using SlopperEngine.Graphics.Loaders;
using SlopperEngine.Graphics.GPUResources.Meshes;

namespace SlopperEngine.TestStuff;

/// <summary>
/// Plimbo.
/// </summary>
public class Plimbo : Rigidbody
{
    [OnRegister]
    void OnRegister()
    {
        var plimboShader = SlopperShader.Create("shaders/plimboShader.sesl");
        Mesh plimbo = MeshLoader.SimpleFromWavefrontOBJ("blends and models/plimbo.obj");
        var plimboTex = TextureLoader.FromFilepath("textures/plimbo.png");
        var plimboMat = Material.Create(plimboShader);
        plimboMat.Uniforms[plimboMat.GetUniformIndexFromName("effectScale")].Value = 1f;

        MeshRenderer plimb = new();
        plimb.Mesh = plimbo;
        plimb.Material = plimboMat;
        plimb.Material.Uniforms[plimb.Material.GetUniformIndexFromName("texture0")].Value = plimboTex;
        plimb.LocalScale = new Vector3(.3f,.3f,.3f);
        plimb.LocalPosition = new Vector3(0.1f,-.4f,0.25f);

        Children.Add(plimb);

        RecenterColliders = false;
        var collider = new BoxCollider(1, (.3f,.35f,.4f));
        collider.Position = (0,0,-.2f);
        Colliders.Add(collider);
        IsKinematic = true;
    }
}
