using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.Graphics;

/// <summary>
/// Hardcoded and messy way of rendering a triangle to the screen. Mainly useful for windowing, as it functions across windows. UVs range from 0-1 from side to side of the screen.
/// </summary>
public unsafe class FullScreenTriangle
{
    static Dictionary<IntPtr, FullScreenTriangle> _renderers = new();
    int _vertexArrayObject;
    int _vertexBufferObject;
    DrawShader? _shader;
    int _texLoc;
    FullScreenTriangle()
    {
        _vertexArrayObject = GL.GenVertexArray(); 
        _vertexBufferObject = GL.GenBuffer(); 
        Use();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

        float[] vertices = {
            -1, 3, 0, 2,
            -1,-1, 0, 0,
             3,-1, 2, 0};

        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2*sizeof(float));
        GL.EnableVertexAttribArray(1);
        
        //not using shader loading for something this simple. raw strings will do the trick
        //especially if it lets me circumvent caching (which only works for the maincontext)
        using VertexShader vert = VertexShader.Create(@"
#version 450 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aUVs;

out vec2 uvCoords;

void main()
{
    gl_Position = vec4(aPosition, 1.0, 1.0);
    uvCoords = aUVs;
}");

        using FragmentShader frag = FragmentShader.Create(@"#version 450 core
out vec4 FragColor;

in vec2 uvCoords;

uniform sampler2D mainTexture;

void main()
{
    vec3 res = texture(mainTexture, uvCoords).xyz;
    FragColor = vec4(res,1.0);
}");

        _shader = DrawShader.Create(vert, frag);
        _texLoc = _shader.GetUniformLocation("mainTexture");
    }

    /// <summary>
    /// Draws the texture to the screen.
    /// </summary>
    /// <param name="texture">The texture to draw to the screen.</param>
    public static void Draw(Texture2D texture)
    {
        Window* currentContext = GLFW.GetCurrentContext();
        if(_renderers.TryGetValue((IntPtr)currentContext, out var trongle))
        {
            trongle._draw(texture);
            return;
        }
        trongle = new();
        _renderers.Add((IntPtr)currentContext, trongle);
        trongle._draw(texture);
    }
    void _draw(Texture2D texture)
    {
        Use();
        texture?.Use(TextureUnit.Texture1);
        _shader?.Use();
        _shader?.SetUniform(_texLoc, 1);
        bool depth = GL.IsEnabled(EnableCap.DepthTest);
        if(depth) GL.Disable(EnableCap.DepthTest);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        if(depth) GL.Enable(EnableCap.DepthTest);
    }
    void Use()
    {
        Mesh.Unuse();
        GL.BindVertexArray(_vertexArrayObject);
    }
}
