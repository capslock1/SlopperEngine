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
    public bool UseInfiniteShadowMap = false;
    [DontSerialize] FrameBuffer _shadowBuffer;
    [DontSerialize] LightBuffer _lights;
    [DontSerialize] Bloom _coolBloom;
    Vector2i _screenSize = (400, 300);
    Vector2i _trueScreenSize = (800, 600);

    public DebugRenderer() : base()
    {
        Buffer = new(400, 300);
        _shadowBuffer = FrameBuffer.CreateShadowBuffer(DirectionalLight.ShadowResolutionPixels, DirectionalLight.ShadowResolutionPixels);
        _coolBloom = new(new(400, 300));
        _lights = new();
    }

    [OnSerialize]
    void OnSerialize(OnSerializeArgs serializer)
    {
        if (serializer.IsWriter)
        {
            Buffer = new(_screenSize.X, _screenSize.Y, 1);
            _shadowBuffer = FrameBuffer.CreateShadowBuffer(DirectionalLight.ShadowResolutionPixels, DirectionalLight.ShadowResolutionPixels);
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
        FrameBuffer.Unuse();
        foreach (Camera cam in Scene.GetDataContainerEnumerable<Camera>().EnumerateReadonly())
        {
            using var _ = DebugGroup.StartUsing("Camera");

            var camTransform = cam.GetGlobalTransform();
            Vector3 camPos = camTransform.ExtractTranslation();
            RenderShadowCasters(camPos);
            
            Buffer.Use();
            globals.Use();
            globals.CameraProjection = cam.Projection;
            globals.CameraView = camTransform.Inverted();
            globals.CameraPosition = new(camPos, 1.0f);
            DrawcallDrawer drawer = new(globals, UseInfiniteShadowMap ? DebugPass.InstanceInfiniteShadow : DebugPass.Instance);
            using(DebugGroup.StartUsing("Draw objects"))
                Scene.GetDataContainerEnumerable<MeshRenderer>().Enumerate(ref drawer);
            FrameBuffer.Unuse();
        }

        using(DebugGroup.StartUsing("Bloom pass"))
            _coolBloom.AddBloom(GetOutputTexture(), .45f, .25f); // bloom pass should be per camera ideally, but is not programmed to right now.
    }
    struct DrawcallDrawer(ShaderGlobals globals, RenderPass pass) : IRefEnumerator<MeshRenderer>
    {
        public void Next(ref MeshRenderer call)
        {
            var mesh = call.Mesh ?? DefaultMeshes.Error;
            (call.Material ?? Material.MissingMaterial).Use(mesh.GetMeshInfo(), pass);
            globals.Model = call.GetGlobalTransform();
            mesh.DrawInstanced(int.Max(1,call.InstanceCount));
        }
    }

    void RenderShadowCasters(Vector3 cameraPosition)
    {
        using var _ = DebugGroup.StartUsing("Render shadow casters");
        foreach (DirectionalLight light in Scene!.GetDataContainerEnumerable<DirectionalLight>().EnumerateReadonly())
        {
            if(!light.CastsShadows) continue;

            using var _2 = DebugGroup.StartUsing("Shadow caster");
            
            var cascades = light.Cascades ?? DirectionalLight.DefaultCascades;
            for(int casc = 0; casc < int.Min(cascades.Length, LightBuffer.MaxShadowCascades); casc++)
            {
                using var _3 = DebugGroup.StartUsing("Cascade");

                float size = cascades.Span[casc];
                
                _shadowBuffer.Use();
                GL.Clear(ClearBufferMask.DepthBufferBit);

                globals.Use();
                globals.CameraProjection = Matrix4.CreateOrthographic(size, size, -light.PlaneDistance, light.PlaneDistance);
                var lightTransform = light.GetGlobalTransform();
                lightTransform.Row3.Xyz = cameraPosition;
                globals.CameraView = lightTransform.Inverted();
                globals.CameraPosition = new(lightTransform.ExtractTranslation(), 1.0f);
                DrawcallDrawer drawer = new(globals, UseInfiniteShadowMap ? ShadowPass.InstanceInfinite : ShadowPass.Instance);
                Scene.GetDataContainerEnumerable<MeshRenderer>().Enumerate(ref drawer);
                
                FrameBuffer.Unuse();
                _lights.UpdateCascadeViewProjAndTexture(light, _shadowBuffer.ColorAttachments[0], casc, globals.CameraView * globals.CameraProjection);
            }
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
        _shadowBuffer.Dispose();
    }
}
