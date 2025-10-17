using System.Collections.Generic;
using SlopperEngine.Core.Collections;

namespace SlopperEngine.Core.SceneData;

/// <summary>
/// Stores all data in a consistent order, but data may be inserted in between.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SparseSemiorderedSceneDataContainer<T> : SceneDataContainer<T>
{
    readonly List<(SceneDataHandle index, T toAdd, bool removes)> _addRemoveQueue = [];
    readonly SpanList<(T, DataState)> _data = new();
    int _earliestFreeIndex = -1;

    public override SceneDataHandle QueueAdd(T data)
    {
        var res = ReserveSpace();
        _addRemoveQueue.Add((res, data, false));
        return res;
    }

    public override void QueueRemove(SceneDataHandle handle, T _)
    {
        _addRemoveQueue.Add((handle, default!, true));
    }

    SceneDataHandle ReserveSpace()
    {
        if(_earliestFreeIndex < 0)
        {
            //no gaps - add a new space to the list, reserve it
            _data.Add((default!, DataState.Reserved));
            return new(_data.Count-1);
        }
        else
        {
            //there is a gap - we can fill it
            int index = _earliestFreeIndex;
            _data[index] = (default!, DataState.Reserved);
            _earliestFreeIndex = IndexOfFirstFree(index);
            return new(index);
        }
    }

    int IndexOfFirstFree(int startIndex)
    {
        for (; startIndex < _data.Count; startIndex++)
        {
            if (_data[startIndex].Item2 == DataState.Empty)
                return startIndex;
        }
        return -1;
    }

    void Set(SceneDataHandle handle, T data)
    {
        _data[handle.Index] = (data, DataState.Used);
    }

    void Remove(SceneDataHandle handle)
    {
        _data[handle.Index] = (default!, DataState.Empty); //its nullness is be registered in DataState, which is why its forgiven.

        _earliestFreeIndex = _earliestFreeIndex < 0 ? handle.Index : int.Min(handle.Index, _earliestFreeIndex);
    }
    
    public override void FinalizeQueue()
    {
        for( int i = 0; i<_addRemoveQueue.Count; i++)
        {
            var (index, toAdd, removes) = _addRemoveQueue[i];
            if (removes)
                Remove(index);
            else Set(index, toAdd);
        }
        _addRemoveQueue.Clear();
    }

    public override void Enumerate<TEnumerator>(ref TEnumerator enumerator)
    {
        for (int i = 0; i < _data.Count; i++)
            if (_data[i].Item2 == DataState.Used)
                enumerator.Next(ref _data[i].Item1);
    }

    enum DataState : byte
    {
        Empty = 0,
        Reserved = 0b01,
        Used = 0b11
    }
}