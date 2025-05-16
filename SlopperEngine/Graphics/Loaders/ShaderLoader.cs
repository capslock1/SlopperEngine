using SlopperEngine.Core;
using SlopperEngine.Core.Collections;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Shaders;

namespace SlopperEngine.Graphics.Loaders;

/// <summary>
/// Loads shaders into VRAM from the disk.
/// </summary>
public static class ShaderLoader
{
    static Cache<string, DrawShader> _shaderCache = new();

    /// <summary>
    /// Gets a DrawShader from the filepaths to a vertex and fragment shader.
    /// </summary>
    /// <param name="VertexFilepath">The path to the .vert file. Is relative to the full game path.</param>
    /// <param name="FragmentFilepath">The path to the .frag file. Is relative to the full game path.</param>
    /// <returns>A new DrawShader instance, or an instance from the cache.</returns>
    /// <exception cref="Exception"></exception>
    public static DrawShader FromRawGLSLFilepaths(string VertexFilepath, string FragmentFilepath)
    {
        string cacheKey = VertexFilepath+":"+FragmentFilepath;
        DrawShader? res = _shaderCache.Get(cacheKey); 
        if(res != null)
            return res;

        string VertexShaderSource = File.ReadAllText(Assets.GetPath(VertexFilepath));
        string FragmentShaderSource = File.ReadAllText(Assets.GetPath(FragmentFilepath));

        VertexShader vert = VertexShader.Create(VertexShaderSource);
        FragmentShader frag = FragmentShader.Create(FragmentShaderSource);

        res = DrawShader.Create(vert, frag);
        res.OverrideOrigin = new RawGLSLShaderOrigin(VertexFilepath, FragmentFilepath);

        vert.Dispose();
        frag.Dispose();

        _shaderCache.Set(cacheKey, res);
        
        return res;
    }

    class RawGLSLShaderOrigin(string vertFilepath, string fragFilepath) : IGPUResourceOrigin
    {
        string vertexFilePath = vertFilepath;
        string fragmentFilePath = fragFilepath;
        public GPUResource CreateResource() => FromRawGLSLFilepaths(vertexFilePath, fragmentFilePath);
        public override string ToString() => $"Shader from filepaths: '{vertexFilePath}', '{fragmentFilePath}'";
    }
}