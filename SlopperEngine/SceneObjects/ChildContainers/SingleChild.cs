
using System;

namespace SlopperEngine.SceneObjects.ChildContainers;

public class SingleChild<TSceneObject> : SingleChild<TSceneObject, NoEvents<TSceneObject>>
    where TSceneObject : SceneObject
{
    public SingleChild(SceneObject owner) : base(owner, default) { }
}

/// <summary>
/// Stores a single child of a SceneObject.
/// </summary>
/// <typeparam name="TSceneObject">The type of SceneObject to store.</typeparam>
public class SingleChild<TSceneObject, TEvents> : SceneObject.ChildContainer
    where TSceneObject : SceneObject
    where TEvents : IChildListEvents<TSceneObject>
{
    public override int Count => _child == null ? 0 : 1;

    /// <summary>
    /// The child stored in the SingleChild.
    /// </summary>
    public TSceneObject? Value
    {
        get => _child;
        set
        {
            if (Owner == value) throw new Exception("Cannot set a SceneObject as its own child!");
            if (value is Scene) throw new Exception("Cannot set a Scene as a child!");

            RemoveByIndex(0);
            _child = value;
            if (_child == null)
                return;

            SetChildListIndex(_child, -1);
            TryRegister(_child);

            _events.OnChildAdded(_child, 0);
        }
    }

    TSceneObject? _child;
    TEvents _events;

    public SingleChild(SceneObject owner, TEvents events) : base(owner)
    {
        _events = events;
    }

    public override SceneObject GetByIndex(int index) => _child!; // if you nullref here, thats intented and deserved.

    public override void RemoveByIndex(int index)
    {
        if (_child == null) return;
        TryUnregister(_child);
        UnsetChildListIndex(_child);
        _events.OnChildRemoved(_child, index);
        _child = null;
    }

    public override bool TryAdd(SceneObject obj)
    {
        if (Value != null && obj is TSceneObject t)
        {
            Value = t;
            return true;
        }
        return false;
    }

    protected override void SetAllChildrensListIndex()
    {
        if (_child != null)
            SetChildListIndex(_child, -1);
    }

    public static implicit operator TSceneObject?(SingleChild<TSceneObject, TEvents> child) => child.Value;
}