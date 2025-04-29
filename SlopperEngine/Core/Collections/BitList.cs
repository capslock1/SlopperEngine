using System.Collections;
using OpenTK.Mathematics;

namespace SlopperEngine.Core.Collections;

/// <summary>
/// Low memory cost list of bools. Does not support functions that don't make sense, mostly index-based or finding T based functions.
/// </summary>
public class BitList
{
    private readonly BitArray _values = new(32);
    public int Count {get; private set;} = 0;


    public bool this[int index] 
    { 
        get{
            if(outOfBounds(index)) throw new IndexOutOfRangeException();
            return _values[index];
        } 
        set{
            if(outOfBounds(index)) throw new IndexOutOfRangeException();
            _values[index] = value;
        }
    }
    bool outOfBounds(int index) => (uint)index >= (uint)Count;
    public void Add(bool item)
    {
        int count = Count;
        Count++;
        if((uint)count >= (uint)_values.Count)
        {
            _values.Length *= 2;
        }
        _values[count] = item;
    }

    /// <summary>
    /// Gets the index of the first occurrence of a certain value.
    /// </summary>
    /// <param name="value">The value to seek.</param>
    /// <param name="startIndex">The index to start the search from.</param>
    /// <returns></returns>
    public int GetIndexOfFirst(bool value, int startIndex = 0)
    {
        startIndex = int.Max(0, startIndex);

        while(startIndex < Count)
        {
            if(_values[startIndex] == value)
                return startIndex;
            startIndex++;
        }
        return -1;
    }

    public void Clear()
    {
        //no need to clear the array as bools are not reference types.
        Count = 0;
    }
}