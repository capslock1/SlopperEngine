using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.SceneObjects;

public partial class SceneObject
{
    /// <summary>
    /// Base class for child containers.
    /// </summary>
    public abstract class ChildContainer
    {
        /// <summary>
        /// The owner of this ChildContainer. SceneObjects added to this container will be children of the owner.
        /// </summary>
        public SceneObject Owner { get; }

        /// <summary>
        /// The amount of children this ChildContainer holds.
        /// </summary>
        public abstract int Count { get; }

        bool _currentlyRegistered;

        public ChildContainer(SceneObject owner)
        {
            Owner = owner;
            _currentlyRegistered = owner.InScene;
            Init();
        }

        void Init()
        {
            Owner._childContainers ??= new(1);
            Owner._childContainers.Add(this);
        }

        [OnSerialize]
        void OnSerialize(OnSerializeArgs serializer)
        {
            if (serializer.IsReader) return;

            Init();
            SetAllChildrensListIndex();
        }

        public void CheckRegistered()
        {
            if (Owner.InScene == _currentlyRegistered) return;

            _currentlyRegistered = Owner.InScene;
            if (_currentlyRegistered)
            {
                int c = Count;
                for (int i = 0; i < c; i++)
                    GetByIndex(i).Register(Owner.Scene!);
            }
            else
            {
                int c = Count;
                for (int i = 0; i < c; i++)
                    GetByIndex(i).Unregister();
            }
        }

        /// <summary>
        /// Gets a child by index.
        /// </summary>
        public abstract SceneObject GetByIndex(int index);

        /// <summary>
        /// Removes a child by index.
        /// </summary>
        public abstract void RemoveByIndex(int index);

        /// <summary>
        /// Adds a child to the container. Avoid using in favour of container specific functions.
        /// </summary>
        /// <returns>Whether or not the object was added.</returns>
        public abstract bool TryAdd(SceneObject obj);

        /// <summary>
        /// Sets the child's parentListIndex.
        /// </summary>
        protected void SetChildListIndex(SceneObject child, int index)
        {
            child._parentListIndex = index;
            child._parentList = this;
        }

        /// <summary>
        /// Sets the child's parentListIndex to -1, and its parentList to null.
        /// </summary>
        protected void UnsetChildListIndex(SceneObject child)
        {
            child._parentListIndex = -1;
            child._parentList = null;
        }

        /// <summary>
        /// Unregisters the child if it is registered.
        /// </summary>
        protected void TryUnregister(SceneObject child)
        {
            if (child.Scene is not null)
                child.Unregister();
        }

        /// <summary>
        /// Registers the child if this container is registered.
        /// </summary>
        protected void TryRegister(SceneObject child)
        {
            if (_currentlyRegistered)
                child.Register(Owner.Scene!);
        }

        /// <summary>
        /// Sets the list index for all children.
        /// </summary>
        protected abstract void SetAllChildrensListIndex();
    }
}