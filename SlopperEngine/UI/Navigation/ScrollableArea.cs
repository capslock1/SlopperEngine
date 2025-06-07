using OpenTK.Mathematics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Navigation;

public class ScrollableArea : UIElement
{
    public override ChildList<UIElement> UIChildren => _movingArea.UIChildren;

    UIElement _movingArea;

    public ScrollableArea(Box2 shape) : base(shape)
    {
        internalUIChildren.Add(_movingArea = new());
    }

    protected override Box2 GetScissorRegion() => new(0, 0, 1, 1);

    protected override void HandleEvent(ref MouseEvent e)
    {
        
    }
}