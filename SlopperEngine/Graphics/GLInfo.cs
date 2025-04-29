using OpenTK.Graphics.OpenGL4;

namespace SlopperEngine.Graphics;

/// <summary>
/// Basic, static info about the available graphics library.
/// </summary>
public static class GLInfo
{
    private static int _version = -1;
    private static string? _versionString = null;

    /// <summary>
    /// The version of OpenGL, as an int. Expected to be at least 430.
    /// </summary>
    public static int Version
    {
        get{
            if(_version == -1) 
                SetVersion();
            return _version;
        }
    }

    /// <summary>
    /// The version of OpenGL, as a string. Expected to be at least 430.
    /// </summary>
    public static string VersionString
    {
        get{
            if(_version == -1)
                SetVersion();
            return _versionString!;
        }
    }
    private static void SetVersion()
    {
        _versionString = GL.GetString(StringName.Version);
        _version = 100*GL.GetInteger(GetPName.MajorVersion);
        _version += 10*GL.GetInteger(GetPName.MinorVersion);
    }
}