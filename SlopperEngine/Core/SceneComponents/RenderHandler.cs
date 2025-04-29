using SlopperEngine.Graphics;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.ShadingLanguage;
using System.CodeDom.Compiler;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Core.SceneComponents;

/// <summary>
/// Abstract base class for all renderers.
/// </summary>
public abstract class RenderHandler : SceneComponent
{
    /// <summary>
    /// The background color of the rendered frame, if used by inheriting classes.
    /// </summary>
    public Color4 ClearColor = new(.1f,.2f,.35f,1);

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
    public override void FrameUpdate(FrameUpdateArgs args)
    {
        globals.Time += args.DeltaTime;
    }

    public void Render()
    {
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