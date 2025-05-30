using OpenTK.Mathematics;
using SlopperEngine.Core;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.UI.Base;

public abstract class Button : UIElement
{
    protected bool hovering { get; private set; }

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
        if(LastRenderer == null) return;

        Vector2 mousePos = args.NormalizedMousePosition * 2;
        mousePos -= new Vector2(1);

        bool pHovering = hovering;
        hovering = LastGlobalShape.ContainsInclusive(mousePos);

        if(!hovering && !pHovering)
            return;

        bool hoverChange = pHovering != hovering;
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
            if(hovering) 
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