using SlopperEngine.Graphics;
using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.SceneObjects.Rendering;
using System.CodeDom.Compiler;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources.Textures;
using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Core.SceneComponents;

/// <summary>
/// Abstract base class for all renderers.
/// </summary>
public abstract class RenderHandler : SceneRenderer
{
    /// <summary>
    /// The background color of the rendered frame, if used by inheriting classes.
    /// </summary>
    public Color4 ClearColor = new(.1f,.2f,.35f,1);
    float _time;

    protected List<Camera> cameras = new();
    protected ShaderGlobals globals;

    public RenderHandler()
    {
        globals = new();
    }

    public void AddCamera(Camera cam)
    {
        cameras.Add(cam);
    }
    public void RemoveCamera(Camera cam)
    {
        cameras.Remove(cam);
    }

    public override void InputUpdate(InputUpdateArgs input){}

    public override void Render(FrameUpdateArgs args)
    {
		_time += args.DeltaTime;
		globals.Time = _time;
        GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
        RenderInternal();
    }
    protected abstract void RenderInternal();
    public abstract void Resize(Vector2i newSize);
    public abstract Vector2i GetScreenSize();
    public abstract Texture2D GetOutputTexture();

    public abstract void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer);
    public abstract void AddFragmentMain(SyntaxTree scope, IndentedTextWriter writer);
}
