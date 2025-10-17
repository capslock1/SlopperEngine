using System;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources;

namespace SlopperEngine.Graphics;

/// <summary>
/// Values defined in the uniform at binding = 0. Only actually global per renderer.
/// </summary>
public class ShaderGlobals : IDisposable
{
    public void Use()
    {
        _buffer.Bind(0);
    }

    BufferObject _buffer = BufferObject.Create(BufferTarget.UniformBuffer, Unsafe.SizeOf<Matrix4>()*3 + sizeof(float)*8);

    Matrix4 _cameraProjection;
    /// <summary>
    /// The camera's projection matrix.
    /// </summary>
    public Matrix4 CameraProjection
    {
        get{return _cameraProjection;}
        set{
            _cameraProjection = value;
            _buffer.SetData(value, 0);
        }
    }

    Matrix4 _cameraView;
    /// <summary>
    /// The camera's view matrix.
    /// </summary>
    public Matrix4 CameraView
    {
        get{return _cameraView;}
        set{
            _cameraView = value;
            _buffer.SetData(value, 64);
        }
    }

    Matrix4 _model;
    /// <summary>
    /// The model to global matrix.
    /// </summary>
    public Matrix4 Model
    {
        get{return _model;}
        set{
            _model = value;
            _buffer.SetData(value, 128);
        }
    }

    Vector4 _camPosition;

    public Vector4 CameraPosition
    {
        get => _camPosition;
        set{
            _camPosition = value;
            _buffer.SetData(value, 192);
        }
    }

    float _time;
    /// <summary>
    /// The time from the start of the scene.
    /// </summary>
    public float Time
    {
        get{return _time;}
        set{
            _time = value;
            _buffer.SetData(value, 208);
        }
    }

    public static string ToGLSL() => 
    @"
layout (std140) uniform SL_Globals
{
    mat4 projection;
    mat4 view;
    mat4 model;
    vec4 cameraPosition;
    float time;
} Globals;";

    bool _alreadyDisposed;
    public void Dispose()
    {
        if(!_alreadyDisposed)
            _buffer.Dispose();
        _alreadyDisposed = true;
    }
}