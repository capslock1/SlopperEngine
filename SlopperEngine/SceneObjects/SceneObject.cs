using System.Collections;
using System.Collections.ObjectModel;
using OpenTK.Mathematics;
using SlopperEngine.Engine;
using SlopperEngine.Engine.SceneData;

namespace SlopperEngine.SceneObjects;

/// <summary>
/// An object that represents a presence within a Scene.
/// </summary>
public class SceneObject
{
    /// <summary>
    /// The parent of the object.
    /// </summary>
    public SceneObject? Parent => _parentList?.Owner;

    /// <summary>
    /// Whether the object is within a Scene.
    /// </summary>
    public bool InScene => Scene != null;

    /// <summary>
    /// The scene this object is in.
    /// </summary>
    public Scene? Scene {get; private set;}

    /// <summary>
    /// Whether the object exists or is destroyed. Destroyed objects cannot be used in Scenes.
    /// </summary>
    public bool Destroyed {get; private set;}

    /// <summary>
    /// The default list of children for this SceneObject. Can contain any type of SceneObject.
    /// </summary>
    public ChildList<SceneObject> Children => _children ??= new ChildList<SceneObject>(this);
    
    private IChildList? _parentList = null;
    private ChildList<SceneObject>? _children;
    private int _parentListIndex = -1;
    private bool _registryComplete = false;
    private List<IChildList>? _childLists;

    //premature optimisation? couldnt be me.
    //actually, the registered method handles could be stored in the scene to avoid allocations..... i shant
    readonly private ReadOnlyCollection<EngineMethodAttribute> _registeredMethods;
    readonly private SceneDataHandle[] _registeredMethodHandles;

    /// <summary>
    /// Creates a SceneObject.
    /// </summary>
    public SceneObject()
    {
        if(this is Scene s)
            Scene = s;
        else Scene = null;

        _registeredMethods = SceneObjectReflectionCache.GetEngineMethods(GetType());
        _registeredMethodHandles = new SceneDataHandle[_registeredMethods.Count];
    }

    [OnRegister] void CompleteRegister() => _registryComplete = true;
    void Register(Scene sc)
    {
        if(Destroyed) 
            throw new Exception("whoops! "+this+" has been destroyed, and should not have been registered.");
        _registryComplete = false;
        Scene = sc;

        for(int i = 0; i<_registeredMethods.Count; i++)
            _registeredMethodHandles[i] = _registeredMethods[i].RegisterToScene(sc, this);

        if(_childLists != null)
            foreach(var children in _childLists)
                children.CheckRegistered();
    }
    void Unregister()
    {
        if(Scene is not null)
        {
            if(!_registryComplete)
                Scene.UpdateRegister();

            for(int i = 0; i<_registeredMethods.Count; i++)
            {
                _registeredMethods[i].UnregisterFromScene(Scene, _registeredMethodHandles[i], this);
                _registeredMethodHandles[i] = default;
            }
            Scene = null;
        }

        if(_childLists != null)
            foreach(var children in _childLists)
                children.CheckRegistered();
    }

    /// <summary>
    /// Removes this SceneObject from its parent.
    /// </summary>
    public void Remove() => _parentList?.Remove(_parentListIndex);

    /// <summary>
    /// Destroys this SceneObject. After destruction, a SceneObject will insist that it is null, and can no longer be used in scenes.
    /// </summary>
    public void Destroy()
    {
        Unregister();
        SetDestroyed();
        OnDestroyed();
        Remove();
    }
    private void SetDestroyed()
    {
        if(_childLists != null)
            foreach(var children in _childLists)
                for(int i = 0; i < children.Count; i++)
                    children.Get(i).SetDestroyed();

        Destroyed = true;
    }
    protected virtual void OnDestroyed(){}
    
    /// <summary>
    /// Gets the transform from this object's space to global space, also known as the model-to-global space matrix.
    /// </summary>
    public virtual Matrix4 GetGlobalTransform()
    {
        if(Parent is null)
            return Matrix4.Identity;
        return Parent.GetGlobalTransform();
    }

    //the evil '== null' code
    public static bool operator ==(SceneObject? A, SceneObject? B)
    {
        bool refEquals = ReferenceEquals(A,B);
        if(refEquals) return true;
        if(B is null)
        {
            if(A is null) return true;
            return A.Destroyed;
        }
        if(A is null) return B.Destroyed;
        return false;
    }
    public static bool operator !=(SceneObject? A, SceneObject? B) => !(A == B);
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();


    /// <summary>
    /// Stores a number of children of a particular type.
    /// </summary>
    /// <typeparam name="TSceneObject"></typeparam>
    /// <param name="owner"></param>
    public class ChildList<TSceneObject> : IChildList where TSceneObject : SceneObject
    {
        public SceneObject Owner {get; private set;}
        public int Count => _children.Count;
        
        /// <summary>
        /// All children as an IEnumerable.
        /// </summary>
        //the reason ChildList isnt IEnumerable, is that you get 1 million extension methods on there.
        //this isnt inherently bad, but definitely obfuscates the intended way to interact with ChildList (where you remove or add, no other complicated stuff)
        //but getting an IEnumerable is convenient (in foreach in particular), so the All property is here to save us.
        public IEnumerable<TSceneObject> All => _children.AsReadOnly(); 

        readonly List<TSceneObject> _children = [];
        bool _currentlyRegistered;

        public ChildList(SceneObject owner)
        {
            Owner = owner;
            _currentlyRegistered = owner.InScene;
            owner._childLists ??= new(1);
            owner._childLists.Add(this);
        }

        public SceneObject GetOwner() => Owner;

        /// <summary>
        /// Adds a child to this SceneObject.
        /// </summary>
        /// <param name="newChild">The child to add.</param>
        /// <exception cref="Exception"></exception>
        public void Add(TSceneObject newChild)
        {
            if(Owner == newChild) throw new Exception("Cannot add a SceneObject to its own ChildList!");
            if(newChild is Scene) throw new Exception("Cannot add a Scene to a ChildList!");
            if(newChild.Destroyed) throw new Exception("Cannot add a destroyed SceneObject as a child!");

            newChild.Remove();

            newChild._parentList = this;
            newChild._parentListIndex = _children.Count;
            _children.Add(newChild);

            if(_currentlyRegistered) 
                newChild.Register(Owner.Scene!);
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
            foreach(var ch in _children)
                if(ch is TToGet Tobj) return Tobj;
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
            foreach(var ch in _children)
                if(ch is TToGet Tobj) res.Add(Tobj);
            return res;
        }

        public void Remove(int index)
        {
            var removed = _children[index];
            if(removed.Scene is not null) 
                removed.Unregister();

            for(int i = index; i<_children.Count-1; i++)
            {
                _children[i] = _children[i+1];
                _children[i]._parentListIndex = i;
            }
            _children.RemoveAt(_children.Count-1);

            removed._parentListIndex = -1;
            removed._parentList = null;
        }

        public void CheckRegistered()
        {
            if(Owner.InScene == _currentlyRegistered) return;

            _currentlyRegistered = Owner.InScene;
            if(_currentlyRegistered)
            {
                foreach(var ch in _children)
                    ch.Register(Owner.Scene!);
            }
            else
            {
                foreach(var ch in _children)
                    ch.Unregister();
            }
        }

        public SceneObject Get(int index) => this[index];
    }

    /// <summary>
    /// Stores a single child of a SceneObject.
    /// </summary>
    /// <typeparam name="TSceneObject">The type of SceneObject to store.</typeparam>
    public class SingleChild<TSceneObject> : IChildList where TSceneObject : SceneObject
    {
        public SceneObject Owner {get; private set;}
        public int Count => _child == null ? 0 : 1;
        public TSceneObject? Value
        {
            get => _child;
            set 
            {
                if(Owner == value) throw new Exception("Cannot set a SceneObject as its own child!");
                if(value is Scene) throw new Exception("Cannot set a Scene as a child!");
                
                Remove(0);
                _child = value;
                if(_child == null) 
                    return;
                
                _child.Remove();
                _child._parentList = this;
                if(_currentlyRegistered)
                    _child.Register(Owner.Scene!);
            }
        }

        TSceneObject? _child;
        bool _currentlyRegistered;

        public SingleChild(SceneObject owner)
        {
            Owner = owner;
            _currentlyRegistered = owner.InScene;
            owner._childLists ??= new(1);
            owner._childLists.Add(this);
        }

        public void CheckRegistered()
        {
            if(_currentlyRegistered == Owner.InScene) return;
            if(_child == null) return;

            if(_currentlyRegistered)
                _child.Register(Owner.Scene!);
            else _child.Unregister();
        }

        public SceneObject Get(int index) => _child!; // if you nullref here, thats intentional.

        public void Remove(int index)
        {
            if(_child == null) return;
            if(_child.Scene is not null)
                _child.Unregister();
            _child = null;
        }
    }
    protected interface IChildList
    {
        /// <summary>
        /// The owner of this ChildList.
        /// </summary>
        public SceneObject Owner {get;}

        /// <summary>
        /// Removes a child by index.
        /// </summary>
        /// <param name="index">The index of the child to remove.</param>
        public void Remove(int index);

        /// <summary>
        /// Gets a child by index.
        /// </summary>
        /// <param name="index">The index of the child. Should be between -1 and Count.</param>
        public SceneObject Get(int index);

        /// <summary>
        /// The amount of children this ChildList stores.
        /// </summary>
        public int Count {get;}
        
        /// <summary>
        /// Should be called by the owner when it gets registered/unregistered.
        /// </summary>
        public void CheckRegistered();
    }
}
