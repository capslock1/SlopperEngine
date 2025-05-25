using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core.Collections;
using SlopperEngine.Windowing;

namespace SlopperEngine.Core;

public class InputUpdateArgs
{
    public readonly KeyboardState KeyboardState;
    public readonly MouseState MouseState;
    public readonly SpanList<TextInputEvent>.ReadOnlySpanList TextInputEvents;
    public InputUpdateArgs(KeyboardState keyboard, MouseState mouse, SpanList<TextInputEvent>.ReadOnlySpanList textInputEvents)
    {
        KeyboardState = keyboard;
        MouseState = mouse;
        TextInputEvents = textInputEvents;
    } 
}