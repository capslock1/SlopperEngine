using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SlopperEngine.Engine.Collections;

/// <summary>
/// A high-speed dictionary, intended to sporadically have content added.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class FridgeDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    Dictionary<TKey, TValue> _dynamic = new();
    FrozenDictionary<TKey, TValue> _frozen = FrozenDictionary<TKey, TValue>.Empty;
    bool _frozenUpToDate = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void updateFrozen()
    {
        if(_frozenUpToDate) return;
        _frozen = _dynamic.ToFrozenDictionary();
        _frozenUpToDate = true;
    }

    public TValue this[TKey key] 
    { 
        get{
            updateFrozen();
            return _frozen[key];
        } 
        set {
            _frozenUpToDate = false;
            _dynamic[key] = value;        
        } 
    }

    public ICollection<TKey> Keys {
        get{updateFrozen(); return _frozen.Keys;}
    }

    public ICollection<TValue> Values {
        get{updateFrozen(); return _frozen.Values;}
    }

    public int Count {
        get{updateFrozen(); return _frozen.Count;}
    }

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public bool IsSynchronized => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    public void Add(TKey key, TValue value)
    {
        _frozenUpToDate = false;
        _dynamic.Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        throw new NotImplementedException();
    }
    public void Clear()
    {
        _frozen = FrozenDictionary<TKey, TValue>.Empty;
        _dynamic.Clear();
        _frozenUpToDate = true;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        updateFrozen();
        return _frozen.Contains(item);
    }

    public bool ContainsKey(TKey key)
    {
        updateFrozen();
        return _frozen.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        updateFrozen();
        _frozen.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        updateFrozen();
        return _frozen.GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        bool res = _dynamic.Remove(key);
        if(res) _frozenUpToDate = false;
        return res;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {        
        throw new NotImplementedException();
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        updateFrozen();
        return _frozen.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}