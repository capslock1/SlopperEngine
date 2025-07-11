using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.SceneObjects;

public partial class SceneObject
{
    /// <summary>
    /// Base class for child containers.
    /// </summary>
    public abstract class ChildContainer : IChildContainer
    {
        public SceneObject Owner { get; }

        int IChildContainer.Count => GetCount();
        bool _currentlyRegistered;

        public ChildContainer(SceneObject owner)
        {
            Owner = owner;
            _currentlyRegistered = owner.InScene;
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
            SetAllChildrensListIndex();
        }

        void IChildContainer.CheckRegistered()
        {
            if (Owner.InScene == _currentlyRegistered) return;

            _currentlyRegistered = Owner.InScene;
            if (_currentlyRegistered)
            {
                int c = GetCount();
                for (int i = 0; i < c; i++)
                    GetByIndex(i).Register(Owner.Scene!);
            }
            else
            {
                int c = GetCount();
                for (int i = 0; i < c; i++)
                    GetByIndex(i).Unregister();
            }
        }

        /// <summary>
        /// Gets the amount of children.
        /// </summary>
        /// <returns></returns>
        protected abstract int GetCount();

        /// <summary>
        /// Gets a child by index.
        /// </summary>
        protected abstract SceneObject GetByIndex(int index);
        SceneObject IChildContainer.Get(int index) => GetByIndex(index);

        /// <summary>
        /// Removes a child by index.
        /// </summary>
        protected abstract void RemoveByIndex(int index);
        void IChildContainer.Remove(int index) => RemoveByIndex(index);

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