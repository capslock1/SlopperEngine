using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.Windowing;

public class WindowSettings(Vector2i Size, Vector2i Position = default, string Title = "SlopperEngine", WindowState WindowState = WindowState.Normal, bool StartVisible = true, bool TransparentFrameBuffer = false, WindowIcon? Icon = null, WindowBorder Border = default)
{
    public Vector2i Size = Size;
    public Vector2i Position = Position;
    public string Title = Title;
    public WindowState WindowState = WindowState;
    public bool StartVisible = StartVisible;
    public bool TransparentFramebuffer = TransparentFrameBuffer;
    [DontSerialize] public WindowIcon? Icon = Icon;
    public WindowBorder Border = Border;
}