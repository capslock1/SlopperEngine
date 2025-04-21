using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SlopperEngine.Rendering;

/// <summary>
/// A shader that can be used to draw a mesh into the bound framebuffer.
/// </summary>
public class DrawShader : ProgramShader
{
    private DrawShader(int handle) : base(handle){}
    
    /// <summary>
    /// Creates a new drawshader from a vertex and fragment shader..
    /// </summary>
    /// <param name="vert">The vertex shader.</param>
    /// <param name="frag">The fragment shader.</param>
    /// <returns>A new DrawShader instance.</returns>
    public static DrawShader Create(VertexShader vert, FragmentShader frag)
    {
        //monstrous. oh yes. make it ONE shader. splendid
        int handle = GL.CreateProgram();

        //hold on a second - can you put multiple frag/vertex shaders on here?
        //that sounds quite vile
        GL.AttachShader(handle, vert.Handle);
        GL.AttachShader(handle, frag.Handle);

        GL.LinkProgram(handle);

        GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(handle);
            Console.WriteLine(infoLog);
        }

        //get rid of the original vertex and frag shader. they are now kissing in Handle
        GL.DetachShader(handle, vert.Handle);
        GL.DetachShader(handle, frag.Handle);

        return new DrawShader(handle);
    }
}