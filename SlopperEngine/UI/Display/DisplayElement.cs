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

    public DisplayElement(Box2 localShape) : base(localShape){}

    protected override bool BlocksMouse() => BlockAnyInput;
}