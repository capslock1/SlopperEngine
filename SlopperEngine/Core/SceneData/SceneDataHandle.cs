
using System;
using System.Diagnostics.CodeAnalysis;

namespace SlopperEngine.Core.SceneData;

/// <summary>
/// Handle for SceneData. 
/// </summary>
public struct SceneDataHandle : IEquatable<SceneDataHandle>
{
    private readonly int _index = 0;
    public int Index => _index-1;
    public bool IsRegistered => _index != 0;
    

    public SceneDataHandle(int index = -1)
    {
        _index = index+1;
    }

    public override string ToString()
    {
        return $"SceneDataHandle {Index}";
    }

    public bool Equals(SceneDataHandle other) => _index == other._index;
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SceneDataHandle h && h.Equals(this);
    public override int GetHashCode() => _index.GetHashCode();
}