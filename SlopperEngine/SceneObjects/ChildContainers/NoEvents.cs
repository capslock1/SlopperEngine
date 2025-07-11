namespace SlopperEngine.SceneObjects.ChildContainers;

/// <summary>
/// Handles the absence of events on a ChildList.
/// </summary>
public struct NoEvents<TSceneObject> : IChildListEvents<TSceneObject> where TSceneObject : SceneObject
{
    public void OnChildAdded(TSceneObject child, int childIndex){}
    public void OnChildRemoved(TSceneObject child, int previousIndex){}
}