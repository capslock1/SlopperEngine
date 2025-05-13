using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics.GPUResources.Shaders;

/// <summary>
/// A specialized type of programshader able to process arbitrary data.
/// </summary>
public class ComputeShader : ProgramShader
{
    protected ComputeShader(int handle) : base(handle){}
    
    /// <summary>
    /// Creates a compute shader from GLSL code.
    /// </summary>
    /// <param name="computeCode">The GLSL source code to create this compute shader.</param>
    /// <returns>A new compute shader instance.</returns>
    public static ComputeShader Create(string computeCode)
    {
        //first make the shader
        int sHandle;

        sHandle = GL.CreateShader(ShaderType.ComputeShader);
        GL.ShaderSource(sHandle, computeCode);

        GL.CompileShader(sHandle);
        GL.GetShader(sHandle, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(sHandle);
            Console.WriteLine(infoLog);
        }

        
        //then make the program
        int handle = GL.CreateProgram();

        GL.AttachShader(handle, sHandle);
        GL.LinkProgram(handle);

        GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(handle);
            Console.WriteLine(infoLog);
        }

        //get rid of the shader object - its pretty much useless
        GL.DetachShader(handle, sHandle);
        GL.DeleteShader(sHandle);

        return new ComputeShader(handle);
    }

    /// <summary>
    /// Activates the main() function of the compute shader on the GPU.
    /// </summary>
    /// <param name="groupsX">The amount of times to call "local_size_x" invocations of the shader.</param>
    /// <param name="groupsY">The amount of times to call "local_size_y" invocations of the shader.</param>
    /// <param name="groupsZ">The amount of times to call "local_size_z" invocations of the shader.</param>
    /// <exception cref="Exception">Illegal amount of groups.</exception>
    public void Dispatch(int groupsX, int groupsY, int groupsZ)
    {
        if(groupsX < 1 || groupsY < 1 || groupsZ < 1)
            throw new Exception("Attempted to dispatch a compute shader with 0 or less groups");
        Use();
        GL.DispatchCompute(groupsX, groupsY, groupsZ);
    }

    protected override IGPUResourceOrigin GetOrigin() => new NoOrigin();
    protected class NoOrigin : IGPUResourceOrigin
    {
        public GPUResource CreateResource() => Create(@"#version 450
layout(local_size_x = 32, local_size_y = 32) in;
void main() {}");
    }
}