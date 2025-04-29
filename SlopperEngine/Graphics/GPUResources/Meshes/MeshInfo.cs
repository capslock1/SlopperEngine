using System.CodeDom.Compiler;
using SlopperEngine.Graphics.ShadingLanguage;

namespace SlopperEngine.Graphics;

/// <summary>
/// Describes the format of a mesh. Should be privately implemented within mesh classes. <br/>
/// Appropriate for use in dictionaries.
/// </summary>
public abstract class MeshInfo
{
    int _id;
    static List<MeshInfo> _allMeshInfos = new();
    protected MeshInfo()
    {
        //assign each unique MeshInfo a unique ID
        //as of now this also prevents them from being GC'd but i assume there wont be so many that this will be an issue
        foreach(var j in _allMeshInfos)
        {
            if(j == this)
            {
                _id = j._id;
                return;
            }
        }
        _id = _allMeshInfos.Count;
        _allMeshInfos.Add(this);
    }

    /// <summary>
    /// Describes the layout of the VAO in GLSL.
    /// </summary>
    /// <returns>A string containing GLSL code, describing the layout and types of the inputs of the vertex shader.</returns>
    public abstract string GLSLGetLayoutBlock();

    /// <summary>
    /// Initializes all vertIn variables within scope, by adding the "vertIn_Initialize()" function.
    /// </summary>
    public abstract void GLSLVertexInitialize(SyntaxTree scope, IndentedTextWriter writer);

    public static bool operator ==(MeshInfo A, MeshInfo B)
    {
        if(A.GetType() == B.GetType()) return A.Equals(B);
        return false;
    }
    public static bool operator !=(MeshInfo A, MeshInfo B) => !(A == B);

    public override bool Equals(object? obj)
    {
        var j = obj as MeshInfo;
        if(j is null) return false;
        return j == this;
    }
    public override int GetHashCode()
    {
        return _id;
    }

    /// <summary>
    /// Internal check for if two meshInfos are equal. other is garuanteed to be of the same type.
    /// </summary>
    /// <param name="other">Garuanteed to be of the same type as this.</param>
    protected abstract bool Equals(MeshInfo other);
}