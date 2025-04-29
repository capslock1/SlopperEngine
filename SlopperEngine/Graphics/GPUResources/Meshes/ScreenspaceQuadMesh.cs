using System.CodeDom.Compiler;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.ShadingLanguage;

namespace SlopperEngine.Graphics;

/// <summary>
/// Represents a quad that lives in screenspace. Does not have a third dimension.
/// </summary>
public sealed class ScreenspaceQuadMesh : Mesh
{
    readonly BufferObject _arrayBuffer;
    readonly float[] _pointValues = new float[6*4];

    static readonly MeshInfo _info = new ScreenspaceQuadMeshInfo();

    public ScreenspaceQuadMesh(Box2 shape)
    {
        SetPointValues(shape);
        _arrayBuffer = BufferObject.Create(BufferTarget.ArrayBuffer, _pointValues.AsSpan(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    /// <summary>
    /// Sets the shape of the ScreenspaceQuadMesh.
    /// </summary>
    /// <param name="shape">The shape to set in normalized device coordinates.</param>
    public void SetShape(Box2 shape)
    {
        SetPointValues(shape);
        _arrayBuffer.SetData(_pointValues.AsSpan(), 0);
    }

    void SetPointValues(Box2 shape)
    {
        //generating quads is my passion
        setIndividual(0, true, true);
        setIndividual(4, true, false);
        setIndividual(8, false, true);
        setIndividual(12, true, false);
        setIndividual(16, false, true);
        setIndividual(20, false, false);

        void setIndividual(int totindex, bool minX, bool minY)
        {
            _pointValues[0+totindex] = minX ? shape.Min.X : shape.Max.X;
            _pointValues[1+totindex] = minY ? shape.Min.Y : shape.Max.Y;
            _pointValues[2+totindex] = minX ? 0 : 1;
            _pointValues[3+totindex] = minY ? 0 : 1;
        }
    }

    public override void Draw()
    {
        Use();
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public override void DrawInstanced(int count)
    {
        if(count <= 0) return;
        Use();
        GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, count);
    }

    sealed class ScreenspaceQuadMeshInfo : MeshInfo
    {
        public override string GLSLGetLayoutBlock()
        {
            return "layout (location = 0) in vec4 Model_vertDat;";
        }

        public override void GLSLVertexInitialize(SyntaxTree scope, IndentedTextWriter writer)
        {
            writer.WriteLine("void vertIn_Initialize(){");
            foreach(var j in scope.vertIn)
            {
                if(j.Name == "position")
                {
                    writer.WriteLine("    vertIn.position = vec4(Model_vertDat.xy,0,1);");
                    continue;
                }
                if(j.Name == "UVCoordinates")
                {
                    writer.WriteLine("    vertIn.UVCoordinates = Model_vertDat.zw;");
                    continue;
                }
            }
            writer.WriteLine("}");
        }

        //see OBJMeshInfo
        protected override bool Equals(MeshInfo other) => true;
    }
    public override MeshInfo GetMeshInfo() => _info;

    protected override ResourceData GetResourceData() => new SSQMResourceData(base.GetResourceData(), _arrayBuffer);
    class SSQMResourceData(ResourceData parent, BufferObject buffer) : ResourceData
    {
        readonly ResourceData _parent = parent;
        readonly BufferObject _arrayBuffer = buffer;
        public override void Clear()
        {
            _parent.Clear();
            _arrayBuffer.Dispose();
        }
    }
}