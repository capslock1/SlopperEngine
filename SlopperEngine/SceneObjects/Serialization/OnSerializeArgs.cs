
namespace SlopperEngine.SceneObjects.Serialization;

/// <summary>
/// Serializes objects.
/// </summary>
public ref struct OnSerializeArgs
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
    SerializedObject _tree;

    readonly ref List<object?>? _target;

    int _serializedObjectsIndex;
    int _serializedObjectsCount;
    Dictionary<int, object?>? _deserializedObjects;
    ref int _currentIndex;
    SerializedObject.SerializationToken _token;

    public OnSerializeArgs(int objectsIndex, int count, Dictionary<int, object?> deserializedObjects, SerializedObject tree, ref int referenceInt, SerializedObject.SerializationToken token)
    {
        _serializedObjectsIndex = objectsIndex;
        _serializedObjectsCount = count;
        _deserializedObjects = deserializedObjects;
        _tree = tree;
        _writes = true;
        _currentIndex = ref referenceInt;
        _currentIndex = 0;
        _token = token;
    }

    public OnSerializeArgs(ref List<object?>? target, SerializedObject tree)
    {
        _target = ref target;
        _writes = false;
        _tree = tree;
    }

    /// <summary>
    /// Serializes a given value.
    /// </summary>
    /// <param name="Value">The value to serialize.</param>
    public void Serialize<T>(ref T Value)
    {
        if (_writes)
        {
            // write to refs
            if (_currentIndex >= _serializedObjectsCount)
                throw new Exception("Attempted to read back more values than were previously written.");

            var val = _token.RecursiveDeserialize(_serializedObjectsIndex + _currentIndex, _deserializedObjects!);
            if (val != null) Value = (T)val;
            _currentIndex++;
        }
        else
        {
            // read from refs
            _target ??= new();
            _target.Add(Value);
        }
    }

    /// <summary>
    /// Calls the method after serialization / deserialization finishes. It's safe to interact with objects higher in the tree after this.
    /// </summary>
    /// <param name="methodToCall">The method to have called after the entire tree is serialized.</param>
    public void CallAfterSerialize(Action methodToCall)
    {
        _tree.CallAfterSerialize(methodToCall);
    }
}