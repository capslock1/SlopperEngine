using System;
using System.CodeDom.Compiler;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.ShadingLanguage;

namespace SlopperEngine.Graphics.GPUResources.Meshes;

/// <summary>
/// A mesh created from a wavefront .obj file.
/// </summary>
public sealed class OBJMesh : Mesh
{
    static OBJMeshInfo _info = new();

    readonly float[] _vertices;
    readonly uint[] _indices;
    readonly int _indexCount;

    BufferObject _arrayBuffer;
    BufferObject _elementBuffer;

    public OBJMesh(float[] vertices, uint[] indices)
    {
        _vertices = vertices;
        _indices = indices;
        _indexCount = indices.Length;

        _arrayBuffer = BufferObject.Create(BufferTarget.ArrayBuffer, vertices.AsSpan(), BufferUsageHint.StaticDraw);
        _elementBuffer = BufferObject.Create(BufferTarget.ElementArrayBuffer, indices.AsSpan(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3*sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5*sizeof(float));
        GL.EnableVertexAttribArray(2);
    }

    public override void Draw()
    {
        Use();
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
    }
    public override void DrawInstanced(int count)
    {
        if(count <= 0) return;
        Use();
        GL.DrawElementsInstanced(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0, count);
    }
    public override MeshInfo GetMeshInfo() => _info;
    
    sealed class OBJMeshInfo : MeshInfo
    {
        public override string GLSLGetLayoutBlock()
        {
            return 
@"layout (location = 0) in vec3 Model_Position;
layout (location = 1) in vec2 Model_UVs;
layout (location = 2) in vec3 Model_Normal;";
        }

        public override void GLSLVertexInitialize(SyntaxTree Scope, IndentedTextWriter writer)
        {
            writer.WriteLine("void vertIn_Initialize(){");
            foreach(var j in Scope.vertIn)
            {
                if(j.Name == "position")
                {
                    writer.WriteLine("    vertIn.position = vec4(Model_Position, 1.0);");
                    continue;
                }
                if(j.Name == "UVCoordinates")
                {
                    writer.WriteLine("    vertIn.UVCoordinates = Model_UVs;");
                    continue;
                }
                if(j.Name == "normal")
                {
                    writer.WriteLine("    vertIn.normal = Model_Normal;");
                    continue;
                }
            }
            writer.WriteLine("}");
        }

        //this doesnt have to check anything because all OBJMeshInfos are equal, and other is an OBJMeshInfo
        protected override bool Equals(MeshInfo other) => true;
    }

    protected override IGPUResourceOrigin GetOrigin() => new OBJMeshOrigin(this);
    private class OBJMeshOrigin(OBJMesh target) : IGPUResourceOrigin
    {
        float[] vertices = target._vertices;
        uint[] indices = target._indices;
        public GPUResource CreateResource() => new OBJMesh(vertices, indices);
    }
    protected override ResourceData GetResourceData()
    {
        return new OBJMeshResourceData(base.GetResourceData(), _arrayBuffer, _elementBuffer);
    }
    sealed class OBJMeshResourceData(ResourceData baseData, BufferObject arrayBuffer, BufferObject elementBuffer) : ResourceData
    {
        readonly ResourceData _baseData = baseData;
        readonly BufferObject _arrayBuffer = arrayBuffer;
        readonly BufferObject _elementBuffer = elementBuffer;
        public override void Clear()
        {
            _baseData.Clear();
            _arrayBuffer.Dispose();
            _elementBuffer.Dispose();
        }
    }
}