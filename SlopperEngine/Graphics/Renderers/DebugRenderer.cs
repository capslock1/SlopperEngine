using System.CodeDom.Compiler;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Graphics.PostProcessing;
using SlopperEngine.SceneObjects.Rendering;
using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.Lighting;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.Graphics.Renderers;

/// <summary>
/// The simplest renderer possible - renders the scene with no regard for lighting, transparency, or other effects.
/// </summary>
public class DebugRenderer : RenderHandler
{
    [field:DontSerialize] public FrameBuffer Buffer {get; private set;}
    [DontSerialize] LightBuffer _lights;
    [DontSerialize] Bloom _coolBloom;
    Vector2i _screenSize = (400,300);
    Vector2i _trueScreenSize = (800,600);

    public DebugRenderer() : base()
    {
        Buffer = new(400, 300);
        _coolBloom = new(new(400, 300));
        _lights = new();
    }

    [OnSerialize]
    void OnSerialize(SerializedObjectTree.CustomSerializer serializer)
    {
        if (serializer.IsWriter)
        {
            Buffer = new(_screenSize.X, _screenSize.Y, 1);
            _coolBloom = new(_screenSize);
            _lights = new();
        }
    }

    protected override void RenderInternal()
    {
        if (Scene == null) return;
        Buffer.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _lights.ClearBuffer();
        foreach (PointLightData dat in Scene.GetDataContainerEnumerable<PointLightData>())
            _lights.AddLight(dat);
        _lights.UseBuffer();
        foreach (Camera cam in cameras)
        {
            globals.Use();
            globals.CameraProjection = cam.Projection;
            var camTransform = cam.GetGlobalTransform();
            globals.CameraView = camTransform.Inverted();
            globals.CameraPosition = new(camTransform.ExtractTranslation(), 1.0f);
            foreach (Drawcall call in Scene.GetDataContainerEnumerable<Drawcall>())
            {
                call.Material.Use(call.Model.GetMeshInfo(), this);
                globals.Model = call.Owner.GetGlobalTransform();
                call.Model.Draw();
            }
        }
        FrameBuffer.Unuse();
        _coolBloom.AddBloom(GetOutputTexture(), .45f, .25f);
    }

    public override void Resize(Vector2i newSize)
    {
        _trueScreenSize = newSize;
        _screenSize = _trueScreenSize/2;
        
        Buffer?.DisposeAndTextures();
        Buffer = new(_screenSize.X, _screenSize.Y);
        _coolBloom.Dispose();
        _coolBloom = new(_screenSize);
    }

    public override Vector2i GetScreenSize() => _screenSize;
    public override Texture2D GetOutputTexture() => Buffer.ColorAttachments[0];

    protected override void OnDestroyed()
    {
        Buffer.DisposeAndTextures();
        globals.Dispose();
        _lights?.Dispose();
        _coolBloom.Dispose();
    }

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
        foreach(var v in scope.pixOut)
        {
            switch(v.Name)
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

float SL_PhongLighting(vec3 position, vec3 normal, vec3 cameraDirection, SL_PointLightData light)
{{
    vec3 lightDir = light.positionSharpness.xyz - position;
    float lightDist = length(lightDir);
    if(lightDist > light.colorRange.w)
        return 0.0;
    float normLightDist = lightDist / light.colorRange.w;
    lightDir /= lightDist;

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
    float sq = (light.positionSharpness.w);
    litness *= (1-normLightDist) / (sq * sq + 1);
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
        SL_PointLightData light = SL_lightlights.lights[l];
        lightContribution += SL_PhongLighting(position, normal, camDir, light) * light.colorRange.xyz;
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
