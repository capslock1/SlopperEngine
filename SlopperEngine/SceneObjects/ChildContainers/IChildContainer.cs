namespace SlopperEngine.SceneObjects;

public partial class SceneObject
{
    protected interface IChildContainer
    {
        /// <summary>
        /// The owner of this ChildContainer.
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
        /// The amount of children this ChildContainer stores.
        /// </summary>
        public int Count {get;}
        
        /// <summary>
        /// Should be called by the owner when it gets registered/unregistered.
        /// </summary>
        public void CheckRegistered();
    }
}