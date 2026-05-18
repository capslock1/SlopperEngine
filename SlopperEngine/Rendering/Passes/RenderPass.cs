using SlopperEngine.Graphics.ShadingLanguage;
using System.CodeDom.Compiler;

namespace SlopperEngine.Rendering.Passes;

/// <summary>
/// Represents a specific way a SlopperShader can be compiled - what it outputs (depth, color, debug etc) and what its inputs are, and how they are laid out. Equal instances must produce equal code!
/// </summary>
public abstract record class RenderPass
{
    /// <summary>
    /// Writes the vertex shader's main() function. Make sure the shader code does not depend on ANY mutable members of the program!
    /// </summary>
    public abstract void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer);
    /// <summary>
    /// Writes the fragment shader's main() function. Make sure the shader code does not depend on ANY mutable members of the program!
    /// </summary>
    public abstract void AddFragmentMain(SyntaxTree scope, IndentedTextWriter writer);
}