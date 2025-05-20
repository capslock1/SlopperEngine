using System.Collections.ObjectModel;
using System.Reflection;

namespace SlopperEngine.Core.Serialization;

/// <summary>
/// Represents information about a type that has gone through the serialization process.
/// </summary>
public record struct SerializedTypeInfo(Type type, ReadOnlyCollection<FieldInfo?> fields, ReadOnlyCollection<MethodInfo?> onSerializeMethods)
{
    /// <summary>
    /// The type being represented.
    /// </summary>
    public Type Type = type;

    /// <summary>
    /// The fields of this type. Null if it could not be found.
    /// </summary>
    public ReadOnlyCollection<FieldInfo?> Fields = fields;

    /// <summary>
    /// The "OnSerialize" methods of the type. Null if it could not be found.
    /// </summary>
    public ReadOnlyCollection<MethodInfo?> OnSerializeMethods = onSerializeMethods;
}