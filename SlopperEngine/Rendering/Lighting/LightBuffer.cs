
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Core.Collections;
using SlopperEngine.SceneObjects;
using OpenTK.Mathematics;

namespace SlopperEngine.Rendering.Lighting;

/// <summary>
/// Worst lighting buffer on television.
/// </summary>
public class LightBuffer : IDisposable
{
    List<LightGLSL> _lights = new();
    List<ShadowCasterGLSL> _shadowCasters = new();
    BufferObject _buffer;
    int _currentBufferLength;
    BufferObject _shadowBuffer;
    int _currentShadowBufferLength;
    
    /// <summary>
    /// Inserts the light buffers as GLSL. The layout is complicated, so please just look at the source to see for yourself.
    /// </summary>
    public const string GLSLString =
@"
struct SL_LightData
{
    vec4 colorRange;
    vec4 positionSharpness;
};
struct SL_ShadowData
{
    mat4 viewProj;
    vec4 color;
    ivec4 cascadeIndices;
    vec4 cascadeSizes;
    ivec4 padding;
};
layout(binding = 1, std140) buffer SL_Lights
{
    int count;
    int pad1; int pad2; int pad3;
    SL_LightData[] lights;
} SL_lightlights;
layout(binding = 2, std140) buffer SL_Shadows
{
    int count;
    int pad1; int pad2; int pad3;
    SL_ShadowData[] lights;
} SL_shadowlights;
";
    const int _HeaderSize = 4*sizeof(int);

    public LightBuffer()
    {
        _buffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, 1*sizeof(int));
        _shadowBuffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, 1*sizeof(int));
    }

    /// <summary>
    /// Clears ONLY THE CPU SIDE!! of the light buffer.
    /// </summary>
    public void ClearBuffer() => _lights.Clear();

    /// <summary>
    /// Adds a point light to the CPU side of the light buffer.
    /// </summary>
    public void AddLight(in PointLight dat) => _lights.Add(new(){
        ColorRange = new(dat.Color, float.Max(dat.Radius, 0)), // ensure range >= 0 so point light is always a point light 
        PositionSharp = new(dat.GetGlobalTransform().ExtractTranslation(), dat.Sharpness)});

    /// <summary>
    /// Adds a directional light to the CPU side of the light buffer.
    /// </summary>
    public void AddLight(in DirectionalLight dat) {
        if(!dat.CastsShadows)
            _lights.Add(new(){
                ColorRange = new(dat.Color, -1), 
                PositionSharp = new(-dat.GetGlobalTransform().Column2.Xyz, 0) // position in directional lights is actually direction
            }); 
        else
        {
            Vector4 cascadeSizes = default;
            var cascades = dat.Cascades ?? DirectionalLight.DefaultCascades;
            for(int i = 0; i<cascades.Length && i<4; i++)
                cascadeSizes[i] = cascades.Span[i];
            _shadowCasters.Add(new()
            {
                Color = new(dat.Color, 0),
                CascadeIndices = new(1,0,0,0),
                CascadeSizes = cascadeSizes,
                ViewProjection = Matrix4.CreateOrthographic(1, 1, -dat.PlaneDistance, dat.PlaneDistance) * dat.GetGlobalTransform(),
            });
        }
    }

    /// <summary>
    /// Updates the buffer on the CPU side and binds it to buffer 1.
    /// </summary>
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

        if(_shadowCasters.Count > _currentShadowBufferLength)
        {
            _shadowBuffer.Dispose();
            int ct = (int)(_shadowCasters.Count*1.5f);
            _shadowBuffer = BufferObject.Create(BufferTarget.ShaderStorageBuffer, _HeaderSize + ct * Unsafe.SizeOf<ShadowCasterGLSL>());
            _currentShadowBufferLength = ct;
        }

        _shadowBuffer.SetData(_shadowCasters.Count, 0);
        _shadowBuffer.SetData(CollectionsMarshal.AsSpan(_shadowCasters), _HeaderSize);

        _shadowBuffer.Bind(2);
    }

    /// <summary>
    /// Updates the light buffer according to a scene's lights and uses it immediately.
    /// </summary>
    public void UseAndUpdateFromScene(Scene sceneToUse)
    {
        ClearBuffer();
        PointLightBufferUpdater lightUpdater = new(this);
        sceneToUse.GetDataContainerEnumerable<PointLight>().Enumerate(ref lightUpdater);
        DirectionalLightBufferUpdater lightUpdater2 = new(this);
        sceneToUse.GetDataContainerEnumerable<DirectionalLight>().Enumerate(ref lightUpdater2);
        UseBuffer();
    }

    bool _alreadyDisposed;
    public void Dispose()
    {
        if(!_alreadyDisposed)
        {
            _buffer.Dispose();
            _shadowBuffer.Dispose();
        }
        _alreadyDisposed = true;
    }

    struct PointLightBufferUpdater(LightBuffer buffer) : IRefEnumerator<PointLight>
    {
        public void Next(ref PointLight value)
        {
            buffer.AddLight(value);
        }
    }
    struct DirectionalLightBufferUpdater(LightBuffer buffer) : IRefEnumerator<DirectionalLight>
    {
        public void Next(ref DirectionalLight value)
        {
            buffer.AddLight(value);
        }
    }
}