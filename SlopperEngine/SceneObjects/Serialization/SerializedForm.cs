using System.Runtime.CompilerServices;

namespace SlopperEngine.SceneObjects.Serialization;

/// <summary>
/// The serialized form of a SceneObject tree.
/// </summary>
public class SerializedObjectTree
{
    Type _objectType;

    /// <summary>
    /// Serializes a SceneObject.
    /// </summary>
    public SerializedObjectTree(SceneObject toSerialize)
    {
        _objectType = toSerialize.GetType();
    }

    /// <summary>
    /// Instantiates this SerializedObjectTree.
    /// </summary>
    public SceneObject Instantiate()
    {
        SceneObject instance = (SceneObject)RuntimeHelpers.GetUninitializedObject(_objectType);

        return instance;
    }
}