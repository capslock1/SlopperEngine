using OpenTK.Mathematics;

namespace SlopperEngine.UI.Style;

/// <summary>
/// A basic style struct. Contains a handful of colors for consistent UI.
/// </summary>
public class BasicStyle
{
    /// <summary>
    /// Stronger version of the background color (darkest assuming dark mode).
    /// </summary>
    public readonly Color4 BackgroundStrong;
    /// <summary>
    /// Weaker version of the background color.
    /// </summary>
    public readonly Color4 BackgroundWeak;
    /// <summary>
    /// Weaker version of the foreground color.
    /// </summary>
    public readonly Color4 ForegroundWeak;
    /// <summary>
    /// Stronger version of the foreground color (for selected foreground elements, brightest assuming dark mode). 
    /// </summary>
    public readonly Color4 ForegroundStrong;
    /// <summary>
    /// The brightest color in the style.
    /// </summary>
    public readonly Color4 Tint;
    /// <summary>
    /// A color for marking selected elements (like text).
    /// </summary>
    public readonly Color4 Highlight;

    /// <summary>
    /// The style objects use by default.
    /// </summary>
    public static readonly BasicStyle DefaultStyle = new(
        new(0.1f, 0.1f, 0.1f, 0.75f),
        new(0.2f, 0.2f, 0.2f, 0.75f),
        new(0.4f, 0.4f, 0.4f, 1),
        new(0.5f, 0.5f, 0.5f, 1),
        new(1f, 1, 1, 1),
        new(0, 1, 1, 0.4f));

    /// <summary>
    /// A fully transparent style. Useful for fully overriding the look of certain elements.
    /// </summary>
    public static readonly BasicStyle FullyTransparent = new(default, default, default, default, default, default);

    /// <summary>
    /// Creates a style using ALL the colors.
    /// </summary>
    public BasicStyle(Color4 backgroundStrong, Color4 backgroundWeak, Color4 foregroundWeak, Color4 foregroundStrong, Color4 tint, Color4 highlight)
    {
        BackgroundWeak = backgroundWeak;
        BackgroundStrong = backgroundStrong;
        ForegroundWeak = foregroundWeak;
        ForegroundStrong = foregroundStrong;
        Tint = tint;
        Highlight = highlight;
    }
}