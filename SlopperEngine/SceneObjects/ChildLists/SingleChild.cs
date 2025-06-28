using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.ChildLists;

namespace SlopperEngine.SceneObjects;

public partial class SceneObject
{
    public class SingleChild<TSceneObject> : SingleChild<TSceneObject, NoEvents<TSceneObject>>
        where TSceneObject : SceneObject
    {
        public SingleChild(SceneObject owner) : base(owner, default) {}
    }

    /// <summary>
    /// Stores a single child of a SceneObject.
    /// </summary>
    /// <typeparam name="TSceneObject">The type of SceneObject to store.</typeparam>
    public class SingleChild<TSceneObject, TEvents> : IChildList
        where TSceneObject : SceneObject
        where TEvents : IChildListEvents<TSceneObject>
    {
        public SceneObject Owner { get; private set; }
        int IChildList.Count => _child == null ? 0 : 1;

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

                (this as IChildList).Remove(0);
                _child = value;
                if (_child == null)
                    return;

                _child._parentList = this;
                if (_currentlyRegistered)
                    _child.Register(Owner.Scene!);

                _events.OnChildAdded(_child, 0);
            }
        }

        TSceneObject? _child;
        TEvents _events;
        bool _currentlyRegistered;

        public SingleChild(SceneObject owner, TEvents events)
        {
            Owner = owner;
            _currentlyRegistered = owner.InScene;
            _events = events;
            Init();
        }

        void Init()
        {
            Owner._childLists ??= new(1);
            Owner._childLists.Add(this);
        }

        [OnSerialize]
        void OnSerialize(SerializedObjectTree.CustomSerializer serializer)
        {
            if (serializer.IsReader) return;

            Init();
            if (_child != null)
                _child._parentList = this;
        }

        void IChildList.CheckRegistered()
        {
            if (_currentlyRegistered == Owner.InScene) return;
            _currentlyRegistered = Owner.InScene;

            if (_child == null) return;

            if (_currentlyRegistered)
                _child.Register(Owner.Scene!);
            else _child.Unregister();
        }

        SceneObject IChildList.Get(int index) => _child!; // if you nullref here, thats intentional.

        void IChildList.Remove(int index)
        {
            if (_child == null) return;
            if (_child.Scene is not null)
                _child.Unregister();

            _events.OnChildRemoved(_child, index);
            _child = null;
        }
    }
}