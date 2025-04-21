using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Common;
using SlopperEngine.SceneObjects;
using SlopperEngine.Rendering;

namespace SlopperEngine.Windowing;

/// <summary>
/// Handles the main events loop of SlopperEngine.
/// </summary>
public class MainContext : GameWindow
{
    /// <summary>
    /// If GL throws an error, the MainContext will shut down. This can make it significantly easier to track down GL errors, but does crash the engine.
    /// </summary>
    public static bool ThrowIfSevereGLError;

    static MainContext? _instance;
    
    public static MainContext Instance{
        get => _instance == null || !_instance.Exists ? new() : _instance;
    }

    MainContext() : base(
        new(){
            Win32SuspendTimerOnDrag = false,
        }, 
        new(){
            APIVersion = new(4,6),
            StartVisible = false,
            ClientSize = (1,1),
        })
    {
        if(_instance == null) _instance = this;
        else throw new Exception("Attempted to make a second MainContext.");
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        Context?.MakeCurrent();
        
        GPUResource.ClearLostResources();

        //first update inputs
        int aliveWindows = 0;
        for(int i = Window.AllWindows.Count-1; i>=0; i--)
        {
            var win = Window.AllWindows[i];
            if(win.KeepProgramAlive) 
                aliveWindows++;
            if(win.Scene != null && !win.Scene.Destroyed) 
                win.Scene.InputUpdate(new(win.KeyboardState, win.MouseState));
            win.NewInputFrame();
        }

        if(aliveWindows == 0)
        {
            for(int i = Window.AllWindows.Count-1; i>=0; i--)
                Window.AllWindows[i].Close();
            Close();
            return;
        }
        
        Scene[] activeScenes = Scene.ActiveScenes.ToArray();

        //then, update every scene once
        foreach(var sc in activeScenes)
            if(!sc.Destroyed)
                sc.FrameUpdate(new((float)args.Time));

        //then, render every scene once
        Context?.MakeCurrent();
        foreach(var sc in activeScenes)
            if(!sc.Destroyed)
                sc.Render();

        //finally render every window once (which is a different thing!)
        for(int i = Window.AllWindows.Count-1; i>=0; i--)
            Window.AllWindows[i].Render();
    }

    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DebugOutput);
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        base.OnLoad();
        if(ThrowIfSevereGLError)
            GL.Enable(EnableCap.DebugOutputSynchronous); //lower performance but better debugging.
    }

    private static DebugProc? DebugMessageDelegate = OnDebugMessage;
    private static void OnDebugMessage(
        DebugSource source,
        DebugType type,
        int id,
        DebugSeverity severity,
        int length,
        IntPtr pMessage,
        IntPtr pUserParam)
    {
        if(type == DebugType.DebugTypeOther)
        return;
        string message = Marshal.PtrToStringAnsi(pMessage, length);

        Console.WriteLine(
@$"Debug message from OpenGL: {severity} 
source: {source} 
type: {type} 
id: {id} 
{message}");

        //this turns off the engine - id rather keep going with an error if possible (perhaps debug it)?
        if (type == DebugType.DebugTypeError && ThrowIfSevereGLError)
        {
            throw new Exception(message);
        }
    }
}