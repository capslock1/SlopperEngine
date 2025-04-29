using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Graphics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.UI;

public abstract class Button : UIElement
{
    bool _hovering;

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
        if(lastRenderer == null) return;

        Vector2 mousePos = args.MouseState.Position * lastRenderer.GetPixelScale();
        mousePos -= new Vector2(1);
        mousePos.Y = -mousePos.Y;

        bool pHovering = _hovering;
        _hovering = lastGlobalShape.ContainsInclusive(mousePos);

        if(!_hovering && !pHovering)
            return;

        bool hoverChange = pHovering != _hovering;
        for(int i = 0; i<=(int)MouseButton.Last; i++)
        {
            var butt = (MouseButton)i;
            if(args.MouseState.IsButtonPressed(butt))
                OnClick(butt);
            
            if(args.MouseState.IsButtonReleased(butt))
                OnRelease(butt);
        }

        if(hoverChange)
        {
            if(_hovering) 
                OnHoverStart();
            else OnHoverEnd();
        }
    }

    /// <summary>
    /// Gets called when the Button gets clicked using any mouse button.
    /// </summary>
    /// <param name="button">The mouse button that was clicked.</param>
    protected virtual void OnClick(MouseButton button){}
    
    /// <summary>
    /// Gets called when any mouse button lets go of the Button.
    /// </summary>
    /// <param name="button">The mouse button that was released.</param>
    protected virtual void OnRelease(MouseButton button){}
    
    /// <summary>
    /// Gets called when the mouse starts hovering over the Button.
    /// </summary>
    protected virtual void OnHoverStart(){}

    /// <summary>
    /// Gets called when the mouse stops hovering over the button.
    /// </summary>
    protected virtual void OnHoverEnd(){}
}