namespace SlopperEngine.SceneObjects;

public partial class SceneObject
{
    /// <summary>
    /// Stores a single child of a SceneObject.
    /// </summary>
    /// <typeparam name="TSceneObject">The type of SceneObject to store.</typeparam>
    public class SingleChild<TSceneObject> : IChildList where TSceneObject : SceneObject
    {
        public SceneObject Owner {get; private set;}
        public int Count => _child == null ? 0 : 1;

        /// <summary>
        /// The child stored in the SingleChild.
        /// </summary>
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
}