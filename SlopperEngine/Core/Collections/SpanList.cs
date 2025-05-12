
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
        set {
            T temp = value;
            _values[index] = temp;
        }
    }

    /// <summary>
    /// Adds a single item to the list.
    /// </summary>
    public void Add(T toAdd)
    {
        EnsureCapacity(_usedSize + 1);
        _values[_usedSize] = toAdd;
        _usedSize++;
    }

    /// <summary>
    /// Reserves a span of T and adds it to the list.
    /// </summary>
    /// <param name="lengthToAdd">The amount of elements to add.</param>
    /// <returns>A span for the caller to write into.</returns>
    public ListSpan AddMultiple(int lengthToAdd)
    {
        int currentCount = _usedSize;
        EnsureCapacity(_usedSize + lengthToAdd);
        _usedSize += lengthToAdd;
        return new(this, currentCount, lengthToAdd);
    }


    /// <summary>
    /// Ensures that a given capacity is available in the SpanList.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        // implementation was shamelessly stolen from System.Collections.Generic.List
        if(capacity <= Capacity) return;
        int newCapacity = 2 * _values.Length;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < capacity) newCapacity = capacity;

        Capacity = newCapacity;
    }

    public ref struct ListSpan(SpanList<T> owner, int startIndex, int length)
    {
        public int Length => _length;

        SpanList<T> _owner = owner;
        int _startIndex = startIndex;
        int _length = length;

        // not checking bounds cuz i dont care honestly
        public ref T this[int index] => ref _owner._values[index + _startIndex];

        /// <summary>
        /// Forms a slice out of the current range that begins at a specified index.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <returns></returns>
        public ListSpan Slice(int start) => new ListSpan(_owner, _startIndex+start, _length-start);

        public static implicit operator Span<T>(ListSpan r) => new(r._owner._values, r._startIndex, r.Length);
    }
}