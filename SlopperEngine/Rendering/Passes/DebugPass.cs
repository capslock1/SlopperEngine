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
        writer.Write(LightBuffer.GLSLString);
        writer.Write(
@$"
out vec4 SL_FragColor;

float SL_PhongLighting(vec3 position, vec3 normal, vec3 cameraDirection, vec3 lightDir)
{{
    float litness = max(dot(normal, lightDir), 0);
{(writesSpecular ?
@"
    vec3 rHatM = reflect(lightDir, normal);

    float spec = 0;
    spec = max(dot(rHatM, cameraDirection), 0);
    spec = pow(spec,20);
    spec *= litness;
    
    litness += 3.*spec;
    " : ' '
)}
    return litness < 0. ? 0. : litness;
}}

vec3 SL_GetLighting(vec3 position, vec3 normal)
{{
    vec3 camDir = normalize(position - Globals.cameraPosition.xyz);
    float ambient = normal.y*.5+1.;
    ambient *= 1.-.5*dot(camDir, normal);

    vec3 lightContribution = vec3(0);
    for(int l = 0; l < SL_lightlights.count; l++)
    {{
        SL_LightData light = SL_lightlights.lights[l];
        bool pointLight = light.colorRange.w > -0.5;
        vec3 lightDir = pointLight ? light.positionSharpness.xyz - position : light.positionSharpness.xyz;

        float distanceDarkening = 1;
        if(pointLight)
        {{
            float lightDist = length(lightDir);
            if(lightDist > light.colorRange.w)
                continue;
            float normLightDist = lightDist / light.colorRange.w;
            lightDir /= lightDist;

            float sq = (light.positionSharpness.w);
            distanceDarkening = (1-normLightDist) / (sq * sq + 1);
        }}
        lightContribution += distanceDarkening * SL_PhongLighting(position, normal, camDir, lightDir) * light.colorRange.xyz;
    }}
    for(int s = 0; s < SL_shadowlights.count; s++)
    {{
        const float normalOffset = 0.002;
        SL_ShadowData shadow = SL_shadowlights.lights[s];
        vec4 projPos = vec4(position + normal * normalOffset, 1.0) * shadow.viewProj;
        float lightMultiplier = 0;
        for(int casc = 0; casc < 4; casc++)
        {{
            float cascadeSize = shadow.cascadeSizes[casc];
            vec2 shadowUV = 0.5 + 0.5 * projPos.xy * cascadeSize;
            if(max(shadowUV.x, shadowUV.y) > 1 || min(shadowUV.x, shadowUV.y) < 0)
                continue;
            
            float shadowDist = texture(SL_ShadowTextures, vec3(shadowUV, shadow.cascadeIndices[casc])).x;
            bool inLight = shadowDist > 0.5 + 0.5 * projPos.z;
            if(!inLight) 
                break;

            lightMultiplier = 1;
        }}
        lightContribution += SL_PhongLighting(position, normal, camDir, -normalize(shadow.viewProj[2].xyz)) * shadow.color.xyz * lightMultiplier;
    }}

    return vec3(0.05,.1,.2)*ambient + lightContribution;
}}

void main()
{{
    pixel();
    vec3 lighting = {(writesNormalAndPosition ? "SL_GetLighting(pixOut.Position, pixOut.Normal)" : "vec3(1.0)")};
    SL_FragColor = vec4({(writesAlbedo ? "pixOut.Albedo" : "vec3(1.0,1.0,1.0)")} * lighting, {(writesAlpha ? "pixOut.Transparency" : "1.0")});
}}"
        );
    }
}