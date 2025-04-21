namespace SlopperEngine.Engine.Collections;

/// <summary>
/// Can cache arbitrary info that gets GC'd when unused.
/// </summary>
/// <typeparam name="TKey">The type of key for the cache. Usually the filepath, as a string.</typeparam>
/// <typeparam name="TValue">The type of value to cache.</typeparam>
public class Cache<TKey, TValue> 
    where TValue : class 
    where TKey : notnull
{
    readonly private Dictionary<TKey, WeakReference<TValue>> _dict = new();

    /// <summary>
    /// Gets a value from the cache. Returns null if it hasn't been cached, or if it's been too long since last access.
    /// </summary>
    /// <param name="key">The key the value was stored at.</param>
    /// <returns>The value to get.</returns>
    public TValue? Get(TKey key)
    {
        WeakReference<TValue>? refres;
        if (!_dict.TryGetValue(key, out refres))
            return null; 
        if(refres.TryGetTarget(out TValue? res))
            return res;
        
        return null;
    }

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <param name="key">The key to set the value at.</param>
    /// <param name="value">The value to set.</param>
    public void Set(TKey key, TValue value)
    {
        _dict[key] = new WeakReference<TValue>(value);
    }
}