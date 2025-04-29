using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Graphics.Renderers;

namespace SlopperEngine.Graphics;

/// <summary>
/// A description of the uniforms contained in a shader.
/// </summary>
public class Material
{
    static RenderHandler? _grossDisgustingFix;

    /// <summary>
    /// The shader the material uses.
    /// </summary>
    public readonly SlopperShader Shader;
    /// <summary>
    /// The uniforms contained within the material's shader. 
    /// </summary>
    public readonly UniformDescription[] Uniforms;
    public static Material MissingMaterial = Create(SlopperShader.Create("shaders/errorShader.sesl"));

    protected Material(SlopperShader shader, UniformDescription[] uniforms)
    {
        Shader = shader;
        Uniforms = uniforms;
        //Console.WriteLine(shader+" contains uniforms:");
        //foreach(var j in uniforms)
        //    Console.WriteLine($"{j.type} {j.name} at location {j.location}");
        //Console.Write("\n");
    }
    /// <summary>
    /// Creates a material instance from a shader.
    /// </summary>
    /// <param name="shader">The shader to create the material from.</param>
    /// <returns>A new Material instance.</returns>
    public static Material Create(SlopperShader shader)
    {
        if(_grossDisgustingFix == null) _grossDisgustingFix = new DebugRenderer();
        if(shader.Scope == null)
        {
            return new(shader, []);
        }
        //this SUCKS.
        //getting the settable uniforms should 100% be fixed up at some point.
        DrawShader sh = shader.GetDrawShader(DefaultMeshes.Cube.GetMeshInfo(), _grossDisgustingFix);
        return new Material(shader, sh.GetSettableUniforms().ToArray());
    }

    /// <summary>
    /// Gets the index of a uniform in the uniforms[] array using the uniform's name.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <returns>The index of a UniformDescription in the uniforms[] array. -1 if not present.</returns>
    public int GetUniformIndexFromName(string name)
    {
        for(int i = 0; i<Uniforms.Length; i++)
        if(Uniforms[i].Name == name) return i;
        return -1;
    }
    /// <summary>
    /// Gets a list of indices of uniforms in the uniforms[] array using the uniform's type.
    /// </summary>
    /// <param name="type">The type of the uniform to look for.</param>
    /// <returns>The indices of the uniforms in the array that match the type.</returns>
    public List<int> GetUniformIndicesFromType(ActiveUniformType type)
    {
        List<int> res = new();
        for(int i = 0; i<Uniforms.Length; i++)
        if(Uniforms[i].Type == type)
            res.Add(i);
        return res;
    }

    /// <summary>
    /// Uses the shader and sets the uniforms, preparing for a DrawShader call.
    /// </summary>
    public void Use(MeshInfo info, RenderHandler renderer)
    {
        if(Shader.Scope == null)
        {
            MissingMaterial.Use(info, renderer);
            return;
        }
        ProgramShader shader = Shader.GetDrawShader(info, renderer);
        shader.Use();
        int texcount = 0;
        int imgcount = 0;
        foreach(var u in Uniforms)
        {
            bool n = u.Value == null;
            try{
            switch(u.Type)
            {
                default:
                Console.WriteLine($"im so sorry but i did not implement {u.Type}");
                break;

                case ActiveUniformType.UnsignedInt: shader.SetUniform(u.Location, n?0:(uint)u.Value!); break;

                case ActiveUniformType.Int: shader.SetUniform(u.Location, n?default:(int)u.Value!); break;
                case ActiveUniformType.IntVec2: shader.SetUniform(u.Location, n?default:(Vector2i)u.Value!); break;
                case ActiveUniformType.IntVec3: shader.SetUniform(u.Location, n?default:(Vector3i)u.Value!); break;
                case ActiveUniformType.IntVec4: shader.SetUniform(u.Location, n?default:(Vector4i)u.Value!); break;
                
                case ActiveUniformType.Float: shader.SetUniform(u.Location, n?default:(float)u.Value!); break;
                case ActiveUniformType.FloatVec2: shader.SetUniform(u.Location, n?default:(Vector2)u.Value!); break;
                case ActiveUniformType.FloatVec3: shader.SetUniform(u.Location, n?default:(Vector3)u.Value!); break;
                case ActiveUniformType.FloatVec4: shader.SetUniform(u.Location, n?default:(Vector4)u.Value!); break;
                
                case ActiveUniformType.FloatMat2: shader.SetUniform(u.Location, n?default:(Matrix2)u.Value!); break;
                case ActiveUniformType.FloatMat3: shader.SetUniform(u.Location, n?default:(Matrix3)u.Value!); break;
                case ActiveUniformType.FloatMat4: shader.SetUniform(u.Location, n?default:(Matrix4)u.Value!); break;

                case ActiveUniformType.Sampler2D:
                shader.SetUniform(u.Location, texcount);
                if(n)DefaultTextures.Error.Use(TextureUnit.Texture0+texcount);
                ((Texture2D)u.Value!)?.Use(TextureUnit.Texture0 + texcount);
                texcount++;
                break;

                case ActiveUniformType.Image2D:
                shader.SetUniform(u.Location, imgcount);
                if(n) DefaultTextures.Error.UseAsImage(imgcount);
                ((Texture2D)u.Value!)?.UseAsImage(imgcount);
                imgcount++;
                break;
            }
            }catch
            {
                Console.WriteLine($"hey please put the right type into the uniforms. you gave me {u.Value!.GetType()} but i want {u.Type}");
            }
        }
    }
}