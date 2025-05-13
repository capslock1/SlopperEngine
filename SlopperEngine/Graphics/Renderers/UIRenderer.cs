
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

    readonly List<(Box2 shape, Material mat)> _UIElementRenderQueue = [];
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
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        foreach(var uiRoot in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            uiRoot.AddRender(this);

        foreach(var (shape, mat) in _UIElementRenderQueue)
        {
            _drawMesh.SetShape(shape);
            mat.Use(_drawMesh.GetMeshInfo(), this);
            _drawMesh.Draw();
        }
        _UIElementRenderQueue.Clear();

        GL.Enable(EnableCap.DepthTest);

        FrameBuffer.Unuse();
    }

    public override void FrameUpdate(FrameUpdateArgs args)
    {
        base.FrameUpdate(args);
        foreach(var uiRoot in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            uiRoot.UpdateShape(new(-1,-1,1,1),this);
    }

    public void AddRenderToQueue(Box2 shape, Material material) => _UIElementRenderQueue.Add((shape, material));

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