using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Interaction;

/// <summary>
/// Base class for buttons. Handles most events.
/// </summary>
public abstract class BaseButton : UIElement
{
    public bool Enabled
    {
        get => _enabled;
    }
    bool _enabled = true;

    protected bool hovered { get; private set; }
    protected int mouseButtonsHeld { get; private set; }

    [OnInputUpdate]
    void InputUpdate(InputUpdateArgs args)
    {
        if (!hovered)
            return;

        if (!LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One))
        {
            hovered = false;
            mouseButtonsHeld = 0;
            OnMouseExit();
        }
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        if (!hovered)
            OnMouseEntry();
        hovered = true;
        if (e.Type == MouseEventType.PressedButton)
        {
            mouseButtonsHeld++;
            OnPressed(e.PressedButton);
            e.Use();
            return;
        }
        if (e.Type == MouseEventType.ReleasedButton && mouseButtonsHeld > 0)
        {
            mouseButtonsHeld--;
            OnAnyRelease(e.ReleasedButton);
            e.Use();
            if (mouseButtonsHeld == 0)
                OnAllButtonsReleased();
            return;
        }
        e.Block();
    }

    protected abstract void OnPressed(MouseButton button);
    protected abstract void OnAnyRelease(MouseButton button);
    protected abstract void OnAllButtonsReleased();
    protected abstract void OnMouseEntry();
    protected abstract void OnMouseExit();
    protected abstract void OnEnable();
    protected abstract void OnDisable();
}