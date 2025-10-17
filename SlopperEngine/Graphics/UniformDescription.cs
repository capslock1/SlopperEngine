using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics;

/// <summary>
/// Describes a shader's uniform. Also has a value field for some reason. This feels like an oversight.
/// </summary>
public struct UniformDescription : IEquatable<UniformDescription>
{
    public readonly ActiveUniformType Type;
    public readonly int SizeBytes;
    public readonly string Name;
    public readonly int Location;
    public object? Value;
    public UniformDescription(ActiveUniformType type, int sizeBytes, string name, int location)
    {
        Type = type;
        SizeBytes = sizeBytes;
        Name = name;
        Location = location;
    }

    public bool Equals(UniformDescription other) => Type == other.Type && SizeBytes == other.SizeBytes && Location == other.Location && Name == other.Name && Value == other.Value;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is UniformDescription desc && desc.Equals(this);
    public override int GetHashCode() => (Type, SizeBytes, Location, Value, Name).GetHashCode();
}