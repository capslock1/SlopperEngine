using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Layout;

/// <summary>
/// Invisible element with externally customizeable shape.
/// </summary>
public sealed class Spacer : UIElement
{
    /// <summary>
    /// The minimum width in pixels.
    /// </summary>
    public int MinWidth = 0;
    /// <summary>
    /// The minimum height in pixels.
    /// </summary>
    public int MinHeight = 0;
    /// <summary>
    /// The maximum width in pixels.
    /// </summary>
    public int MaxWidth = int.MaxValue;
    /// <summary>
    /// The maximum height in pixels.
    /// </summary>
    public int MaxHeight = int.MaxValue;
    /// <summary>
    /// The grow direction on the x axis, used when the shape doesn't meet the MinWidth or MaxWidth.
    /// </summary>
    public Alignment GrowDirectionX;
    /// <summary>
    /// The grow direction on the y axis, used when the shape doesn't meet the MinHeight or MaxHeight.
    /// </summary>
    public Alignment GrowDirectionY;
    /// <summary>
    /// Decides where in the spacer's space elements should be cut off.
    /// </summary>
    public Box2 ScissorRegion = new(Vector2.NegativeInfinity, Vector2.PositiveInfinity);

    protected override UIElementSize GetSizeConstraints() => new(GrowDirectionX, GrowDirectionY, MinWidth, MinHeight, MaxWidth, MaxHeight);

    protected override Box2 GetScissorRegion() => ScissorRegion;
}