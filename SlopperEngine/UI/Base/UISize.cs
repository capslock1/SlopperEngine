
using OpenTK.Mathematics;
using SlopperEngine.Rendering;

namespace SlopperEngine.UI.Base;

/// <summary>
/// Represents a size in UI, either relative to the parent or a constant size in pixels.
/// </summary>
public readonly record struct UISize
{
    readonly Mode _mode;
    readonly Vector2i _sizeInt;
    readonly Vector2 _sizeFloat;

    private UISize(Mode mode, Vector2i sizeInt, Vector2 sizeFloat)
    {
        _mode = mode;
        _sizeInt = sizeInt;
        _sizeFloat = sizeFloat;
    }

    /// <summary>
    /// Creates a UISize that has a constant size in pixels on the target screen.
    /// </summary>
    /// <param name="sizePixels">The size in pixels.</param>
    public static UISize FromPixels(Vector2i sizePixels) => new(Mode.PixelSize, sizePixels, default);

    /// <summary>
    /// Creates a UISize that has a size relative to its parent.
    /// </summary>
    /// <param name="sizeRelative">The size relative to the parent in range 0-1.</param>
    public static UISize RelativeToParent(Vector2 sizeRelative) => new(Mode.RelativeToParent, default, sizeRelative);
    
    /// <summary>
    /// Creates a UISize that has a size relative to its UI root.
    /// </summary>
    /// <param name="sizeGlobal">The size relative to global space, range 0-1.</param>
    public static UISize RelativeToRoot(Vector2 sizeGlobal) => new(Mode.RelativeToRoot, default, sizeGlobal);

    /// <summary>
    /// Gets this UISize's size relative to a parent's global shape.
    /// </summary>
    public readonly Vector2 GetLocalSize(Box2 parentGlobalShape, UIRenderer? renderer)
    {
        switch (_mode)
        {
            default:
                return _sizeFloat;

            case Mode.PixelSize:
                var invElementSize = _sizeInt / parentGlobalShape.Size;
                if (renderer == null)
                    return new Vector2(0.01f, 0.01f) * invElementSize;
                return renderer.GetPixelScale() * invElementSize;

            case Mode.RelativeToRoot:
                return _sizeFloat / parentGlobalShape.Size;
        }
    }

    enum Mode
    {
        RelativeToParent,
        RelativeToRoot,
        PixelSize
    }
}