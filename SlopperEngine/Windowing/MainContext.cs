using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Windowing.Common;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Core.Serialization;
using OpenTK.Mathematics;

namespace SlopperEngine.Windowing;

/// <summary>
/// Handles the main events loop of SlopperEngine.
/// </summary>

public class MainContext : GameWindow, ISerializableFromKey<byte>
{
    /// <summary>
    /// If GL throws an error, the MainContext will shut down. This can make it significantly easier to track down GL errors, but does crash the engine.
    /// </summary
    public static bool ThrowIfSevereGLError;

    static MainContext? _instance;
    
    public static MainContext Instance{
        get => _instance == null || !_instance.Exists ? new() : _instance;
    }

	private void SceneThread(Scene[] Scenes, FrameEventArgs args)
    {
		foreach(var sc in Scenes)
			sc.FrameUpdate(new((float)args.Time));
	}
	
	public void CalcThread(Scene[] active, FrameEventArgs args)
    {
		//How many threads our CPU has
		var MaxThreads = 12;
		//How many groups of MaxThreads have been assigned a scene
		var ThreadsUsed = 0;
		//How many scenes we need to update
		Scene[] ToUpdate = new Scene[active.Length];
		var UpIndex = 0;
		foreach(var sc in active)
           if(!sc.Destroyed)
                ToUpdate[UpIndex] = sc;
                UpIndex++;
		Scene[,] ThreadAlloc = new Scene[MaxThreads, active.Length];
        while(UpIndex != 0){
			for(int i = 0; i < MaxThreads; i++){
				ThreadAlloc[i, ThreadsUsed] = ToUpdate[i];
				UpIndex--;
				if(UpIndex == 0){
					break;
				}
			}
			ThreadsUsed++;
		}
		for(int i = 0; i < ThreadAlloc.Length; i++){
			Scene[] Ret = new Scene[active.Length];
			for(int k = 0; k < active.Length; k++){
				Ret[k] = ThreadAlloc[i,k];
			}
			Thread SceneUp = new Thread(new ThreadStart(() => {SceneThread(Ret, args);}));
			SceneUp.Start();
		}
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
        
        //then, update every scene once
        Scene[] activeScenes = Scene.ActiveScenes.ToArray();
        foreach(var sc in activeScenes)
			if(!sc.Destroyed)
				sc.FrameUpdate(new((float)args.Time));
		//CalcThread(activeScenes, args);
		
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

    byte ISerializableFromKey<byte>.Serialize() => 0;
    static object? ISerializableFromKey<byte>.Deserialize(byte key) => Instance;
}
