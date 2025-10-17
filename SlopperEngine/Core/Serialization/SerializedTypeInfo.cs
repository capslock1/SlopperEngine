using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SlopperEngine.Core.Serialization;

/// <summary>
/// Represents information about a type that has gone through the serialization process.
/// </summary>
/// <param name="Type">The type being represented.</param>
/// <param name="Fields">The fields of this type. Null if it could not be found.</param>
/// <param name="OnSerializeMethods">The OnSerialize" methods of the type. Null if it could not be found.</param>
public record struct SerializedTypeInfo(
    Type Type,
    ReadOnlyCollection<FieldInfo?> Fields,
    ReadOnlyCollection<MethodInfo?> OnSerializeMethods)
{}