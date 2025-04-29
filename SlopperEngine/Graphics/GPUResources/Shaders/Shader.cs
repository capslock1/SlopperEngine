using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics;

/// <summary>
/// Abstract class describing an unbound shader object. Cannot be used outside of a ProgramShader.
/// </summary>
public abstract class Shader : GPUResource
{
    public readonly int Handle;
    protected Shader(int handle)
    {
        Handle = handle;
    }

    protected override ResourceData GetResourceData() => new ShaderResourceData(){handle = this.Handle};
    protected class ShaderResourceData : ResourceData
    {
        public int handle;
        public override void Clear()
        {
            GL.DeleteShader(handle);
        }
    } 
}