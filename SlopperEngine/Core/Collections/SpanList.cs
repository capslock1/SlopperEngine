
namespace SlopperEngine.Core.Collections;

/// <summary>
/// A list for writing spans into directly.
/// </summary>
public class SpanList<T> 
{
    T[] _values;
    int _usedSize;
    
    public SpanList(uint startCapacity = 4)
    {
        _values = new T[startCapacity];
    }

    public int Count => _usedSize;
    public Span<T> AllValues => new(_values, 0, _usedSize);
    public int Capacity 
    {
        get => _values.Length;
        set 
        {
            if(value < _usedSize)
                throw new ArgumentOutOfRangeException("Setting capacity below Count is not allowed!");
            
            if(value == _usedSize || value < 5) return;

            T[] newArray = new T[value];
            if(_usedSize > 0)
                Array.Copy(_values, newArray, _usedSize);
            _values = newArray;
        }
    }
    public T this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }

    /// <summary>
    /// Reserves a span of T and adds it to the list.
    /// </summary>
    /// <param name="lengthToAdd">The amount of elements to add.</param>
    /// <returns>A span for the caller to write into.</returns>
    public Span<T> Add(int lengthToAdd)
    {
        int currentCount = _usedSize;
        _usedSize += lengthToAdd;
        EnsureCapacity(_usedSize);
        return new(_values, currentCount, lengthToAdd);
    }


    /// <summary>
    /// Ensures that a given capacity is available in the SpanList.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        // implementation was shamelessly stolen from System.Collections.Generic.List
        int newCapacity = 2 * _values.Length;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < capacity) newCapacity = capacity;

        Capacity = newCapacity;
    }
}