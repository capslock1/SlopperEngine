using SlopperEngine.Core;
using SlopperEngine.Windowing;

namespace SlopperEngine.UI;

public class TextField : Button
{
    /// <summary>
    /// The text that is editable/shown in the TextField.
    /// </summary>
    public string Text = "";
    /// <summary>
    /// The actual renderer of the text. Avoid setting the string in here, as it will not work.
    /// </summary>
    public TextBox TextRenderer { get; private set; }
    /// <summary>
    /// The maximum amount of characters shown in the text field.
    /// </summary>
    public int MaximumLength;

    int _shownTextOffset = 0;
    int _cursorPosition = -1;
    int _selectionLength = 0;

    public TextField(int maximumLength)
    {
        MaximumLength = maximumLength;
        TextRenderer = new();
        hiddenUIChildren.Add(TextRenderer);
    }

    void UpdateTextBox()
    {

    }

    [OnInputUpdate]
    void Input(InputUpdateArgs args)
    {
        foreach (var j in (ReadOnlySpan<TextInputEvent>)args.TextInputEvents)
        {
            if (j.TryGetAsChar(out char c)) Text += c;
            else Text += j.GetAsString();
        }
        if (args.MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left) && !hovering)
        {
            _cursorPosition = -1;
            _selectionLength = 0;
        }
    }
}