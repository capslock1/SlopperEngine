using System.Reflection;
using SlopperEngine.Core.SceneData;
using SlopperEngine.SceneObjects;
using System;

namespace SlopperEngine.Core;

/// <summary>
/// Makes a method get called when the SceneObject gets added and registered to the scene, at the end of the update loop.
/// </summary>
public unsafe sealed class OnRegisterAttribute : EngineMethodAttribute
{
    readonly delegate*<SceneObject, void> _func = null;

    public OnRegisterAttribute(){}
    private OnRegisterAttribute(nint functionPointer)
    {
        _func = (delegate*<SceneObject,void>)functionPointer;
    }

    public override EngineMethodAttribute CreateUsableFromMethodInfo(MethodInfo info)
    {
        var handle = info.MethodHandle;
        var functionPointer = handle.GetFunctionPointer();

        if(info.GetParameters().Length != 0) throw new Exception("OnRegister expects methods to have no parameters.");
        if(info.ReturnType != typeof(void)) throw new Exception("OnRegister expects methods to return void.");

        return new OnRegisterAttribute(functionPointer);
    }

    public override SceneDataHandle RegisterToScene(Scene scene, SceneObject obj)
    {
        if(_func == null) throw new Exception("Invalid OnRegisterAttribute used. It has to be created using CreateUsableFromMethodInfo.");
        return scene.RegisterSceneData<OnRegister>(new(_func, obj));
    }

    public override void UnregisterFromScene(Scene scene, SceneDataHandle handle, SceneObject obj)
    {
        scene.UnregisterSceneData<OnRegister>(handle, new());
    }
}

/// <summary>
/// Stores the necessary information to call OnRegister on a SceneObject.
/// </summary>
public unsafe struct OnRegister
{
    public readonly SceneObject Owner;
    readonly delegate*<SceneObject, void> _func;
    
    public OnRegister(delegate*<SceneObject, void> func, SceneObject owner)
    {
        _func = func;
        Owner = owner;
    }
    public void Invoke()
    {
        _func(Owner);
    }
}