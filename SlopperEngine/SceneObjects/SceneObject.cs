using System.Collections.ObjectModel;
using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.SceneObjects;

/// <summary>
/// An object that represents a presence within a Scene.
/// </summary>
public partial class SceneObject
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
    [field:DontSerialize] public Scene? Scene {get; private set;}

    /// <summary>
    /// Whether the object exists or is destroyed. Destroyed objects cannot be used in Scenes.
    /// </summary>
    public bool Destroyed {get; private set;}

    /// <summary>
    /// The default list of children for this SceneObject. Can contain any type of SceneObject.
    /// </summary>
    public ChildList<SceneObject> Children => _children ??= new ChildList<SceneObject>(this);
    
    [DontSerialize] private IChildList? _parentList = null;
    private ChildList<SceneObject>? _children;
    [DontSerialize] private int _parentListIndex = -1;
    [DontSerialize] private bool _registryComplete = false;
    [DontSerialize] private List<IChildList>? _childLists;

    //premature optimisation? couldnt be me.
    //actually, the registered method handles could be stored in the scene to avoid allocations..... i shant
    [DontSerialize] readonly private ReadOnlyCollection<EngineMethodAttribute> _registeredMethods;
    [DontSerialize] readonly private SceneDataHandle[] _registeredMethodHandles;

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
    public Matrix4 GetGlobalTransform()
    {
        Matrix4 res = Matrix4.Identity;
        RecursiveParentTransform(ref res, this);
        return res;

        void RecursiveParentTransform(ref Matrix4 mat, SceneObject curr)
        {
            if(curr.Parent is not null)
                RecursiveParentTransform(ref mat, curr.Parent);
            curr.TransformFromParent(ref mat);
        }
    }

    /// <summary>
    /// In overriding classes, this function should transform the parent's transform to the local transform.
    /// </summary>
    /// <param name="parentMatrix">The parent's transform.</param>
    protected virtual void TransformFromParent(ref Matrix4 parentMatrix){}

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
}
