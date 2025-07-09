using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.ChildLists;

namespace SlopperEngine.SceneObjects;

public partial class SceneObject
{
    public class ChildList<TSceneObject> : ChildList<TSceneObject, NoEvents<TSceneObject>> where TSceneObject : SceneObject
    {
        public ChildList(SceneObject owner) : base(owner, default){}
    }

    /// <summary>
    /// Stores a number of children of a particular type.
    /// </summary>
    /// <typeparam name="TSceneObject"></typeparam>
    /// <param name="owner"></param>
    public class ChildList<TSceneObject, TEvents> : IChildList
        where TSceneObject : SceneObject
        where TEvents : IChildListEvents<TSceneObject>
    {
        public SceneObject Owner { get; private set; }
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
        bool _currentlyRegistered;

        public ChildList(SceneObject owner, TEvents events)
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
        void OnSerialize(OnSerializeArgs serializer)
        {
            if (serializer.IsReader) return;

            Init();
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i]._parentList = this;
                _children[i]._parentListIndex = i;
            }
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

            newChild._parentList = this;
            newChild._parentListIndex = _children.Count;
            _children.Add(newChild);

            if (_currentlyRegistered)
                newChild.Register(Owner.Scene!);

            _events.OnChildAdded(newChild, newChild._parentListIndex);
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

        public void Remove(int index)
        {
            var removed = _children[index];
            if (removed.Scene is not null)
                removed.Unregister();

            for (int i = index; i < _children.Count - 1; i++)
            {
                _children[i] = _children[i + 1];
                _children[i]._parentListIndex = i;
            }
            _children.RemoveAt(_children.Count - 1);

            removed._parentListIndex = -1;
            removed._parentList = null;

            _events.OnChildRemoved(removed, index);
        }

        public void CheckRegistered()
        {
            if (Owner.InScene == _currentlyRegistered) return;

            _currentlyRegistered = Owner.InScene;
            if (_currentlyRegistered)
            {
                foreach (var ch in _children)
                    ch.Register(Owner.Scene!);
            }
            else
            {
                foreach (var ch in _children)
                    ch.Unregister();
            }
        }

        public SceneObject Get(int index) => this[index];
    }
}