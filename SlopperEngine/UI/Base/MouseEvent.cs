using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.UI.Base;

/// <summary>
/// Represents a blockable event of the mouse in UI.
/// </summary>
/// <param name="position">NDC mouse position.</param>
/// <param name="delta">NDC mouse delta.</param>
/// <param name="pressedButton">A button that was pressed this frame.</param>
/// <param name="releasedButton">A button that was released this frame.</param>
/// <param name="state">The mouse state. Should be passed for querying if a button is held.</param>
/// <param name="moveOrDrag">Whether this event represents the mouse moving or being dragged.</param>
public ref struct MouseEvent(Vector2 position, Vector2 delta, MouseButton pressedButton, MouseButton releasedButton, MouseState state, bool moveOrDrag)
{
    /// <summary>
    /// Whether the event represents the mouse moving, or being dragged.
    /// </summary>
    public bool IsMoveOrDrag { get; private set; } = moveOrDrag;

    /// <summary>
    /// The mouse position in UI global space.
    /// </summary>
    public Vector2 NDCPosition { get; private set; } = position;

    /// <summary>
    /// The mouse delta in UI global space.
    /// </summary>
    public Vector2 NDCDelta { get; private set; } = delta;

    /// <summary>
    /// The button that was pressed in this event. -1 (invalid enum value) if this event does not represent a click.
    /// </summary>
    public MouseButton PressedButton { get; private set; } = pressedButton;

    /// <summary>
    /// The button that was released in this event. -1 (invalid enum value) if this event does not represent a mouse button release.
    /// </summary>
    public MouseButton ReleasedButton { get; private set; } = releasedButton;

    /// <summary>
    /// Whether or not the event has been used by a UIElement down the tree. No need to check - UIElement does this on its own.
    /// </summary>
    public bool Used { get; private set; } = false;

    private MouseState _state = state;

    /// <summary>
    /// Uses the event, ensuring no other UIElement receives it.
    /// </summary>
    public void Use()
    {
        Used = true;
    }

    /// <summary>
    /// Gets a bool indicating whether a given button is held.
    /// </summary>
    public bool IsButtonDown(MouseButton button) => _state.IsButtonDown(button);
}