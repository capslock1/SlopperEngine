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
        MeshRenderer plimb = new();
        plimb.LocalScale = new Vector3(.3f,.3f,.3f);
        plimb.LocalPosition = new Vector3(0.1f,-.4f,0.25f);

        Children.Add(plimb);

        var collider = new BoxCollider(1, (.3f,.35f,.4f));
        collider.Position = (0,0,-0.2f);
        Colliders.Add(collider);
        RecenterColliders = false;
        IsKinematic = true;

        Scene!.SceneRenderer!.OnPreRender += () =>
        {
            var plimboShader = SlopperShader.Create("shaders/plimboShader.sesl");
            Mesh plimbo = MeshLoader.SimpleFromWavefrontOBJ("blends and models/plimbo.obj");
            var plimboTex = TextureLoader.FromFilepath("textures/plimbo.png");
            var plimboMat = Material.Create(plimboShader);

            plimboMat.Uniforms[plimboMat.GetUniformIndexFromName("effectScale")].Value = 1f;
            plimboMat.Uniforms[plimboMat.GetUniformIndexFromName("texture0")].Value = plimboTex;

            plimb.Material = plimboMat;
            plimb.Mesh = plimbo;
        };
    }
}
