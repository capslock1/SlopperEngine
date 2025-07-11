namespace SlopperEngine.SceneObjects.ChildContainers;

public class ChildList<TSceneObject> : ChildList<TSceneObject, NoEvents<TSceneObject>> where TSceneObject : SceneObject
{
    public ChildList(SceneObject owner) : base(owner, default){}
}

/// <summary>
/// Stores a number of children of a particular type.
/// </summary>
public class ChildList<TSceneObject, TEvents> : SceneObject.ChildContainer
    where TSceneObject : SceneObject
    where TEvents : IChildListEvents<TSceneObject>
{
    public int Count => _children.Count;

    /// <summary>
    /// All children as an IEnumerable.
    /// </summary>
    //the reason ChildList isnt IEnumerable, is that you get 1 million extension methods on there.
    //this isnt inherently bad, but definitely obfuscates the intended way to interact with ChildList (where you remove or add, no other complicated stuff)
    //but getting an IEnumerable is convenient (in foreach in particular), so the All property is here to save us.
    public IEnumerable<TSceneObject> All => _children;

    readonly List<TSceneObject> _children = [];
    TEvents _events;

    public ChildList(SceneObject owner, TEvents events) : base(owner)
    {
        _events = events;
    }

    public SceneObject GetOwner() => Owner;

    /// <summary>
    /// Adds a child to this SceneObject.
    /// </summary>
    /// <param name="newChild">The child to add.</param>
    /// <exception cref="Exception"></exception>
    public void Add(TSceneObject newChild)
    {
        if (Owner == newChild) throw new Exception("Cannot add a SceneObject to its own ChildList!");
        if (newChild is Scene) throw new Exception("Cannot add a Scene to a ChildList!");
        if (newChild.Destroyed) throw new Exception("Cannot add a destroyed SceneObject as a child!");

        newChild.Remove();

        SetChildListIndex(newChild, Count);
        _children.Add(newChild);
        TryRegister(newChild);

        _events.OnChildAdded(newChild, Count-1);
    }

    public TSceneObject this[int index]
    {
        get => _children[index];
    }

    /// <summary>
    /// Gets the first child of the SceneObject of a specific type.
    /// </summary>
    /// <typeparam name="TToGet">The type of child to get.</typeparam>
    /// <returns></returns>
    public TToGet? FirstOfType<TToGet>() where TToGet : TSceneObject
    {
        foreach (var ch in _children)
            if (ch is TToGet Tobj) return Tobj;
        return null;
    }

    /// <summary>
    /// Gets a list of children of this SceneObject of a specific type.
    /// </summary>
    /// <typeparam name="TToGet">The type of children to get.</typeparam>
    /// <returns></returns>
    public List<TToGet> AllOfType<TToGet>() where TToGet : TSceneObject
    {
        List<TToGet> res = new();
        foreach (var ch in _children)
            if (ch is TToGet Tobj) res.Add(Tobj);
        return res;
    }

    protected override int GetCount() => Count;
    protected override SceneObject GetByIndex(int index) => this[index];

    protected override void RemoveByIndex(int index)
    {
        var removed = _children[index];
        TryUnregister(removed);

        for (int i = index; i < _children.Count - 1; i++)
        {
            _children[i] = _children[i + 1];
            SetChildListIndex(_children[i], i);
        }
        _children.RemoveAt(_children.Count - 1);

        UnsetChildListIndex(removed);

        _events.OnChildRemoved(removed, index);
    }

    protected override void SetAllChildrensListIndex()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            SetChildListIndex(_children[i], i);
        }
    }
}