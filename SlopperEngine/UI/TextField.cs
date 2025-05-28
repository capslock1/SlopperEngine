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
            _invalidate = true;
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
            _invalidate = true;
        }
    }

    string _fullText = "";
    int _fieldLength;

    int _shownTextOffset = 0;
    int _cursorPosition = -1;
    int _selectionLength = 0;
    ColorRectangle _cursor;
    bool _invalidate;

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
    public void ForceUpdateTextBox()
    {
        _invalidate = false;

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

        if (_cursor.InScene && _cursorPosition < 0)
            _cursor.Remove();
        if (!_cursor.InScene && _cursorPosition > -1)
            hiddenUIChildren.Add(_cursor);

        float cursorCharacterLength = _selectionLength;
        if (_selectionLength < 1)
            cursorCharacterLength = .2f;

        cursorCharacterLength /= _fieldLength;
        _cursor.LocalShape = new();
    }

    [OnInputUpdate]
    void Input(InputUpdateArgs args)
    {
        foreach (var j in args.TextInputEvents)
        {
            if (j.CharacterIsAsKey)
            {
                switch (j.CharacterAsKey)
                {
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.C:
                        if (_selectionLength < 1) break;
                        if ((uint)_cursorPosition >= _fullText.Length) break;
                        if ((uint)(_cursorPosition + _selectionLength) >= _fullText.Length) break;
                        MainContext.Instance.ClipboardString = _fullText.Substring(_cursorPosition, _selectionLength);
                        break;
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.V:
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
        if (args.MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left) && !hovering)
        {
            _cursorPosition = -1;
            _selectionLength = 0;
        }
    }

    protected override UIElementSize GetSizeConstraints()
    {
        if (_invalidate)
        {
            _invalidate = false;
            ForceUpdateTextBox();
        }
        return TextRenderer.LastSizeConstraints;
    }
}