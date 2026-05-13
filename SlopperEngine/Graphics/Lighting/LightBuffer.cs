
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.GPUResources;

namespace SlopperEngine.Graphics.Lighting;

/// <summary>
/// Worst lighting buffer on television.
/// </summary>
public class LightBuffer : IDisposable
{
    List<LightGLSL> _lights = new();
    BufferObject _buffer;
    int _currentBufferLength;
    
    /// <summary>
    /// Inserts the light buffer as GLSL. The lights are laid out: colorRange (vec4), positionSharpness (vec4), type3xNull (ivec4). type.x means 0 for point and 1 for directional.
    /// </summary>
    public const string GLSLString =
@"
struct SL_LightData
{
    vec4 colorRange;
    vec4 positionSharpness;
};
layout(binding = 1, std140) buffer SL_Lights
{
    int count;
    int pad1; int pad2; int pad3;
    SL_LightData[] lights;
} SL_lightlights;
";
    const int _HeaderSize = 4*sizeof(int);

    public LightBuffer()
    {
        _buffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, 1*sizeof(int));
    }

    public void ClearBuffer() => _lights.Clear();
    public void AddLight(in PointLightData dat) => _lights.Add(new(){
        ColorRange = new(dat.Color, float.Max(dat.Radius, 0)), // ensure range >= 0 so point light is always a point light 
        PositionSharp = new(dat.Object.GetGlobalTransform().ExtractTranslation(), dat.Sharpness)});

    public void AddLight(in DirectionalLightData dat) =>_lights.Add(new(){
        ColorRange = new(dat.Color, -1), 
        PositionSharp = new(dat.Object.GetGlobalTransform().Column2.Xyz, 0)}); // position in directional lights is actually direction

    public void UseBuffer()
    {
        if(_lights.Count > _currentBufferLength)
        {
            _buffer.Dispose();
            int ct = (int)(_lights.Count*1.5f);
            _buffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, _HeaderSize + ct * Unsafe.SizeOf<LightGLSL>());
            _currentBufferLength = ct;
        }

        _buffer.SetData(_lights.Count, 0);
        _buffer.SetData(CollectionsMarshal.AsSpan(_lights), _HeaderSize);

        _buffer.Bind(1);
    }

    bool _alreadyDisposed;
    public void Dispose()
    {
        if(!_alreadyDisposed)
            _buffer.Dispose();
        _alreadyDisposed = true;
    }
}