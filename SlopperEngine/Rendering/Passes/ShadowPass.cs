using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.Rendering.Lighting;
using System.CodeDom.Compiler;

namespace SlopperEngine.Rendering.Passes;

/// <summary>
/// RenderPass that only outputs depth.
/// </summary>
public record class ShadowPass(bool InfiniteShadowpass) : RenderPass
{
    /// <summary>
    /// The singleton DebugPass.
    /// </summary>
    public static readonly ShadowPass Instance = new(false);
     
    public static readonly ShadowPass InstanceInfinite = new(true); 

    public override void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        writer.Write(InfiniteMappedShadow.GLSLInfiniteMapFunction);
        writer.Write(
@"void main()
{
    vertIn_Initialize();
    vertex();");
        if(InfiniteShadowpass)
            writer.Write("vertOut.position.xy = SL_ShadowInfiniteMap(vertOut.position.xy);");
        writer.Write("gl_Position = vertOut.position; }");
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