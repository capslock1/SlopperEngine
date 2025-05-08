
namespace SlopperEngine.SceneObjects.Serialization;

partial class SerializedObjectTree
{
    /// <summary>
    /// Serializes objects.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="writes"></param>
    public ref struct CustomSerializer(SerializedObjectTree target, bool writes)
    {
        /// <summary>
        /// True if this Serializer writes to given values (overwrites the ref).
        /// </summary>
        public bool IsWriter => _writes;

        /// <summary>
        /// True if this Serializer reads from given values (leaves the ref intact).
        /// </summary>
        public bool IsReader => !_writes;
        
        SerializedObjectTree _target = target;
        bool _writes = writes;

        /// <summary>
        /// Serializes a given value.
        /// </summary>
        /// <param name="Value">The value to serialize.</param>
        public void Serialize(ref object Value)
        {

        }
    }
}