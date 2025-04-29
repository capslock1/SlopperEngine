using System.Reflection;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Physics;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Core;

/// <summary>
/// Makes a method get called every frame, before the scene gets rendered.
/// </summary>
public unsafe sealed class OnPhysicsUpdateAttribute : EngineMethodAttribute
{
    readonly delegate*<SceneObject, PhysicsUpdateArgs, void> _func = null;

    public OnPhysicsUpdateAttribute(){}
    private OnPhysicsUpdateAttribute(nint functionPointer)
    {
        _func = (delegate*<SceneObject, PhysicsUpdateArgs, void>)functionPointer;
    }

    public override EngineMethodAttribute CreateUsableFromMethodInfo(MethodInfo info)
    {
        var handle = info.MethodHandle;
        var functionPointer = handle.GetFunctionPointer();

        var param = info.GetParameters();
        if(param.Length != 1) throw new Exception("OnPhysicsUpdate expects methods to have one parameter.");
        if(param[0].ParameterType != typeof(PhysicsUpdateArgs)) throw new Exception("OnPhysicsUpdate expects method's parameter to be SlopperEngine.Physics.PhysicsUpdateArgs.");
        if(info.ReturnType != typeof(void)) throw new Exception("OnPhysicsUpdate expects methods to return void.");

        return new OnPhysicsUpdateAttribute(functionPointer);
    }

    public override SceneDataHandle RegisterToScene(Scene scene, SceneObject obj)
    {
        if(_func == null) throw new Exception("Invalid OnPhysicsUpdateAttribute used. It has to be created using CreateUsableFromMethodInfo.");
        return scene.RegisterSceneData<OnPhysicsUpdate>(new(_func, obj));
    }

    public override void UnregisterFromScene(Scene scene, SceneDataHandle handle, SceneObject obj)
    {
        scene.UnregisterSceneData<OnPhysicsUpdate>(handle, new(_func, obj));
    }
}

/// <summary>
/// Stores the necessary information to call OnUnregister on a SceneObject.
/// </summary>
public unsafe struct OnPhysicsUpdate
{
    readonly SceneObject _owner;
    readonly delegate*<SceneObject, PhysicsUpdateArgs, void> _func;
    
    public OnPhysicsUpdate(delegate*<SceneObject, PhysicsUpdateArgs, void> func, SceneObject owner)
    {
        _func = func;
        _owner = owner;
    }
    public void Invoke(PhysicsUpdateArgs args)
    {
        _func(_owner, args);
    }
}