using System.Reflection;
using SlopperEngine.Core.SceneData;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Core;

/// <summary>
/// Makes a method get called when the SceneObject gets removed and unregistered to the scene, at the end of the update loop.
/// </summary>
public unsafe sealed class OnUnregisterAttribute : EngineMethodAttribute
{
    readonly delegate*<SceneObject,Scene?,void> _func = null;

    public OnUnregisterAttribute(){}
    private OnUnregisterAttribute(nint functionPointer)
    {
        _func = (delegate*<SceneObject,Scene?,void>)functionPointer;
    }

    public override EngineMethodAttribute CreateUsableFromMethodInfo(MethodInfo info)
    {
        var handle = info.MethodHandle;
        var functionPointer = handle.GetFunctionPointer();

        var param = info.GetParameters();
        if(param.Length != 1) throw new Exception("OnUnregister expects methods to have one parameter.");
        if(param[0].ParameterType != typeof(Scene)) throw new Exception("OnUnregister expects method's parameter to be SlopperEngine.SceneObjects.Scene.");
        if(info.ReturnType != typeof(void)) throw new Exception("OnUnregister expects methods to return void.");

        return new OnUnregisterAttribute(functionPointer);
    }

    public override SceneDataHandle RegisterToScene(Scene scene, SceneObject obj)
    {
        return scene.RegisterSceneData<OnUnregister>(new(_func, obj));
    }

    public override void UnregisterFromScene(Scene scene, SceneDataHandle handle, SceneObject obj)
    {
        if(_func == null) throw new Exception("Invalid OnUnregisterAttribute used. It has to be created using CreateUsableFromMethodInfo.");
        scene.UnregisterSceneData<OnUnregister>(handle, new(_func, obj));
    }
}

/// <summary>
/// Stores the necessary information to call OnUnregister on a SceneObject.
/// </summary>
public unsafe struct OnUnregister
{
    public readonly SceneObject Owner;
    readonly delegate*<SceneObject, Scene?, void> _func;
    
    public OnUnregister(delegate*<SceneObject, Scene?, void> func, SceneObject owner)
    {
        this._func = func;
        this.Owner = owner;
    }
    public void Invoke(Scene? scene)
    {
        _func(Owner, scene);
    }
}