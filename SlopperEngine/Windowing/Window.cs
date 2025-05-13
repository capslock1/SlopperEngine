using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Textures;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using SlopperEngine.SceneObjects;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SlopperEngine.Windowing;

/// <summary>
/// Represents a window from the operating system.
/// </summary>
public class Window : NativeWindow
{
    static List<Window> _allWindows = new();
    public static ReadOnlyCollection<Window> AllWindows => _allWindows.AsReadOnly();
    public Texture2D? WindowTexture;
    public Scene? Scene;
    public bool KeepProgramAlive = true;

    //Considering shared resources are a PAIN IN THE buttock,
    //windows only have one responsibility - draw a texture to the screen
    //to that end, fullscreentriangle specifically can handle several windows
    //everything else should stay locked to the MainContext

    /// <summary>
    /// Should only be called by MainContext. Updates the window's image.
    /// </summary>
    public void Render()
    {
        if(WindowTexture == null)
            return;
        if(Context == null)
            return;

        Context.MakeCurrent();
        
        FrameBuffer.Unuse();
        GL.Viewport(0,0,ClientSize.X, ClientSize.Y);
        FullScreenTriangle.Draw(WindowTexture);
        
        Context.SwapBuffers();
    }

    Window(NativeWindowSettings settings) : base(settings)
    {
        _allWindows.Add(this);
        MainContext.Instance.MakeCurrent();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if(disposing)
            _allWindows.Remove(this);
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if(!e.Cancel) 
            Dispose();
    }

    /// <summary>
    /// Creates a new window.
    /// </summary>
    public static Window Create()
    {
        return new Window(new()
        {
            SharedContext = MainContext.Instance.Context,
            APIVersion = new(4,6),
        });
    }
    /// <summary>
    /// Creates a new window of a specific size.
    /// </summary>
    /// <param name="Size">The size of the window.</param>
    /// <param name="Position">The position to start it at.</param>
    /// <param name="Title">The title of the window.</param>
    /// <param name="WindowState">The state in which the window should start.</param>
    /// <param name="StartVisible">Whether or not the window is visible when it starts.</param>
    /// <param name="TransparentFrameBuffer">Whether or not the window can be transparent.</param>
    /// <param name="Icon">The icon of the window.</param>
    public static Window Create(Vector2i Size, Vector2i? Position = null, string Title = "SlopperEngine", WindowState WindowState = WindowState.Normal, bool StartVisible = true, bool TransparentFrameBuffer = false, WindowIcon? Icon = null, WindowBorder Border = default)
    {
        return new Window(new(){
            SharedContext = MainContext.Instance.Context,
            APIVersion = new(4,6),
            ClientSize = Size,
            Location = Position,
            Title = Title,
            WindowState = WindowState,
            StartVisible = StartVisible,
            TransparentFramebuffer = TransparentFrameBuffer,
            Icon = Icon,
            WindowBorder = Border,
        });
    }
}