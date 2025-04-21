
using System.Runtime.CompilerServices;

namespace SlopperEngine.SceneObjects;

public static class SceneObjectExtensions
{
    /// <summary>
    /// Whether or not the SceneObject exists - meaning it is neither null nor destroyed. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Exists(this SceneObject? obj)
    {
        if(obj is null) return false;
        return !obj.Destroyed;
    }
}
