using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SlopperEngine.Graphics.GPUResources;

/// <summary>
/// A GPU-side buffer containing arbitrary data. 
/// Writeability depends on the BufferTarget, but is generally only CPU-side.
/// </summary>
public class BufferObject : GPUResource
{
    public readonly int Handle;
    public readonly int Size;
    public readonly BufferTarget BufferType;
    
    private BufferObject(int size, BufferTarget type)
    {
        Size = size;
        Handle = GL.GenBuffer();
        BufferType = type;
        GL.BindBuffer(BufferType, Handle);
    }

    /// <summary>
    /// Creates a new BufferObject initialized with a specific type of data.
    /// </summary>
    /// <typeparam name="T">The type of data to fill the buffer with.</typeparam>
    /// <param name="type">The type of buffer object.</param>
    /// <param name="data">The data to store.</param>
    /// <param name="usage">Optionally, the usage hint.</param>
    public static BufferObject Create<T>(BufferTarget type, Span<T> data, BufferUsageHint usage = BufferUsageHint.DynamicDraw) where T : struct
    {
        int size = Unsafe.SizeOf<T>()*data.Length;
        var res = new BufferObject(size, type);
        if(!data.IsEmpty) //why not just let you create an empty ass buffer. why not
            GL.BufferData(type, size, ref data[0], usage);
        return res;
    }

    /// <summary>
    /// Creates a new BufferObject initialized with no data.
    /// </summary>
    /// <param name="type">The type of the buffer object.</param>
    /// <param name="size">The size of the buffer in bytes.</param>
    /// <param name="usage">Optionally, the usage hint.</param>
    public static BufferObject Create(BufferTarget type, int size, BufferUsageHint usage = BufferUsageHint.DynamicDraw) 
    {
        var res = new BufferObject(size, type);
        GL.BufferData(type, size, 0, usage);
        return res;
    }

    /// <summary>
    /// Uses this BufferObject for CPU-Side operations.
    /// </summary>
    public void Use()
    {
        GL.BindBuffer(BufferType, Handle);
    }
    
    /// <summary>
    /// Uses this BufferObject for GPU-side reading.
    /// </summary>
    /// <param name="index"></param>
    public void Bind(int index)
    {
        GL.BindBufferBase((BufferRangeTarget)BufferType, index, Handle);
    }

    /// <summary>
    /// Writes a span to the buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to write.</typeparam>
    /// <param name="data">The data to write.</param>
    /// <param name="index">The byte offset to write at.</param>
    public void SetData<T>(Span<T> data, int index) where T : struct
    {
        if(data.Length < 1) return;
        Use();
        GL.BufferSubData(BufferType, index, data.Length * Unsafe.SizeOf<T>(), ref data[0]);
    }
    
    /// <summary>
    /// Writes an int to the buffer.
    /// </summary>
    /// <param name="data">Int to set.</param>
    /// <param name="index">Byte offset to set the int at.</param>
    public void SetData(int data, int index)
    {
        Use();
        GL.BufferSubData(BufferType, index, sizeof(int), ref data);
    }
    
    /// <summary>
    /// Writes a float to the buffer.
    /// </summary>
    /// <param name="data">Float to set.</param>
    /// <param name="index">Byte offset to set the float at.</param>
    public void SetData(float data, int index)
    {
        Use();
        GL.BufferSubData(BufferType, index, sizeof(float), ref data);
    }
    
    /// <summary>
    /// Writes a 2D vector to the buffer.
    /// </summary>
    /// <param name="data">Vector to set.</param>
    /// <param name="index">Byte offset to set the vector at.</param>
    public void SetData(Vector2 data, int index)
    {
        Use();
        GL.BufferSubData(BufferType, index, Unsafe.SizeOf<Vector2>(), ref data);
    }
    
    /// <summary>
    /// Writes a 3D vector to the buffer.
    /// </summary>
    /// <param name="data">Vector to set.</param>
    /// <param name="index">Byte offset to set the vector at.</param>
    public void SetData(Vector3 data, int index)
    {
        Use();
        GL.BufferSubData(BufferType, index, Unsafe.SizeOf<Vector3>(), ref data);
    }
    
    /// <summary>
    /// Writes a 4D vector to the buffer.
    /// </summary>
    /// <param name="data">Vector to set.</param>
    /// <param name="index">Byte offset to set the vector at.</param>
    public void SetData(Vector4 data, int index)
    {
        Use();
        GL.BufferSubData(BufferType, index, Unsafe.SizeOf<Vector4>(), ref data);
    }
    
    /// <summary>
    /// Writes a 2D matrix to the buffer.
    /// </summary>
    /// <param name="data">Matrix to set.</param>
    /// <param name="index">Byte offset to set the matrix at.</param>
    public void SetData(Matrix2 data, int index)
    {
        Use();
        data.Transpose();
        GL.BufferSubData(BufferType, index, Unsafe.SizeOf<Matrix2>(), ref data);
    }
    
    /// <summary>
    /// Writes a 3D matrix to the buffer.
    /// </summary>
    /// <param name="data">Matrix to set.</param>
    /// <param name="index">Byte offset to set the matrix at.</param>
    public void SetData(Matrix3 data, int index)
    {
        Use();
        data.Transpose();
        GL.BufferSubData(BufferType, index, Unsafe.SizeOf<Matrix3>(), ref data);
    }

    /// <summary>
    /// Writes a 4D matrix to the buffer.
    /// </summary>
    /// <param name="data">Matrix to set.</param>
    /// <param name="index">Byte offset to set the matrix at.</param>
    public void SetData(Matrix4 data, int index)
    {
        Use();
        data.Transpose();
        GL.BufferSubData(BufferType, index, Unsafe.SizeOf<Matrix4>(), ref data);
    }
    
    protected override ResourceData GetResourceData() => new BOResourceData(){ubo = this.Handle};

    protected override IGPUResourceOrigin GetOrigin() => new BufferObjectOrigin(Size, BufferType);
    protected class BufferObjectOrigin(int size, BufferTarget type) : IGPUResourceOrigin
    {
        int size = size;
        BufferTarget type = type;
        public GPUResource CreateResource() => Create(type, size);
    }

    protected class BOResourceData : ResourceData
    {
        public int ubo;
        public override void Clear()
        {
            GL.DeleteBuffer(ubo);
        }
    } 
}