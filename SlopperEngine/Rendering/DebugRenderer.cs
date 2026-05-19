using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.PostProcessing;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.Core.Collections;
using SlopperEngine.Rendering.Passes;
using SlopperEngine.Rendering.Lighting;
using SlopperEngine.Graphics;
using SlopperEngine.Graphics.DefaultResources;

namespace SlopperEngine.Rendering;

/// <summary>
/// The simplest renderer possible - renders the scene with no regard for lighting, transparency, or other effects.
/// </summary>
public class DebugRenderer : SceneRenderer
{
    [field: DontSerialize] public FrameBuffer Buffer { get; private set; }
    [DontSerialize] LightBuffer _lights;
    [DontSerialize] Bloom _coolBloom;
    Vector2i _screenSize = (400, 300);
    Vector2i _trueScreenSize = (800, 600);

    public DebugRenderer() : base()
    {
        Buffer = new(400, 300);
        _coolBloom = new(new(400, 300));
        _lights = new();
    }

    [OnSerialize]
    void OnSerialize(OnSerializeArgs serializer)
    {
        if (serializer.IsWriter)
        {
            Buffer = new(_screenSize.X, _screenSize.Y, 1);
            _coolBloom = new(_screenSize);
            _lights = new();
        }
    }

    //this is kind of lame but it works
    public override void InputUpdate(InputUpdateArgs args) { }

    protected override void RenderInternal()
    {
        if (Scene == null) return;

        _lights.UseAndUpdateFromScene(Scene);
        
        Buffer.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        foreach (Camera cam in Scene.GetDataContainerEnumerable<Camera>().EnumerateReadonly())
        {
            globals.Use();
            globals.CameraProjection = cam.Projection;
            var camTransform = cam.GetGlobalTransform();
            globals.CameraView = camTransform.Inverted();
            globals.CameraPosition = new(camTransform.ExtractTranslation(), 1.0f);
            DrawcallDrawer drawer = new(globals, DebugPass.Instance);
            Scene.GetDataContainerEnumerable<MeshRenderer>().Enumerate(ref drawer);
        }
        FrameBuffer.Unuse();
        _coolBloom.AddBloom(GetOutputTexture(), .45f, .25f);
    }
    struct DrawcallDrawer(ShaderGlobals globals, DebugPass pass) : IRefEnumerator<MeshRenderer>
    {
        public void Next(ref MeshRenderer call)
        {
            var mesh = call.Mesh ?? DefaultMeshes.Error;
            (call.Material ?? Material.MissingMaterial).Use(mesh.GetMeshInfo(), pass);
            globals.Model = call.GetGlobalTransform();
            mesh.Draw();
        }
    }

    public override void Resize(Vector2i newSize)
    {
        _trueScreenSize = newSize;
        _screenSize = _trueScreenSize / 2;

        Buffer?.DisposeAndTextures();
        Buffer = new(_screenSize.X, _screenSize.Y);
        _coolBloom.Dispose();
        _coolBloom = new(_screenSize);
    }

    public override Vector2i GetScreenSize() => _screenSize;
    public override Texture2D GetOutputTexture() => Buffer.ColorAttachments[0];

    protected override void OnDestroyed()
    {
        Buffer.DisposeAndTextures();
        globals.Dispose();
        _lights?.Dispose();
        _coolBloom.Dispose();
    }
}
