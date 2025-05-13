using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics.GPUResources.Shaders;

/// <summary>
/// An abstract shader that describes the fragment function of a drawshader. Cannot be used outside of a DrawShader.
/// </summary>
public class VertexShader : Shader
{
    VertexShader(int handle) : base(handle){}
    
    /// <summary>
    /// Creates a new VertexShader instance from GLSL code.
    /// </summary>
    /// <param name="vertCode">The source code of the vertex shader.</param>
    /// <returns>A new VertexShader instance.</returns>
    public static VertexShader Create(string vertCode)
    {
        int handle;

        handle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(handle, vertCode);

        GL.CompileShader(handle);
        GL.GetShader(handle, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(handle);
            Console.WriteLine(infoLog);
        }

        return new VertexShader(handle);
    }

    protected override IGPUResourceOrigin GetOrigin() => new NoOrigin();
    protected class NoOrigin : IGPUResourceOrigin
    {
        public GPUResource CreateResource() => Create(@"#version 450 core
void main()
{
    gl_Position = vec4(0);
}");
    }
}