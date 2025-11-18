using OpenTK.Mathematics;
using SlopperEngine.Rendering;
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
    public bool IsLayoutHorizontal = false;

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

    public override bool AddsPadding(Box2 parentShape, UIRenderer renderer, out Vector2 currentlyAddedPadding)
    {
        currentlyAddedPadding = Padding.GetGlobalSize(parentShape, renderer);
        return true;
    }

    public override void LayoutChildren<TLayoutKey>(UIElement owner, ref TLayoutKey layoutKey, Box2 parentShape, UIRenderer renderer)
    {
        var parentSize = parentShape.Size;
        float parentWidth = IsLayoutHorizontal ? parentSize.Y : parentSize.X;
        float direction = StartAtMax ? -1 : 1;

        Vector2 paddingVector = Padding.GetGlobalSize(parentShape, renderer);
        if (StartAtMax)
        {
            if (IsLayoutHorizontal)
                paddingVector.X = -paddingVector.X;
            else paddingVector.Y = -paddingVector.Y;
        }

        float forwardPos; // the position *along* the layout
        float tangentPos; // the position *tangent to* the layout
        switch(ChildAlignment)
        {
            default: tangentPos = IsLayoutHorizontal ? parentShape.Center.Y : parentShape.Center.X; break; 
            case Alignment.Min: tangentPos = IsLayoutHorizontal ? parentShape.Min.Y : parentShape.Min.X; break;
            case Alignment.Max: tangentPos = IsLayoutHorizontal ? parentShape.Max.Y : parentShape.Max.X; break;
        }
        float forwardPadding;
        float tangentPadding;
        if (IsLayoutHorizontal)
        {
            forwardPadding = paddingVector.X;
            tangentPadding = paddingVector.Y;
            forwardPos = StartAtMax ? parentShape.Max.X : parentShape.Min.X;
        }
        else
        {
            forwardPadding = paddingVector.Y;
            tangentPadding = paddingVector.X;
            forwardPos = StartAtMax ? parentShape.Max.Y : parentShape.Min.Y;
        }

        var children = owner.UIChildren;
        for (int i = 0; i < children.Count; i++) 
        {
            var ch = children[i];

            forwardPos += forwardPadding;

            var desiredSize = ch.LocalShape.Size * parentSize;
            if (IsLayoutHorizontal)
                desiredSize = desiredSize.Yx; // rotate

            desiredSize.X = float.Max(0, float.Min(desiredSize.X, parentWidth - 2 * tangentPadding)); // make sure the width doesnt exceed the container's + padding on both sides

            Box2 globalShape;
            switch (ChildAlignment)
            {
                default:
                    float hWidth = 0.5f * desiredSize.X;
                    globalShape = new(
                        tangentPos - hWidth, forwardPos,
                        tangentPos + hWidth, forwardPos + desiredSize.Y * direction);
                    break;

                case Alignment.Min:
                    globalShape = new(
                        tangentPos + tangentPadding, forwardPos,
                        tangentPos + desiredSize.X, forwardPos + desiredSize.Y * direction);
                    break;

                case Alignment.Max:
                    globalShape = new(
                        tangentPos + tangentPadding, forwardPos,
                        tangentPos - desiredSize.X, forwardPos + desiredSize.Y * direction);
                    break;
            }

            if(IsLayoutHorizontal)
                globalShape = new(globalShape.Min.Yx, globalShape.Max.Yx); // rotate back

            layoutKey.SetGlobalShape(i, ref globalShape, renderer);

            var finalSize = Vector2.ComponentMax(globalShape.Size, ch.LastChildrenBounds.Size);
            forwardPos += (IsLayoutHorizontal ? finalSize.X : finalSize.Y) * direction; // add the child's final size to the position
        }
    }
}