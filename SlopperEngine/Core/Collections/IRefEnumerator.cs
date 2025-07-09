namespace SlopperEngine.Core.Collections;

/// <summary>
/// Enumerates over an IRefEnumerable.
/// </summary>
public interface IRefEnumerator<T>
{
    /// <summary>
    /// When implemented, may be called by IRefEnumerable for every T in the collection.
    /// </summary>
    public void Next(ref T value);
}