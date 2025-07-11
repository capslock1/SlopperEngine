namespace SlopperEngine.SceneObjects.ChildContainers;

/// <summary>
/// Events for ChildLists.
/// </summary>
public interface IChildListEvents<TSceneObject> where TSceneObject : SceneObject
{
    /// <summary>
    /// Gets called after a child gets added to the list.
    /// </summary>
    /// <param name="child">The child that got added.</param>
    /// <param name="childIndex">The index of the child that got added.</param>
    void OnChildAdded(TSceneObject child, int childIndex);
    /// <summary>
    /// Gets called after a child gets removed from the list.
    /// </summary>
    /// <param name="child">The child that got removed.</param>
    /// <param name="previousIndex">The index the child had before it got removed.</param>
    void OnChildRemoved(TSceneObject child, int previousIndex);
}