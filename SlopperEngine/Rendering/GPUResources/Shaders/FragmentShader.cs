using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Rendering;

/// <summary>
/// An abstract shader that describes the fragment function of a drawshader. Cannot be used outside of a DrawShader.
/// </summary>
public class FragmentShader : Shader
{
    FragmentShader(int handle) : base(handle){}
    
    /// <summary>
    /// Creates a new FragmentShader instance from GLSL code.
    /// </summary>
    /// <param name="fragCode">The source code of the fragment shader.</param>
    /// <returns>A new FragmentShader instance.</returns>
    public static FragmentShader Create(string fragCode)
    {
        int handle;

        handle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(handle, fragCode);

        GL.CompileShader(handle);
        GL.GetShader(handle, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(handle);
            Console.WriteLine(infoLog);
        }

        return new FragmentShader(handle);
    }
}