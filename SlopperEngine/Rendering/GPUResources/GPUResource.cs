using System.Collections.Concurrent;
using SlopperEngine.Windowing;

namespace SlopperEngine.Rendering;

/// <summary>
/// Abstract class to handle resources on the GPU.
/// Automatically cleaned from VRAM if garbage collected.
/// </summary>
public abstract class GPUResource : IDisposable
{
    /// <summary>
    /// Gets called when the GPUResource gets disposed.
    /// </summary>
    public event Action? OnDispose = null;

    //protected GPUResource()
    //{
    //    if(!MainContext.Instance.Context.IsCurrent)
    //        System.Console.WriteLine($"Created a GPUResource ({this}) outside of the main context.");
    //}

    private static ConcurrentQueue<ResourceData> _GCdResources = new();
    private bool _disposedValue = false;

    /// <summary>
    /// Gotten when the class is disposed or garbage collected.
    /// </summary>
    /// <returns>A class containing a function to remove the GPUResource from VRAM.</returns>
    protected abstract ResourceData GetResourceData();
    /// <summary>
    /// Removes any garbage collected GPUResources from VRAM. 
    /// The garbage collector works inconsistently, and should not be relied on over manual disposing.
    /// </summary>
    public static void ClearLostResources()
    {
        if(_GCdResources.IsEmpty)
            return;
        Console.WriteLine($"GPUResource: clearing {_GCdResources.Count} lost GPU resource{(_GCdResources.Count != 1 ? "s":"")}.");
        foreach(var data in _GCdResources)
            data.Clear();
        _GCdResources.Clear();
    }
    
    protected void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        OnDispose?.Invoke();
        GetResourceData().Clear();
        _disposedValue = true;
    }

    /// <summary>
    /// Removes the GPUResource from VRAM.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract class ResourceData
    {
        public abstract void Clear(); 
    }
    ~GPUResource()
    {
        //this should NOT happen
        //but it will be caught!
        if (!_disposedValue)
        {
            Console.WriteLine(ToString() + ": GPU Resource leak! Did you forget to call Dispose()?");
            ResourceData leak = GetResourceData();
            _GCdResources.Enqueue(leak);
        }
    }
}