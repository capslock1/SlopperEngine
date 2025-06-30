
using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Layout;

/// <summary>
/// Lays out children in an axis aligned line.
/// </summary>.
public class LinearArrangedLayout : LayoutHandler
{
    /// <summary>
    /// Whether the elements should be arranged horizontally or vertically.
    /// </summary>
    public bool IsLayoutHorizontal;

    /// <summary>
    /// Whether the elements should be arranged from max or from min.
    /// </summary>
    public bool StartAtMax = true;

    /// <summary>
    /// How the elements should be arranged tangent to their axis.
    /// </summary>
    public Alignment ChildAlignment = Alignment.Min;

    /// <summary>
    /// The padding to use between elements.
    /// </summary>
    public UISize Padding = UISize.FromPixels(new(4,4));

    public override void LayoutChildren(UIElement owner, Box2 ownerGlobalShape)
    {
        var invOwnerSize = Vector2.One / ownerGlobalShape.Size;

        float position = StartAtMax ? 1 : 0;
        float direction = StartAtMax ? -1 : 1;

        Vector2 padding = Padding.GetLocalSize(ownerGlobalShape, owner.LastRenderer);
        if (StartAtMax)
        {
            if (IsLayoutHorizontal)
                padding.X = -padding.X;
            else padding.Y = -padding.Y;
        }
        switch (ChildAlignment)
        {
            default:
                if (IsLayoutHorizontal)
                    padding.Y = 0;
                else padding.X = 0;
                break;
            case Alignment.Min:
                break;
            case Alignment.Max:
                if (IsLayoutHorizontal)
                    padding.Y = -padding.Y;
                else padding.X = -padding.X;
                break;
        }

        var children = owner.UIChildren;
        bool flip = StartAtMax ^ IsLayoutHorizontal;
        for (int i = flip ? 0 : children.Count - 1; flip ? i < children.Count : i >= 0; i += flip ? 1 : -1) // disgusting loop
        {
            var ch = children[i];
            var globalBounds = ch.LastChildrenBounds;
            globalBounds.Extend(ch.LastGlobalShape.Min);
            globalBounds.Extend(ch.LastGlobalShape.Max);

            var localBounds = new Box2(MapToLocal(globalBounds.Min), MapToLocal(globalBounds.Max));
            var corner = StartAtMax ? localBounds.Max : localBounds.Min;
            float posDist = position - (IsLayoutHorizontal ? corner.X : corner.Y);

            float alignDist;
            switch (ChildAlignment)
            {
                default:
                    alignDist = 0.5f - (IsLayoutHorizontal ? localBounds.Center.Y : localBounds.Center.X);
                    break;
                case Alignment.Min:
                    alignDist = IsLayoutHorizontal ? -localBounds.Min.Y : -localBounds.Min.X;
                    break;
                case Alignment.Max:
                    alignDist = 1 - (IsLayoutHorizontal ? localBounds.Max.Y : localBounds.Max.X);
                    break;
            }
            ch.LocalShape.Center += padding + (IsLayoutHorizontal ? new Vector2(posDist, alignDist) : new Vector2(alignDist, posDist));

            position += direction * (IsLayoutHorizontal ? localBounds.Size.X : localBounds.Size.Y) + (IsLayoutHorizontal ? padding.X : padding.Y);
        }

        Vector2 MapToLocal(Vector2 globalPoint)
        {
            globalPoint -= ownerGlobalShape.Min;
            globalPoint *= invOwnerSize;
            return globalPoint;
        }
    }
}