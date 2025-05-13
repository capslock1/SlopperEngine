namespace SlopperEngine.Core.Serialization;

/// <summary>
/// Offers functions to serialize and deserialize objects that are unable to be serialized by their actual value.
/// </summary>
public interface ISerializableFromKey<TKey> : ISerializableFromKeyBase
{
    /// <summary>
    /// Serializes this object into its key.
    /// </summary>
    /// <returns>An object that represents this object.</returns>
    public TKey Serialize();

    /// <summary>
    /// Deserializes an object from its key. The key not being null is NOT garuanteed, even when it's not marked nullable.
    /// </summary>
    /// <param name="key">The key to create the object from.</param>
    /// <returns>A new instance created using the key.</returns>
    public static abstract object? Deserialize(TKey key);
    
    object? ISerializableFromKeyBase.SerializeToObject() => Serialize();
    Type ISerializableFromKeyBase.GetKeyType() => typeof(TKey);
}

/// <summary>
/// Non-generic ISerializableFromKey interface.
/// </summary>
public interface ISerializableFromKeyBase
{
    public object? SerializeToObject();
    public Type GetKeyType();
}