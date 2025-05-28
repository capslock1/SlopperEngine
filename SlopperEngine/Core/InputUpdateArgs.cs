using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core.Collections;
using SlopperEngine.Windowing;

namespace SlopperEngine.Core;

/// <summary>
/// Describes an input for OnInputUpdate.
/// </summary>
public class InputUpdateArgs
{
    /// <summary>
    /// The state of the keyboard when the input update was made.
    /// </summary>
    public readonly KeyboardState KeyboardState;
    /// <summary>
    /// The state of the mouse when the input update was made.
    /// </summary>
    public readonly MouseState MouseState;
    /// <summary>
    /// The text input events that were received.
    /// </summary>
    public ReadOnlySpan<TextInputEvent> TextInputEvents => _textInputEvents;

    readonly SpanList<TextInputEvent>.ReadOnlySpanList _textInputEvents;

    public InputUpdateArgs(KeyboardState keyboard, MouseState mouse, SpanList<TextInputEvent>.ReadOnlySpanList textInputEvents)
    {
        KeyboardState = keyboard;
        MouseState = mouse;
        _textInputEvents = textInputEvents;
    }
}