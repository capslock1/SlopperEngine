using SlopperEngine.Engine.Collections;

namespace SlopperEngine.Rendering;

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

        string VertexShaderSource = File.ReadAllText(Path.Combine("../",VertexFilepath));
        string FragmentShaderSource = File.ReadAllText(Path.Combine("../",FragmentFilepath));

        VertexShader vert = VertexShader.Create(VertexShaderSource);
        FragmentShader frag = FragmentShader.Create(FragmentShaderSource);

        res = DrawShader.Create(vert, frag);

        vert.Dispose();
        frag.Dispose();

        _shaderCache.Set(cacheKey, res);
        
        return res;
    }
}