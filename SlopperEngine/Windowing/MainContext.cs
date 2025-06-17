using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK.Windowing.Common;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Core;
using OpenTK.Mathematics;

namespace SlopperEngine.Windowing;

/// <summary>
/// Handles the main events loop of SlopperEngine.
/// </summary>
public class MainContext : GameWindow, ISerializableFromKey<byte>
{
    /// <summary>
    /// If GL throws an error, the MainContext will shut down. This can make it significantly easier to track down GL errors, but does crash the engine.
    /// </summary>
    public static bool ThrowIfSevereGLError;

    private List<Task> toDo = new List<Task>();

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
    
    //waits for previous threads if any are still running, and cleans them up
    private void CheckExec(){
		while(toDo.Count > 0)
		{
			for(int i = 0; i < toDo.Count; i++)
			{
				if(toDo[i] == null){
					continue;
				}
				if(!toDo[i].IsCompleted)
				{
					toDo[i].Wait();
				}else{
					toDo.RemoveAt(i);
				}
			}
		}
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
		CheckExec();
        base.OnUpdateFrame(args);
        Context?.MakeCurrent();
        
        GPUResource.ClearLostResources();

        //first update inputs
        int aliveWindows = 0;
        for (int i = Window.AllWindows.Count - 1; i >= 0; i--)
        {
            var win = Window.AllWindows[i];
            if (win.KeepProgramAlive)
                aliveWindows++;
            if (win.Scene != null && !win.Scene.Destroyed)
            {
                Vector2 mousePosNorm = win.MouseState.Position;
                mousePosNorm /= win.ClientSize;
                mousePosNorm.Y = 1 - mousePosNorm.Y;
                win.Scene.InputUpdate(new(win.KeyboardState, win.MouseState, win.GetTextInputs(), mousePosNorm));
            }
            win.ClearTextInputs();
            win.NewInputFrame();
        }

        if(aliveWindows == 0)
        {
            for(int i = Window.AllWindows.Count-1; i>=0; i--)
                Window.AllWindows[i].Close();
            Close();
            return;
        }
        
        //spawns a new parallel task for every scene that needs an update, once previous frame is finished
        FrameUpdateArgs time = new FrameUpdateArgs((float)args.Time);
        List<Scene> alive = new List<Scene>();
        foreach(var sc in Scene.ActiveScenes.ToArray())
            if(!sc.Destroyed)
                alive.Add(sc);
        foreach(var sc in alive)
        {
            toDo.Add(new Task(() => {
                sc.FrameUpdate(time);
            }));
            toDo[toDo.Count-1].Start();
        }
        //Console.WriteLine(toDo.Count);
        //then, render every scene once
        Context?.MakeCurrent();
        foreach(var sc in alive)
            sc.Render(time);
        //renderMS.Stop();

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

    byte ISerializableFromKey<byte>.Serialize() => 0;
    static object? ISerializableFromKey<byte>.Deserialize(byte key) => Instance;
}
