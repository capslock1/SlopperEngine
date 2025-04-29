using System.Reflection;
using SlopperEngine.Core.SceneData;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Core;

/// <summary>
/// Makes a method get called every frame, before the scene gets rendered.
/// </summary>
public unsafe sealed class OnInputUpdateAttribute : EngineMethodAttribute
{
    readonly delegate*<SceneObject, InputUpdateArgs, void> _func = null;

    public OnInputUpdateAttribute(){}
    private OnInputUpdateAttribute(nint functionPointer)
    {
        _func = (delegate*<SceneObject, InputUpdateArgs, void>)functionPointer;
    }

    public override EngineMethodAttribute CreateUsableFromMethodInfo(MethodInfo info)
    {
        var handle = info.MethodHandle;
        var functionPointer = handle.GetFunctionPointer();

        var param = info.GetParameters();
        if(param.Length != 1) throw new Exception("OnInputUpdate expects methods to have one parameter.");
        if(param[0].ParameterType != typeof(InputUpdateArgs)) throw new Exception("OnInputUpdate expects method's parameter to be SlopperEngine.Engine.InputUpdateArgs.");
        if(info.ReturnType != typeof(void)) throw new Exception("OnInputUpdate expects methods to return void.");

        return new OnInputUpdateAttribute(functionPointer);
    }

    public override SceneDataHandle RegisterToScene(Scene scene, SceneObject obj)
    {
        if(_func == null) throw new Exception("Invalid OnUpdateAttribute used. It has to be created using CreateUsableFromMethodInfo.");
        return scene.RegisterSceneData<OnInputUpdate>(new(_func, obj));
    }

    public override void UnregisterFromScene(Scene scene, SceneDataHandle handle, SceneObject obj)
    {
        scene.UnregisterSceneData<OnInputUpdate>(handle, new(_func, obj));
    }
}

/// <summary>
/// Stores the necessary information to call OnUnregister on a SceneObject.
/// </summary>
public unsafe struct OnInputUpdate(delegate*<SceneObject, InputUpdateArgs, void> func, SceneObject owner)
{
    readonly SceneObject _owner = owner;
    readonly delegate*<SceneObject, InputUpdateArgs, void> _func = func;

    public void Invoke(InputUpdateArgs args)
    {
        _func(_owner, args);
    }
}