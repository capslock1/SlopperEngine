using System.ComponentModel.Design;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SlopperEngine.Graphics;

/// <summary>
/// Abstract class that stores a mesh on the GPU side.
/// </summary>
public abstract class Mesh : GPUResource
{
    static int _lastBoundVAO;
    public readonly int VertexArrayObject;

    protected Mesh()
    {
        VertexArrayObject = GL.GenVertexArray(); //stores the format
        Use();
    }
    
    /// <summary>
    /// Draws a single instance of the mesh using the currently bound shader.
    /// </summary>
    abstract public void Draw();
    /// <summary>
    /// Draws several instances of the mesh using the currently bound shader.
    /// </summary>
    /// <param name="count">The amount of instaces to draw.</param>
    abstract public void DrawInstanced(int count);

    /// <summary>
    /// Gets necessary information about the mesh for in-shader use.
    /// </summary>
    abstract public MeshInfo GetMeshInfo();

    /// <summary>
    /// Binds the VAO.
    /// </summary>
    public void Use()
    {
        if(VertexArrayObject != _lastBoundVAO) GL.BindVertexArray(VertexArrayObject);
        _lastBoundVAO = VertexArrayObject;
    }
    /// <summary>
    /// Unbinds the VAO. 
    /// </summary>
    public static void Unuse(){
        GL.BindVertexArray(0);
        _lastBoundVAO = 0;
    }
    
    protected override ResourceData GetResourceData() => new MeshResourceData(VertexArrayObject);
    protected class MeshResourceData(int VAO) : ResourceData
    {
        public int VAO = VAO;
        public override void Clear()
        {
            GL.DeleteBuffer(VAO);
        }
    } 
}
