using System.Text;
using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Windowing;

namespace SlopperEngine.UI;

public class TextField : Button
{
    /// <summary>
    /// The text that is editable/shown in the TextField.
    /// </summary>
    public string Text
    {
        get => _fullText;
        set
        {
            _fullText = value;
            _invalidateRenderer = true;
        }
    }

    /// <summary>
    /// The actual renderer of the text. Avoid setting the string in here, as it will not properly update.
    /// </summary>
    public TextBox TextRenderer { get; private set; }

    /// <summary>
    /// The maximum amount of characters shown in the text field.
    /// </summary>
    public int Length
    {
        get => _fieldLength;
        set
        {
            _fieldLength = value;
            _invalidateRenderer = true;
        }
    }

    /// <summary>
    /// The color of the overlay on selected text.
    /// </summary>
    public Color4 SelectionColor = new(0,1,1,.4f);

    string _fullText = "";
    int _fieldLength;

    bool _fieldSelected;
    int _shownTextOffset = 0;
    bool _invalidateRenderer;

    ColorRectangle _cursor;
    int _cursorPosition = -1;
    int _selectionLength = 0;

    
    public TextField(int length)
    {
        Length = length;
        TextRenderer = new();
        hiddenUIChildren.Add(TextRenderer);
        _cursor = new(default, Color4.White);
    }

    /// <summary>
    /// Call this after changing the font of the TextRenderer.
    /// </summary>
    public void ForceUpdateRenderer()
    {
        _invalidateRenderer = false;

        StringBuilder builder = new();
        int pos = _shownTextOffset;
        while (pos < _fullText.Length && pos < _fieldLength)
        {
            builder.Append(_fullText[pos]);
            pos++;
        }
        while (pos < _fieldLength)
        {
            builder.Append(' ');
            pos++;
        }

        TextRenderer.Text = builder.ToString();
    }

    void UpdateCursor()
    {
        if (_cursor.InScene && !_fieldSelected)
            _cursor.Remove();
        if (!_cursor.InScene && _fieldSelected)
            hiddenUIChildren.Add(_cursor);

        float cursorCharacterLength = _selectionLength;
        if (_selectionLength == 0)
        {
            cursorCharacterLength = .2f;
            _cursor.Color = (Color4)TextRenderer.TextColor;
        }
        else
            _cursor.Color = SelectionColor;

        float invFieldLength = 1f / _fieldLength;
        cursorCharacterLength *= invFieldLength;
        float cursorPos = _cursorPosition * invFieldLength;
        _cursor.LocalShape = new(cursorPos, 0.1f, cursorPos + cursorCharacterLength, 0.9f);
    }

    [OnInputUpdate]
    void Input(InputUpdateArgs args)
    {
        if (hovering)
        {
            if (args.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
            {
                _cursorPosition = GetCharOffsetFromMousePos(args.NormalizedMousePosition.X);
                _fieldSelected = true;
            }
            if (args.MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
            {
                _selectionLength = GetCharOffsetFromMousePos(args.NormalizedMousePosition.X) - _cursorPosition;
            }
        }

        if (!_fieldSelected) return;

        if (!hovering && args.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
        {
            _cursorPosition = -1;
            _fieldSelected = false;
        }

        foreach (var j in args.TextInputEvents)
        {
            if (j.CharacterIsAsKey)
            {
                switch (j.CharacterAsKey)
                {
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.X:
                        if (!j.AnyControlheld) break;

                        SelectionToClipboard();
                        // remove text here
                        break;
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.C:
                        if (!j.AnyControlheld) break;

                        SelectionToClipboard();
                        break;
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.V:
                        if (!j.AnyControlheld) break;

                        Text += MainContext.Instance.ClipboardString;
                        break;
                }
                continue;
            }
            else
            {
                if (j.TryGetAsChar(out char c)) Text += c;
                else Text += j.GetAsString();
            }
        }
    }

    void SelectionToClipboard()
    {
        if (_selectionLength == 0)
            return;

        int selectionMax = int.Max(_cursorPosition, _cursorPosition + _selectionLength);
        int selectionMin = int.Min(_cursorPosition, _cursorPosition + _selectionLength);
        if ((uint)selectionMax > _fullText.Length || (uint)selectionMin >= _fullText.Length)
            return;

        MainContext.Instance.ClipboardString = _fullText.Substring(selectionMin, selectionMax - selectionMin);
    }

    int GetCharOffsetFromMousePos(float mouseX)
    {
        mouseX *= 2;
        mouseX -= 1;
        float minX = LastGlobalShape.Min.X;
        float sizeX = LastGlobalShape.Size.X;
        mouseX -= minX;
        mouseX /= sizeX; // range 0..1
        mouseX *= Length;
        mouseX += .2f; // select slightly to the right
        return (int)mouseX;
    }

    protected override UIElementSize GetSizeConstraints()
    {
        if (_invalidateRenderer)
            ForceUpdateRenderer();

        UpdateCursor();
        return TextRenderer.LastSizeConstraints;
    }
}