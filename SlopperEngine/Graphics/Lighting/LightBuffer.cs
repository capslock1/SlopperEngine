
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources;

namespace SlopperEngine.Graphics.Lighting;

/// <summary>
/// Worst lighting buffer on television.
/// </summary>
public class LightBuffer : IDisposable
{
    List<Light> _lights = new();
    BufferObject _buffer;
    int _currentBufferLength;
    
    public const string GLSLString =
@"
struct SL_PointLightData
{
    vec4 colorRange;
    vec4 positionSharpness;
};
layout(binding = 1, std140) buffer SL_Lights
{
    int count;
    int pad1; int pad2; int pad3;
    SL_PointLightData[] lights;
} SL_lightlights;
";

    public LightBuffer()
    {
        _buffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, 1*sizeof(int));
    }

    public void ClearBuffer() => _lights.Clear();
    public void AddLight(in PointLightData dat) => _lights.Add(new(){
        ColorRange = new(dat.Color, dat.Radius), 
        PositionSharp = new(dat.Object.GetGlobalTransform().ExtractTranslation(), dat.Sharpness)});

    public void UseBuffer()
    {
        if(_lights.Count > _currentBufferLength)
        {
            _buffer.Dispose();
            int ct = (int)(_lights.Count*1.5f);
            _buffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, ct * Unsafe.SizeOf<Light>() + 16);
            _currentBufferLength = ct;
        }

        _buffer.SetData(_lights.Count, 0);
        _buffer.SetData(CollectionsMarshal.AsSpan(_lights), 16);

        _buffer.Bind(1);
    }

    bool _alreadyDisposed;
    public void Dispose()
    {
        if(!_alreadyDisposed)
            _buffer.Dispose();
        _alreadyDisposed = true;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Light
    {
        [FieldOffset(0)]
        public Vector4 ColorRange;

        [FieldOffset(16)]
        public Vector4 PositionSharp;
    }
}