using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.Engine;

public class InputUpdateArgs
{
    public readonly KeyboardState KeyboardState;
    public readonly MouseState MouseState;
    public InputUpdateArgs(KeyboardState keyboard, MouseState mouse)
    {
        KeyboardState = keyboard;
        MouseState = mouse;
    } 
}