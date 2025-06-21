using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Display;

/// <summary>
/// Abstract base class for simple display objects.
/// </summary>
public abstract class DisplayElement : UIElement
{
    /// <summary>
    /// Whether this element blocks all inputs from passing through.
    /// </summary>
    public bool BlockAnyInput = true;

    /// <summary>
    /// Whether this element should block click events from passing through.
    /// </summary>
    public bool BlockClicks = false;

    /// <summary>
    /// Whether this element should block scroll events from passing through.
    /// </summary>
    public bool BlockScrolls = false;

    public DisplayElement(Box2 localShape) : base(localShape){}

    protected override void HandleEvent(ref MouseEvent e)
    {
        if (BlockAnyInput)
        {
            e.Block();
            return;
        }

        switch (e.Type)
        {
            case MouseEventType.PressedButton:
            case MouseEventType.ReleasedButton:
                if (BlockClicks)
                    e.Block();
                return;

            case MouseEventType.Scroll:
                if (BlockScrolls)
                    e.Block();
                return;
        }
    }
}