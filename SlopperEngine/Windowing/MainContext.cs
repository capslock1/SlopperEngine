using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Common;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Core;
using OpenTK.Mathematics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;

namespace SlopperEngine.Windowing;

/// <summary>
/// Handles the main events loop of SlopperEngine.
/// </summary>
public class MainContext : GameWindow, ISerializableFromKey<byte>
{
    /// <summary>
    /// If GL throws an error, the MainContext will shut down. This can make it significantly easier to track down GL errors, but does crash the engine.
    /// </summary>
    public static volatile bool ThrowIfSevereGLError;
    
    /// <summary>
    /// Whether or not the FrameUpdate should be multithreaded. Should be deprecated when this is known not to produce errors.
    /// </summary>
    public static volatile bool MultithreadedFrameUpdate = true;

    /// <summary>
    /// The central MainContext instance.
    /// </summary>
    public static MainContext Instance{
        get
        {
            if(_instance == null) 
                InitializeInstance();
            return _instance!;
        } 
    }

    static MainContext? _instance;
    private List<Task> _frameUpdateQueue = new List<Task>();
    /// <summary>
    /// 0 when an instance is not currently being created. Used to safely have a 
    /// </summary>
    volatile static uint _instanceInCreation = 0;

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

    /// <summary>
    /// Starts SlopperEngine.
    /// </summary>
    public static void Main()
    {
        InitializeInstance();
        Core.Mods.SlopModInfo.InitializeMods();
    }

    // waits for previous threads if any are still running, and cleans them up
    private void FinishPreviousFrameExecution()
    {
        for (int i = 0; i < _frameUpdateQueue.Count; i++)
        {
            if (_frameUpdateQueue[i].IsCompleted)
                continue;

            _frameUpdateQueue[i].Wait();
        }
        _frameUpdateQueue.Clear();
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        FinishPreviousFrameExecution();
        base.OnUpdateFrame(args);
        Context?.MakeCurrent();
        
        GPUResource.ClearLostResources();

        // first update inputs
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
        
        // creates a new parallel task for every scene that needs an update, once previous frame is finished
        FrameUpdateArgs time = new ((float)args.Time);
        Scene[] alive = Scene.ActiveScenes.ToArray();

        if (MultithreadedFrameUpdate)
        {
            foreach (var sc in alive)
            {
                var task = new Task(() =>
                {
                    sc.FrameUpdate(time);
                });
                _frameUpdateQueue.Add(task);
                task.Start();
            }
        }
        else foreach (var sc in alive)
            sc.FrameUpdate(time);

        // then, render every scene once
        Context?.MakeCurrent();
        for (int i = 0; i < alive.Length; i++)
        {
            const string Message = "SlopperEngine Scene render";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, i, Message.Length, Message);
            try
            {
                alive[i].Render(time);
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Exception while rendering: {e}");
            }
            GL.PopDebugGroup();
        }

        //finally render every window once (which is a different thing!)
        for (int i = Window.AllWindows.Count - 1; i >= 0; i--)
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

    static void InitializeInstance()
    {
        uint inCreation = Interlocked.Increment(ref _instanceInCreation);
        if(inCreation > 1)
        {
            while(_instance == null || !_instance.Exists)
                Thread.Yield();
            return;
        }
        _instance = new();
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

        if ((type == DebugType.DebugTypePushGroup || type == DebugType.DebugTypePopGroup) && source == DebugSource.DebugSourceApplication)
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
