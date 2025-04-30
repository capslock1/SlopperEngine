using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using SlopperEngine.Core;

namespace SlopperEngine.SceneObjects.Serialization;

/// <summary>
/// The serialized form of a SceneObject tree.
/// </summary>
public class SerializedObjectTree
{
    Type _objectType;
    ReadOnlyCollection<FieldInfo> _fields;
    object?[]? _fieldValues;

    /// <summary>
    /// Serializes a SceneObject. Should only be called by SceneObject.Serialize().
    /// </summary>
    internal SerializedObjectTree(SceneObject toSerialize)
    {
        if(toSerialize.InScene) 
            throw new Exception("SceneObject was in the scene while being serialized - call SceneObject.Serialize() to properly serialize it.");
        _objectType = toSerialize.GetType();
        _fields = SceneObjectReflectionCache.GetSerializableFields(_objectType);
        _fieldValues = new object[_fields.Count];
        for(int i = 0; i<_fields.Count; i++)
        {
            _fieldValues[i] = _fields[i].GetValue(toSerialize);
            System.Console.WriteLine($"{_fields[i].Name}, {_fieldValues[i]}");
        }
    }

    /// <summary>
    /// Instantiates this SerializedObjectTree.
    /// </summary>
    public SceneObject Instantiate()
    {
        if(_fieldValues == null)
            throw new Exception("SerializedObjectTree was uninitialized and could not instantiate.");
        
        SceneObject instance = (SceneObject)RuntimeHelpers.GetUninitializedObject(_objectType);
        for(int i = 0; i<_fields.Count; i++)
            _fields[i].SetValue(instance, _fieldValues[i]);

        return instance;
    }
}