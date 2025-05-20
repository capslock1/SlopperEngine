using System.Collections.ObjectModel;
using System.Reflection;

namespace SlopperEngine.Core.Serialization;

/// <summary>
/// Represents information about a type that has gone through the serialization process.
/// </summary>
public record struct SerializedTypeInfo(
    /// <summary>
    /// The type being represented.
    /// </summary>
    Type Type,
    /// <summary>
    /// The fields of this type. Null if it could not be found.
    /// </summary>
    ReadOnlyCollection<FieldInfo?> Fields,
    /// <summary>
    /// The "OnSerialize" methods of the type. Null if it could not be found.
    /// </summary>
    ReadOnlyCollection<MethodInfo?> OnSerializeMethods)
{}