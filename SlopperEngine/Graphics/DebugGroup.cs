using System;
using OpenTK.Graphics.OpenGL;

namespace SlopperEngine.Graphics;

/// <summary>
/// Adds a label in debug programs like render doc to make debugging the rendering easier.
/// </summary>
public struct DebugGroup : IDisposable
{
    /// <summary>
    /// Make sure to call this in a using statement! Make sure the GroupName is a const string!
    /// </summary>
    public static DebugGroup StartUsing(string GroupName)
    {
        GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, GroupName.Length, GroupName);
        return new();
    }

    /// <summary>
    /// Ends the debug group. only call this use through using the StartUsing function and with a using statement.
    /// </summary>
    public void Dispose()
    {
        GL.PopDebugGroup();
    }
}