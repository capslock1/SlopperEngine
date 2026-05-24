using SlopperEngine.Graphics.ShadingLanguage;
using System.CodeDom.Compiler;
using SlopperEngine.Rendering.Lighting;

namespace SlopperEngine.Rendering.Passes;

/// <summary>
/// Render pass used by the DebugRenderer. Has basic phong shading.
/// </summary>
public record class DebugPass : RenderPass
{
    /// <summary>
    /// The singleton DebugPass.
    /// </summary>
    public static readonly DebugPass Instance = new(); 
    private DebugPass(){}

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
        bool writesSpecular = false;
        int normPosWrite = 0;
        foreach (var v in scope.pixOut)
        {
            switch (v.Name)
            {
                case "Albedo": writesAlbedo = true; break;
                case "Transparency": writesAlpha = true; break;
                case "Normal": normPosWrite++; break;
                case "Position": normPosWrite++; break;
                case "Specular": writesSpecular = true; break;
            }
        }
        bool writesNormalAndPosition = normPosWrite == 2;
        writer.Write(LightBuffer.GetGLSLString(writesSpecular));
        writer.Write(
@$"
out vec4 SL_FragColor;

void main()
{{
    pixel();
    vec3 lighting = {(writesNormalAndPosition ? "SL_GetLighting(pixOut.Position, pixOut.Normal)" : "vec3(1.0)")};
    SL_FragColor = vec4({(writesAlbedo ? "pixOut.Albedo" : "vec3(1.0,1.0,1.0)")} * lighting, {(writesAlpha ? "pixOut.Transparency" : "1.0")});
}}"
        );
    }
}