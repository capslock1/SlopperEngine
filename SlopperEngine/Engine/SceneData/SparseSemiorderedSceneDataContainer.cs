using System.Collections;
using SlopperEngine.Engine.Collections;

namespace SlopperEngine.Engine.SceneData;

/// <summary>
/// Stores all data in a consistent order, but data may be inserted in between.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SparseSemiorderedSceneDataContainer<T> : SceneDataContainer<T>
{
    readonly List<(SceneDataHandle index, T toAdd, bool removes)> _addRemoveQueue = [];
    readonly List<T> _data = [];
    readonly List<DataState> _activeData = [];
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
            _data.Add(default!);
            _activeData.Add(DataState.Reserved);
            return new(_data.Count-1);
        }
        else
        {
            //there is a gap - we can fill it
            int index = _earliestFreeIndex;
            _data[index] = default!;
            _activeData[index] = DataState.Reserved;
            _earliestFreeIndex = _activeData.IndexOf(DataState.Empty, index);
            return new(index);
        }
    }

    void Set(SceneDataHandle handle, T data)
    {
        _data[handle.Index] = data;
        _activeData[handle.Index] = DataState.Used;
    }

    void Remove(SceneDataHandle handle)
    {
        _data[handle.Index] = default!; //its nullness will be registered in activeData, which is why its forgiven.
        _activeData[handle.Index] = DataState.Empty;

        _earliestFreeIndex = _earliestFreeIndex < 0 ? handle.Index : int.Min(handle.Index, _earliestFreeIndex);
    }
    
    public override void FinalizeQueue()
    {
        foreach(var (index, toAdd, removes) in _addRemoveQueue)
        {
            if(removes)
                Remove(index);
            else Set(index, toAdd);
        }
        _addRemoveQueue.Clear();
    }
    
    public override IEnumerator<T> GetEnumerator() => new SSSDCEnumerator(this);

    enum DataState : byte
    {
        Empty = 0,
        Reserved = 0b01,
        Used = 0b11
    }

    private struct SSSDCEnumerator : IEnumerator<T>
    {
        SparseSemiorderedSceneDataContainer<T> values;
        int currentIndex = -1;
        public T Current => values._data[currentIndex];

        object IEnumerator.Current => Current!;

        public SSSDCEnumerator(SparseSemiorderedSceneDataContainer<T> values)
        {
            currentIndex = -1;
            this.values = values;
        }

        public void Dispose(){}

        public bool MoveNext()
        {
            currentIndex++;
            while(currentIndex < values._data.Count)
            {
                if(values._activeData[currentIndex] == DataState.Used)
                    return true;

                currentIndex++;
            }
            return false;
        }

        public void Reset()
        {
            currentIndex = -1;
        }
    }
}