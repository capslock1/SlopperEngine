
using System.CodeDom.Compiler;
using OpenTK.Mathematics;
using SlopperEngine.Core.SceneComponents;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.Core;
using SlopperEngine.UI;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Meshes;
using SlopperEngine.Graphics.GPUResources.Textures;


namespace SlopperEngine.Graphics.Renderers;

public class UIRenderer : RenderHandler
{
    public FrameBuffer Buffer {get; private set;} = new(400,300);

    readonly List<(Box2 shape, Material mat, Box2 scissor)> _UIElementRenderQueue = [];
    readonly ScreenspaceQuadMesh _drawMesh = new(default);
    Vector2i _screenSize = (400,300);

    public UIRenderer()
    {
        globals.CameraProjection = Matrix4.Identity;
        globals.CameraView = Matrix4.Identity;
        globals.Model = Matrix4.Identity;
    }

    protected override void RenderInternal()
    {
        if(Scene == null) return;
        Buffer.Use();
        globals.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        bool scissorActive = false;
        Box2 viewport = new(default, _screenSize);

        foreach (var uiRoot in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            uiRoot.AddRender(new(Vector2.NegativeInfinity, Vector2.PositiveInfinity), this);

        foreach (var (shape, mat, scissor) in _UIElementRenderQueue)
        {
            Box2 screenScaleScissor = new((scissor.Min + Vector2.One) * 0.5f, (scissor.Max + Vector2.One) * 0.5f);
            screenScaleScissor.Min *= _screenSize;
            screenScaleScissor.Max *= _screenSize;
            if (screenScaleScissor.Min.X <= viewport.Min.X &&
                screenScaleScissor.Max.X >= viewport.Max.X &&
                screenScaleScissor.Min.Y <= viewport.Min.Y &&
                screenScaleScissor.Max.Y >= viewport.Max.Y
            )
            {
                if (scissorActive)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    scissorActive = false;
                }
            }
            else
            {
                if (!scissorActive)
                    GL.Enable(EnableCap.ScissorTest);
                scissorActive = true;
                GL.Scissor(
                    (int)screenScaleScissor.Min.X,
                    (int)screenScaleScissor.Min.Y,
                    (int)screenScaleScissor.Size.X,
                    (int)screenScaleScissor.Size.Y);
            }

            _drawMesh.SetShape(shape);
            mat.Use(_drawMesh.GetMeshInfo(), this);
            _drawMesh.Draw();
        }
        _UIElementRenderQueue.Clear();

        GL.Enable(EnableCap.DepthTest);
        if(scissorActive)
            GL.Disable(EnableCap.ScissorTest);

        FrameBuffer.Unuse();
    }

    public override void FrameUpdate(FrameUpdateArgs args)
    {
        base.FrameUpdate(args);
        foreach(var uiRoot in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            uiRoot.UpdateShape(new(-1,-1,1,1),this);
    }

    public void AddRenderToQueue(Box2 shape, Material material, Box2 scissor) => _UIElementRenderQueue.Add((shape, material, scissor));

    public override Texture2D GetOutputTexture() => Buffer.ColorAttachments[0];
    public override Vector2i GetScreenSize() => _screenSize;
    public Vector2 GetPixelScale() => new Vector2(2,2) / _screenSize;
    public override void Resize(Vector2i newSize)
    {
        _screenSize = newSize;
        Buffer?.DisposeAndTextures();
        Buffer = new(_screenSize.X, _screenSize.Y);
    }

    protected override void OnDestroyed()
    {
        Buffer.DisposeAndTextures();
        globals.Dispose();
    }
    
    public override void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        writer.Write(
@"void main()
{
    vertIn_Initialize();
    vertex();
    gl_Position = vertOut.position;
}"
        );
    }
    public override void AddFragmentMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        bool writesAlbedo = false;
        bool writesAlpha = false;
        foreach(var v in scope.pixOut)
        {
            if(v.Name == "Albedo")
                writesAlbedo = true;
            if(v.Name == "Transparency")
                writesAlpha = true;
        }
        writer.Write(
@$"
out vec4 SL_FragColor;
void main()
{{
    pixel();
    SL_FragColor = vec4({(writesAlbedo ? "pixOut.Albedo" : "1.0,1.0,1.0")},{(writesAlpha ? "pixOut.Transparency" : "1.0")});
}}"
        );
    }
}