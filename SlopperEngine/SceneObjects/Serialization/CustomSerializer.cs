
namespace SlopperEngine.SceneObjects.Serialization;

partial class SerializedObjectTree
{
    /// <summary>
    /// Serializes objects.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="writes"></param>
    public ref struct CustomSerializer
    {
        /// <summary>
        /// True if this Serializer writes to given values (overwrites the ref).
        /// </summary>
        public bool IsWriter => _writes;

        /// <summary>
        /// True if this Serializer reads from given values (leaves the ref intact).
        /// </summary>
        public bool IsReader => !_writes;
        
        bool _writes;

        readonly ref List<object?>? _target;

        int _serializedObjectsIndex;
        int _serializedObjectsCount;
        Dictionary<int, object?>? _deserializedObjects;
        SerializedObjectTree? _tree;
        ref int _currentIndex;
        
        public CustomSerializer(int objectsIndex, int count, Dictionary<int, object?> deserializedObjects, SerializedObjectTree tree, ref int referenceInt)
        {
            _serializedObjectsIndex = objectsIndex;
            _serializedObjectsCount = count;
            _deserializedObjects = deserializedObjects;
            _tree = tree;
            _writes = true;
            _currentIndex = ref referenceInt;
            _currentIndex = 0;
        }

        public CustomSerializer(ref List<object?>? target)
        {
            _target = ref target;
            _writes = false;
        }

        /// <summary>
        /// Serializes a given value.
        /// </summary>
        /// <param name="Value">The value to serialize.</param>
        public void Serialize<T>(ref T Value)
        {
            if(_writes)
            {
                // write to refs
                if(_currentIndex >= _serializedObjectsCount)
                    throw new Exception("Attempted to read back more values than were previously written.");

                var val = _tree!.RecursiveDeserialize(_serializedObjectsIndex + _currentIndex, _deserializedObjects!);
                if(val != null) Value = (T)val;
                _currentIndex++;
            }
            else
            {
                // read from refs
                _target ??= new();
                _target.Add(Value);
            }
        }
    }
}