using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SlopperEngine.Core;
using SlopperEngine.Core.Collections;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Core.Serialization;
using SlopperEngine.Rendering;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.SceneObjects.ChildContainers;

namespace SlopperEngine.SceneObjects;

/// <summary>
/// Contains a set of SceneObjects, and handles their update loop.
/// </summary>
public sealed class Scene : SceneObject
{
    public static ReadOnlyCollection<Scene> ActiveScenes => _activeScenes.AsReadOnly();
    public readonly ChildList<SceneComponent> Components;
    public readonly ChildList<SceneRenderer> Renderers;
    public UpdateHandler? UpdateHandler {get; private set;}
    public SceneRenderer? SceneRenderer {get; private set;}
    public PhysicsHandler? PhysicsHandler {get; private set;}

    [DontSerialize] FridgeDictionary<Type, ISceneDataContainer> _dataContainers = new();
    [DontSerialize] RegisterHandler _register;
    static readonly List<Scene> _activeScenes = [];

    private Scene()
    {
        _activeScenes.Add(this);
        _register = new(this);
        Components = new(this);
        Renderers = new(this);
    }
    [OnSerialize] void OnSerialize(OnSerializeArgs serializer)
    {
        if (serializer.IsWriter)
        {
            _activeScenes.Add(this);
            _dataContainers = new();
            _register = new(this);
            serializer.CallAfterSerialize(UpdateRegister);
        }
    }

    /// <summary>
    /// Creates a scene with several useful components pre-added.
    /// </summary>
    /// <returns>A scene with a DebugRenderer, UpdateHandler, and a PhysicsHandler.</returns>
    public static Scene CreateDefault()
    {
        Scene res = new();
        res.Renderers.Add(new DebugRenderer());
        res.Components.Add(new UpdateHandler()); //should the updatehandler even be optional?
        res.Components.Add(new PhysicsHandler());
        res._register.Resolve();
        return res;
    }

    /// <summary>
    /// Creates a scene with no attached components. It will not update or render its children.
    /// </summary>
    public static Scene CreateEmpty()
    {
        return new Scene();
    }

    /// <summary>
    /// Updates the scene.
    /// </summary>
    /// <param name="update">Arguments for the update.</param>
    public void FrameUpdate(FrameUpdateArgs update)
    {
        UpdateRegister();
        FinalizeQueues();
        foreach(var comp in Components.All)
            comp.FrameUpdate(update);
        UpdateRegister();
        FinalizeQueues();
    }

    /// <summary>
    /// Updates the scene's inputs.
    /// </summary>
    /// <param name="update">Arguments for the update.</param>
    public void InputUpdate(InputUpdateArgs update)
    {
        UpdateRegister();
        FinalizeQueues();
        foreach(var comp in Components.All)
            comp.InputUpdate(update);
        foreach (var rend in Renderers.All)
            rend.InputUpdate(update);
        UpdateRegister();
        FinalizeQueues();
    }

    /// <summary>
    /// Renders the scene.
    /// </summary>
    public void Render(FrameUpdateArgs args)
    {
        UpdateRegister();
        FinalizeQueues();
        foreach(var rend in Renderers.AllOfType<SceneRenderer>())
            rend.Render(args);
        UpdateRegister();
        FinalizeQueues();
    }

    /// <summary>
    /// Gets a SceneDataContainer as IEnumerable. Should not be modified.
    /// </summary>
    /// <typeparam name="T">The type of scene data to enumerate.</typeparam>
    public IRefEnumerable<T> GetDataContainerEnumerable<T>()
    {
        return GetOrCreateDataContainer<T>();
    }

    /// <summary>
    /// Registers data to the scene.
    /// </summary>
    /// <typeparam name="T">The type of data to register.</typeparam>
    /// <param name="sceneData">The data to register.</param>
    /// <returns>The unique handle to delete this data with later.</returns>
    public SceneDataHandle RegisterSceneData<T>(T sceneData)
    {
        var handle = GetOrCreateDataContainer<T>().QueueAdd(sceneData);
        //System.Console.WriteLine("Registered "+typeof(T)+" at "+handle.Index);
        return handle;
    }

    /// <summary>
    /// Unregisters data from the scene.
    /// </summary>
    /// <typeparam name="T">The data to unregister.</typeparam>
    /// <param name="handle">The data's handle.</param>
    public void UnregisterSceneData<T>(SceneDataHandle handle, T data)
    {
        //System.Console.WriteLine("Unregistered "+typeof(T)+" at "+handle.Index);
        if(handle.IsRegistered)
            GetOrCreateDataContainer<T>().QueueRemove(handle, data);
    }
    
    SceneDataContainer<T> GetOrCreateDataContainer<T>()
    {
        if(_dataContainers.TryGetValue(typeof(T), out var container))
            return Unsafe.As<SceneDataContainer<T>>(container);

        SceneDataContainer<T> result = new SparseSemiorderedSceneDataContainer<T>(); //sparsesemiordered by default cuz I DONT CARE :D
        AddDataContainer(result);
        return result;
    }

    /// <summary>
    /// Adds a data container for a specific type.
    /// </summary>
    /// <typeparam name="T">The type of data to contain.</typeparam>
    /// <param name="container">The container to add to the scene.</param>
    /// <exception cref="Exception"></exception>
    public void AddDataContainer<T>(SceneDataContainer<T> container)
    {
        if(_dataContainers.TryAdd(typeof(T), container)) 
            container.OnAddedToScene(this);
        else throw new Exception($"There was already a DataContainer of type {typeof(T)} in the scene.");
    }

    /// <summary>
    /// Adds a specific type of data container, unless the scene contains one already.
    /// </summary>
    /// <typeparam name="T">The type of SceneData.</typeparam>
    /// <typeparam name="TContainer">The type of the container to possibly add.</typeparam>
    public void TryAddDataContainerNew<T, TContainer>() where TContainer : SceneDataContainer<T>, new()
    {
        if(_dataContainers.TryGetValue(typeof(T), out var _)) 
            return;
        var res = new TContainer();
        _dataContainers.Add(typeof(T), res);
    }

    /// <summary>
    /// Updates the Register functions of SceneObjects. Should be used generously in scene interoperation.
    /// </summary>
    public void UpdateRegister()
    {
        //limit resolving to 100 for safety - would rather have unexpected behaviour than an infinite loop... i think
        //is this even necessary?
        for(int i = 0; i<100; i++) 
        {
            _register.Resolve();
            if(_register.QueueFinished) break;
        }
    }

    /// <summary>
    /// Sets the cached components (Scene.SceneRenderer, Scene.UpdateHandler, etc).
    /// </summary>
    public void CheckCachedComponents()
    {
        SceneRenderer = Renderers.FirstOfType<SceneRenderer>();
        UpdateHandler = Components.FirstOfType<UpdateHandler>();
        PhysicsHandler = Components.FirstOfType<PhysicsHandler>();
    }

    void FinalizeQueues()
    {
        //seperate from UpdateRegister.
        //registry has to be finalized almost randomly while scene data needs to be finalized only at the end of a frame (pretty much)
        //the reason is, imagine an object with FrameUpdate while doing a FrameUpdate
        //the container can decide to place this before *or* after the current FrameUpdate's execution
        //hence, finalizing the queue while working in a scene update would cause undefined behaviour (as most data containers arent built for this)
        //registry is the exception here, as its just a queue and never enumerates.
        //in that sense, registry functions have a *monopoly* in responding to registering/unregistering directly
        foreach(var typeContainer in _dataContainers)
            typeContainer.Value.FinalizeQueue();
        CheckCachedComponents();
    }

    protected override void OnDestroyed()
    {
        _activeScenes.Remove(this);
    }
}
