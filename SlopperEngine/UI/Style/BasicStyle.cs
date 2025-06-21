using OpenTK.Mathematics;

namespace SlopperEngine.UI.Style;

/// <summary>
/// A basic style struct. Contains a handful of colors for consistent UI.
/// </summary>
public class BasicStyle
{
    /// <summary>
    /// Weaker version of the background color.
    /// </summary>
    public readonly Color4 BackgroundWeak;
    /// <summary>
    /// Background color.
    /// </summary>
    public readonly Color4 Background;
    /// <summary>
    /// Stronger version of the background color.
    /// </summary>
    public readonly Color4 BackgroundStrong;
    /// <summary>
    /// Weaker version of the foreground color.
    /// </summary>
    public readonly Color4 ForegroundWeak;
    /// <summary>
    /// Foreground color.
    /// </summary>
    public readonly Color4 Foreground;
    /// <summary>
    /// Stronger version of the foreground color (for selected foreground elements). 
    /// </summary>
    public readonly Color4 ForegroundStrong;
    /// <summary>
    /// The brightest color in the style.
    /// </summary>
    public readonly Color4 Tint = Color4.White;
    /// <summary>
    /// A color for marking selected elements (like text).
    /// </summary>
    public readonly Color4 Highlight = new(0, 1, 1, 0.4f);

    /// <summary>
    /// The style objects use by default.
    /// </summary>
    public static readonly BasicStyle DefaultStyle = new(new(0.1f,0.1f,0.1f,0.5f), new(0.5f,0.5f,0.5f,0.5f));

    /// <summary>
    /// Creates a style using two colors. The strong variants may clip.
    /// </summary>
    public BasicStyle(Color4 background, Color4 foreground)
    {
        BackgroundWeak = (Color4)((Vector4)background * 0.8f);
        BackgroundWeak.A = background.A;
        Background = background;
        BackgroundStrong = (Color4)((Vector4)background * 1.2f);
        BackgroundStrong.A = background.A;
        ForegroundWeak = (Color4)((Vector4)foreground * 0.8f);
        ForegroundWeak.A = foreground.A;
        Foreground = foreground;
        ForegroundStrong = (Color4)((Vector4)foreground * 1.2f);
        ForegroundStrong.A = foreground.A;
    }

    /// <summary>
    /// Creates a style using ALL the colors.
    /// </summary>
    public BasicStyle(Color4 backgroundWeak, Color4 background, Color4 backgroundStrong, Color4 foregroundWeak, Color4 foreground, Color4 foregroundStrong, Color4 tint, Color4 highlight)
    {
        BackgroundWeak = backgroundWeak;
        Background = background;
        BackgroundStrong = backgroundStrong;
        ForegroundWeak = foregroundWeak;
        Foreground = foreground;
        ForegroundStrong = foregroundStrong;
        Tint = tint;
        Highlight = highlight;
    }
}