using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.UI.Base;

/// <summary>
/// Represents a blockable event of the mouse in UI.
/// </summary>
/// <param name="position">NDC mouse position.</param>
/// <param name="delta">NDC mouse delta.</param>
/// <param name="scrollDelta">The scrollwheel delta.</param>
/// <param name="pressedButton">A button that was pressed this frame.</param>
/// <param name="releasedButton">A button that was released this frame.</param>
/// <param name="state">The mouse state. Should be passed for querying if a button is held.</param>
/// <param name="keyboardState">The keyboardState. Should be passed for quering what modifiers are held.</param>
/// <param name="type">What type of event this object represents.</param>
public ref struct MouseEvent(Vector2 position, Vector2 delta, Vector2 scrollDelta, MouseButton pressedButton, MouseButton releasedButton, MouseState state, KeyboardState keyboardState, MouseEventType type)
{
    /// <summary>
    /// What type of event this object represents.
    /// </summary>
    public MouseEventType Type { get; private set; } = type;

    /// <summary>
    /// The mouse position in UI global space.
    /// </summary>
    public Vector2 NDCPosition { get; private set; } = position;

    /// <summary>
    /// The mouse delta in UI global space.
    /// </summary>
    public Vector2 NDCDelta { get; private set; } = delta;

    /// <summary>
    /// How many pixels the mouse has scrolled this frame. Default if this is event does not represent a scroll.
    /// </summary>
    public Vector2 ScrollDelta { get; private set; } = scrollDelta;

    /// <summary>
    /// The button that was pressed in this event. -1 (invalid enum value) if this event does not represent a click.
    /// </summary>
    public MouseButton PressedButton { get; private set; } = pressedButton;

    /// <summary>
    /// The button that was released in this event. -1 (invalid enum value) if this event does not represent a mouse button release.
    /// </summary>
    public MouseButton ReleasedButton { get; private set; } = releasedButton;

    private MouseState _state = state;
    private KeyboardState _keyboard = keyboardState;

    /// <summary>
    /// Uses the event, ensuring no other UIElement receives it.
    /// </summary>
    public void Use() => Type = MouseEventType.Used;

    /// <summary>
    /// Blocks the event, ensuring only UIElements rendered over this can receive it.
    /// </summary>
    public void Block() => Type = MouseEventType.Blocked;

    /// <summary>
    /// Gets a bool indicating whether a given button is held.
    /// </summary>
    public readonly bool IsButtonDown(MouseButton button) => _state.IsButtonDown(button);

    /// <summary>
    /// Gets a bool indicating whether the given key is held.
    /// </summary>
    public readonly bool IsKeyDown(Keys key) => _keyboard.IsKeyDown(key);
}