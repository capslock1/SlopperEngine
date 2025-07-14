
using System.Collections;
using SlopperEngine.Core.Collections;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Core.SceneData;

/// <summary>
/// Abstract container for mass amounts of scene data, like drawcalls or registered methods. 
/// </summary>
/// <typeparam name="T">The data type to store. Optimally a struct.</typeparam>
public abstract class SceneDataContainer<T> : IRefEnumerable<T>, ISceneDataContainer
{

    /// <summary>
    /// Adds data to the container. Is only used once FinalizeQueue is called.
    /// </summary>
    /// <param name="data">The data to set.</param>
    public abstract SceneDataHandle QueueAdd(T data);

    /// <summary>
    /// Removes data from the container. Is only removed once FinalizeQueue is called.
    /// </summary>
    /// <param name="handle">The data's handle.</param>
    /// <param name="data">The data that should be removed. Should only be taken into account in situations where the container cannot find it.</param>
    public abstract void QueueRemove(SceneDataHandle handle, T data);
    
    public abstract void FinalizeQueue();

    public virtual void OnAddedToScene(Scene scene){}

    public abstract void Enumerate<TEnumerator>(ref TEnumerator enumerator) where TEnumerator : IRefEnumerator<T>, allows ref struct;
}

/// <summary>
/// Stores the SceneDataContainer without the generic bit. Should only ever be inherited by SceneDataContainer
/// </summary>
internal interface ISceneDataContainer
{
    /// <summary>
    /// Finalizes the add/remove queue.
    /// </summary>
    public void FinalizeQueue();
}