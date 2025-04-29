using System.CodeDom.Compiler;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Graphics.PostProcessing;
using SlopperEngine.SceneObjects.Rendering;
using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Graphics.Renderers;

/// <summary>
/// The simplest renderer possible - renders the scene with no regard for lighting, transparency, or other effects.
/// </summary>
public class DebugRenderer : RenderHandler
{
    public FrameBuffer Buffer {get; private set;}
    Bloom _coolBloom;
    Vector2i _screenSize = (400,300);
    Vector2i _trueScreenSize = (800,600);

    public DebugRenderer() : base()
    {
        Buffer = new(400,300);
        _coolBloom = new(new(400,300));
    }

    protected override void RenderInternal()
    {
        if(Scene == null) return;

        Buffer.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach(Camera cam in cameras)
        {
            globals.Use();
            globals.CameraProjection = cam.Projection;
            globals.CameraView = cam.GetGlobalTransform().Inverted();
            foreach(Drawcall call in Scene.GetDataContainerEnumerable<Drawcall>())
            {
                call.Material.Use(call.Model.GetMeshInfo(), this);
                globals.Model = call.Owner.GetGlobalTransform();
                call.Model.Draw();
            }
        }

        FrameBuffer.Unuse();
        _coolBloom.AddBloom(GetOutputTexture(), .45f, .25f);
    }

    public override void Resize(Vector2i newSize)
    {
        _trueScreenSize = newSize;
        _screenSize = _trueScreenSize/2;
        
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
        _coolBloom.Dispose();
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