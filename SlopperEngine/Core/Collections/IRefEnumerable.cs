using System.Collections.Generic;

namespace SlopperEngine.Core.Collections;

/// <summary>
/// A collection that can be enumerated by reference.
/// </summary>
public interface IRefEnumerable<T>
{
    /// <summary>
    /// When implemented, calls Next() on the given enumerator.
    /// </summary>
    /// <param name="enumerator">The enumerator to use.</param>
    public void Enumerate<TEnumerator>(ref TEnumerator enumerator) where TEnumerator : IRefEnumerator<T>, allows ref struct;

    /// <summary>
    /// When implemented, enumerates the IRefEnumerable compatibly with foreach. Likely performs worse.
    /// </summary>
    public IEnumerable<T> EnumerateReadonly();
}