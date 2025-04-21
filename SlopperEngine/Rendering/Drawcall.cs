using System.Diagnostics.CodeAnalysis;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Rendering;

/// <summary>
/// Contains basic information about a drawcall.
/// </summary>
public struct Drawcall : IEquatable<Drawcall>
{
    public SceneObject Owner;
    public Mesh Model;
    public Material Material;
    public Drawcall(SceneObject owner, Mesh model, Material material)
    {
        Owner = owner;
        Model = model;
        Material = material;
    }

    public bool Equals(Drawcall other) => other.Owner == Owner && other.Model == Model && other.Material == Material;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Drawcall d && d.Equals(this);
    public override int GetHashCode() => (Owner, Model, Material).GetHashCode();
}