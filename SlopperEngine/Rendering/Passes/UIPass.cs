using SlopperEngine.Graphics.ShadingLanguage;
using System.CodeDom.Compiler;

namespace SlopperEngine.Rendering.Passes;

/// <summary>
/// Render pass used by the UIRenderer.
/// </summary>
public record class UIPass : RenderPass
{
    public static readonly UIPass Instance = new();

    private UIPass(){}

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
        bool writesAlbedo = false;
        bool writesAlpha = false;
        foreach (var v in scope.pixOut)
        {
            if (v.Name == "Albedo")
                writesAlbedo = true;
            if (v.Name == "Transparency")
                writesAlpha = true;
        }
        writer.Write(
@$"
out vec4 SL_FragColor;
void main()
{{
    pixel();
    SL_FragColor = vec4({(writesAlbedo ? "pixOut.Albedo" : "1.0,1.0,1.0")},{(writesAlpha ? "pixOut.Transparency" : "1.0")});
}}"
        );
    }
}