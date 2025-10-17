using SlopperEngine.Graphics;
using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.SceneObjects;
using System.CodeDom.Compiler;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources.Textures;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Core;
using System;
using System.Collections.Generic;

namespace SlopperEngine.Rendering;

/// <summary>
/// Special SceneObject for renderers - this allows for multithreading by handling them seperately
/// </summary>
public abstract class SceneRenderer : SceneObject
{
    /// <summary>
    /// The background color of the rendered frame, if used by inheriting classes.
    /// </summary>
    public Color4 ClearColor = new(.1f,.2f,.35f,1);
    /// <summary>
    /// Gets called ONCE on the render thread.
    /// </summary>
    public event Action? OnPreRender;

    float _time;

    protected List<Camera> cameras = new();
    protected ShaderGlobals globals;

    public SceneRenderer()
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

	/// <summary>
    /// Updates user input, if applicable
    /// </summary>
    public abstract void InputUpdate(InputUpdateArgs input);

	/// <summary>
    /// Renders the scene
    /// </summary>
    public void Render(FrameUpdateArgs args)
    {
		_time += args.DeltaTime;
		globals.Time = _time;
        OnPreRender?.Invoke();
        OnPreRender = null;
        GL.ClearColor(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
        RenderInternal();
    }
    protected abstract void RenderInternal();
    public abstract void Resize(Vector2i newSize);
    public abstract Vector2i GetScreenSize();
    public abstract Texture2D GetOutputTexture();

    public abstract void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer);
    public abstract void AddFragmentMain(SyntaxTree scope, IndentedTextWriter writer);
    
    [OnRegister] void Register() => Scene!.CheckCachedComponents();
    [OnUnregister] void Unregister(Scene scene) => scene.CheckCachedComponents();
}
