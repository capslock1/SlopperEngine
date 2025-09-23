using System.Collections.ObjectModel;
using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.SceneObjects.ChildContainers;

namespace SlopperEngine.SceneObjects;

/// <summary>
/// An object that represents a presence within a Scene.
/// </summary>
public partial class SceneObject
{
    /// <summary>
    /// The parent of the object.
    /// </summary>
    [EditorIntegration.HideInInspector]
    public SceneObject? Parent => _parentList?.Owner;

    /// <summary>
    /// The childcontainer containing this object.
    /// </summary>
    [EditorIntegration.HideInInspector]
    public ChildContainer? ParentContainer => _parentList;

    /// <summary>
    /// Whether the object is within a Scene.
    /// </summary>
    [EditorIntegration.HideInInspector]
    public bool InScene => Scene != null;

    /// <summary>
    /// The scene this object is in.
    /// </summary>
    [EditorIntegration.HideInInspector]
    [field:DontSerialize] public Scene? Scene {get; private set;}

    /// <summary>
    /// Whether the object exists or is destroyed. Destroyed objects cannot be used in Scenes.
    /// </summary>
    [EditorIntegration.HideInInspector]
    public bool Destroyed {get; private set;}

    /// <summary>
    /// The default list of children for this SceneObject. Can contain any type of SceneObject.
    /// </summary>
    public ChildList<SceneObject> Children => _children ??= new ChildList<SceneObject>(this);

    [DontSerialize] private ChildContainer? _parentList = null;
    private ChildList<SceneObject>? _children;
    [DontSerialize] private int _parentListIndex = -1;
    [DontSerialize] private bool _registryComplete = false;
    [DontSerialize] private List<ChildContainer>? _childContainers;

    //premature optimisation? couldnt be me.
    //actually, the registered method handles could be stored in the scene to avoid allocations..... i shant
    [DontSerialize] private ReadOnlyCollection<EngineMethodAttribute> _registeredMethods;
    [DontSerialize] private SceneDataHandle[] _registeredMethodHandles;

    /// <summary>
    /// Creates a SceneObject.
    /// </summary>
    #pragma warning disable CS8618 // yes it does
    public SceneObject()
    #pragma warning restore CS8618
    {
        Initialize();
    }

    private void Initialize()
    {
        if(this is Scene s)
            Scene = s;
        else Scene = null;

        _registeredMethods = SceneObjectReflectionCache.GetEngineMethods(GetType());
        _registeredMethodHandles = new SceneDataHandle[_registeredMethods.Count];
    }

    [OnSerialize] void OnSerialize(OnSerializeArgs serializer)
    {
        if(serializer.IsWriter)
        {
            Initialize();
            if (this is Scene s)
            {
                Scene = s;
                serializer.CallAfterSerialize(() => { Register(s); });
            }
        }
    }

    [OnRegister] void CompleteRegister() => _registryComplete = true;
    internal void Register(Scene sc)
    {
        if(Destroyed) 
            throw new Exception("whoops! "+this+" has been destroyed, and should not have been registered.");
        _registryComplete = false;
        Scene = sc;

        for(int i = 0; i<_registeredMethods.Count; i++)
            _registeredMethodHandles[i] = _registeredMethods[i].RegisterToScene(sc, this);

        if(_childContainers != null)
            foreach(var children in _childContainers)
                children.CheckRegistered();
    }

    internal void Unregister()
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

        if(_childContainers != null)
            foreach(var children in _childContainers)
                children.CheckRegistered();
    }

    /// <summary>
    /// Removes this SceneObject from its parent.
    /// </summary>
    public void Remove() => _parentList?.RemoveByIndex(_parentListIndex);

    /// <summary>
    /// Destroys this SceneObject. After destruction, a SceneObject will insist that it is null, and can no longer be used in scenes.
    /// </summary>
    public void Destroy()
    {
        Unregister();
        SetDestroyed();
        Remove();
    }
    private void SetDestroyed()
    {
        if(_childContainers != null)
            foreach(var children in _childContainers)
                for(int i = 0; i < children.Count; i++)
                    children.GetByIndex(i).SetDestroyed();

        Destroyed = true;
        OnDestroyed();
    }
    
    /// <summary>
    /// Gets called after the SceneObject is destroyed.
    /// </summary>
    protected virtual void OnDestroyed() { }
    
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
