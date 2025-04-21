using System.Reflection;
using SlopperEngine.Engine.SceneData;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Engine;

/// <summary>
/// Makes a method get called every frame, before the scene gets rendered.
/// </summary>
public unsafe sealed class OnFrameUpdateAttribute : EngineMethodAttribute
{
    readonly delegate*<SceneObject, FrameUpdateArgs, void> _func = null;

    public OnFrameUpdateAttribute(){}
    private OnFrameUpdateAttribute(nint functionPointer)
    {
        _func = (delegate*<SceneObject, FrameUpdateArgs, void>)functionPointer;
    }

    public override EngineMethodAttribute CreateUsableFromMethodInfo(MethodInfo info)
    {
        var handle = info.MethodHandle;
        var functionPointer = handle.GetFunctionPointer();

        var param = info.GetParameters();
        if(param.Length != 1) throw new Exception("OnFrameUpdate expects methods to have one parameter.");
        if(param[0].ParameterType != typeof(FrameUpdateArgs)) throw new Exception("OnFrameUpdate expects method's parameter to be SlopperEngine.Engine.FrameUpdateArgs.");
        if(info.ReturnType != typeof(void)) throw new Exception("OnFrameUpdate expects methods to return void.");

        return new OnFrameUpdateAttribute(functionPointer);
    }

    public override SceneDataHandle RegisterToScene(Scene scene, SceneObject obj)
    {
        if(_func == null) throw new Exception("Invalid OnUpdateAttribute used. It has to be created using CreateUsableFromMethodInfo.");
        return scene.RegisterSceneData<OnFrameUpdate>(new(_func, obj));
    }

    public override void UnregisterFromScene(Scene scene, SceneDataHandle handle, SceneObject obj)
    {
        scene.UnregisterSceneData<OnFrameUpdate>(handle, new(_func, obj));
    }
}

/// <summary>
/// Stores the necessary information to call OnUnregister on a SceneObject.
/// </summary>
public unsafe struct OnFrameUpdate
{
    readonly SceneObject _owner;
    readonly delegate*<SceneObject, FrameUpdateArgs, void> _func;
    
    public OnFrameUpdate(delegate*<SceneObject, FrameUpdateArgs, void> func, SceneObject owner)
    {
        _func = func;
        _owner = owner;
    }
    public void Invoke(FrameUpdateArgs args)
    {
        _func(_owner, args);
    }
}