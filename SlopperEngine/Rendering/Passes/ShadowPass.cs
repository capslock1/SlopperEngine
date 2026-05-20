using SlopperEngine.Graphics.ShadingLanguage;
using System.CodeDom.Compiler;

namespace SlopperEngine.Rendering.Passes;

/// <summary>
/// RenderPass that only outputs depth.
/// </summary>
public record class ShadowPass : RenderPass
{
    /// <summary>
    /// The singleton DebugPass.
    /// </summary>
    public static readonly ShadowPass Instance = new(); 
    private ShadowPass(){}

    public override void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        writer.Write(
@"void main()
{
    vertIn_Initialize();
    vertex();
    gl_Position = vertOut.position;
}"
        );
    }

    public override void AddFragmentMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        bool writesAlpha = false;
        foreach (var v in scope.pixOut)
        {
            switch (v.Name)
            {
                case "Transparency": writesAlpha = true; break;
            }
        }
        writer.Write(
@$"
void main()
{{
    pixel();
    {(writesAlpha ? "if(pixOut.Transparency > 0.5) discard;" : "")}
}}"
        );
    }
}