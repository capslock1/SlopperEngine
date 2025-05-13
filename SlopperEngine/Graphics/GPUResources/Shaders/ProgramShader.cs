using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SlopperEngine.Graphics.GPUResources.Shaders;

/// <summary>
/// A shader that can execute a program on the GPU. Used by DrawShader and ComputeShader.
/// </summary>
public abstract class ProgramShader : GPUResource
{
    static int _activeShader;
    public readonly int Handle;
    Action? _setUniformQueue = null;

    protected ProgramShader(int handle)
    {
        Handle = handle;
    }

    /// <summary>
    /// Uses the shader. Does not execute any code on the GPU.
    /// </summary>
    public void Use()
    {
        if(Handle != _activeShader) GL.UseProgram(Handle);
        _activeShader = Handle;
        _setUniformQueue?.Invoke();
        _setUniformQueue = null;
    }
    /// <summary>
    /// Gets the location of a uniform.
    /// </summary>
    /// <param name="name">The name of the shader's uniform.</param>
    /// <returns>-1 if the uniform does not exist, or is unused. Else a positive integer describing a location for use in SetUniform().</returns>
    public int GetUniformLocation(string name)
    {
        return GL.GetUniformLocation(Handle, name);
    }
    /// <summary>
    /// Gets a list of uniforms in the shader.
    /// </summary>
    /// <returns>A list of uniforms. Uniforms with location -1 are removed.</returns>
    public List<UniformDescription> GetSettableUniforms()
    {
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int uniformCount);
        List<UniformDescription> res = new();
        for(int i = 0; i < uniformCount; i++)
        {
            GL.GetActiveUniform(Handle, i, 256, out _, out int size, out ActiveUniformType type, out string name);
            int loc = GetUniformLocation(name);
            if(loc > -1)
                res.Add(new(type, size, name, loc));
        }
        return res;
    }
    
    void AddToSetUniformQueue(Action GLCall)
    {
        if(_activeShader == Handle)
            GLCall.Invoke();
        else _setUniformQueue += GLCall;
    }
    public void SetUniform(int location, float value) {
        AddToSetUniformQueue( ()=>{
            GL.Uniform1(location, value);
        });
    }
    public void SetUniform(int location, Vector2 value) { 
        AddToSetUniformQueue( ()=>{
            GL.Uniform2(location, value);
        });
    }
    public void SetUniform(int location, Vector3 value) { 
        AddToSetUniformQueue( ()=>{
            GL.Uniform3(location, value);
        });
    }
    public void SetUniform(int location, Vector4 value) { 
        AddToSetUniformQueue( ()=>{
            GL.Uniform4(location, value);
        });
    }
    public void SetUniform(int location, int value) {
        AddToSetUniformQueue( ()=>{
            GL.Uniform1(location, value);
        });
    } 
    public void SetUniform(int location, Vector2i value) {
        AddToSetUniformQueue( ()=>{
            GL.Uniform2(location, value);
        });
    }
    public void SetUniform(int Location, Vector3i Value) {
        AddToSetUniformQueue( ()=>{
            GL.Uniform3(Location, Value);
        });
    }
    public void SetUniform(int location, Vector4i value) {
        AddToSetUniformQueue( ()=>{
            GL.Uniform4(location, value);
        });
    }
    public void SetUniform(int location, Matrix2 value) { 
        AddToSetUniformQueue( ()=>{
            GL.UniformMatrix2(location, true, ref value);
        });
    }
    public void SetUniform(int location, Matrix3 value) { 
        AddToSetUniformQueue( ()=>{
            GL.UniformMatrix3(location, true, ref value);
        });
    }
    public void SetUniform(int location, Matrix4 value) { 
        AddToSetUniformQueue( ()=>{
            GL.UniformMatrix4(location, true, ref value);
        });
    }
    public void SetUniformBuffer(string name, int index)
    {
        int location = GL.GetUniformBlockIndex(Handle, name);
        if(location < 0) 
        {
            Console.WriteLine($"Attempted to set uniform buffer {name}, but shader with handle {Handle} did not have a binding for this");
            return;
        }
        GL.UniformBlockBinding(Handle, location, index);
    }

    protected override ResourceData GetResourceData() => new ShaderResourceData(){handle = this.Handle};
    protected class ShaderResourceData : ResourceData
    {
        public int handle;
        public override void Clear()
        {
            GL.DeleteProgram(handle);
        }
    } 
}
