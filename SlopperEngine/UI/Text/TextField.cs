using System.Text;
using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Windowing;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;

namespace SlopperEngine.UI.Text;

/// <summary>
/// An editable line of text.
/// </summary>
public class TextField : UIElement
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
            _cursorPosition = int.Min(_cursorPosition, value.Length);
            OnTextChanged?.Invoke();
        }
    }

    /// <summary>
    /// The actual renderer of the text. After making any changes, call "UpdateTextRenderer()". Setting the Text string of the TextRenderer does not work.
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
    public Color4 SelectionColor = new(0, 1, 1, 0.4f);

    /// <summary>
    /// Gets called when the text of the text field gets changed in any way.
    /// </summary>
    public event Action? OnTextChanged;

    /// <summary>
    /// Gets called when the textbox is clicked off, or if the Enter key gets hit.
    /// </summary>
    public event Action? OnEntered;

    string _fullText = "";
    int _fieldLength;

    bool _fieldSelected;
    int _shownTextOffset = 0;
    bool _invalidateRenderer;

    ColorRectangle _cursor;
    int _cursorPosition = -1;
    int _selectionLength = 0;

    int _selectionMax => int.Min(_fullText.Length+1, int.Max(_cursorPosition, _cursorPosition + _selectionLength));
    int _selectionMin => int.Max(0, int.Min(_cursorPosition, _cursorPosition + _selectionLength));

    public TextField(int length)
    {
        Length = length;
        TextRenderer = new();
        internalUIChildren.Add(TextRenderer);
        _cursor = new(default, Color4.White);
    }

    /// <summary>
    /// Call this after making any changes to the text renderer.
    /// </summary>
    public void UpdateTextRenderer()
    {
        _invalidateRenderer = true;
    }

    void ForceUpdateRenderer()
    {
        _invalidateRenderer = false;

        StringBuilder builder = new();
        int pos = 0;
        if (_shownTextOffset < 0)
            _shownTextOffset = 0;
        while (pos + _shownTextOffset < _fullText.Length && pos < _fieldLength)
        {
            builder.Append(_fullText[pos + _shownTextOffset]);
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
        if (_cursorPosition + _selectionLength - _shownTextOffset >= _fieldLength)
        {
            _shownTextOffset += _cursorPosition + _selectionLength - _shownTextOffset - _fieldLength + 1;
            _invalidateRenderer = true;
        }
        if (_shownTextOffset > 0 && _cursorPosition + _selectionLength - _shownTextOffset < 1)
        {
            _shownTextOffset += _cursorPosition + _selectionLength - _shownTextOffset - 1;
            _invalidateRenderer = true;
        }

        if (_cursor.InScene && !_fieldSelected)
            _cursor.Remove();
        if (!_cursor.InScene && _fieldSelected)
            internalUIChildren.Add(_cursor);

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
        float cursorPos = (_cursorPosition - _shownTextOffset) * invFieldLength;
        _cursor.LocalShape = new(
            float.Clamp(cursorPos, 0, 1),
            0.1f,
            float.Clamp(cursorPos + cursorCharacterLength, 0, 1),
            0.9f);
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        if (e.PressedButton == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
        {
            _cursorPosition = GetCharOffsetFromMousePos(e.NDCPosition.X);
            _selectionLength = 0;
            _fieldSelected = true;
            e.Use();
            return;
        }
        if (e.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
        {
            _selectionLength = int.Max(-_cursorPosition, GetCharOffsetFromMousePos(e.NDCPosition.X) - _cursorPosition);
            e.Use();
            return;
        }
    }

    [OnInputUpdate]
    void Input(InputUpdateArgs args)
    {
        if (!_fieldSelected)
            return;

        if (!LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One) && args.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
        {
            _cursorPosition = -1;
            _selectionLength = 0;
            _fieldSelected = false;
            _shownTextOffset = 0;
            _invalidateRenderer = true;
            OnEntered?.Invoke();
        }

        foreach (var j in args.TextInputEvents)
        {
            if (j.CharacterIsAsKey)
            {
                switch (j.CharacterAsKey)
                {
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace: // remove previous character or all selected characters
                        if (j.AnyControlheld)
                            SelectUntilNextWhitespace(true);
                        if (_selectionLength != 0)
                        {
                            DeleteSelected();
                            break;
                        }
                        if (_cursorPosition < 1) break;
                        if (_fullText.Length < 1) break;

                        _invalidateRenderer = true;
                        Text = _fullText.Substring(0, _cursorPosition - 1) + _fullText.Substring(_cursorPosition);
                        if(_cursorPosition != _fullText.Length)
                            _cursorPosition--;
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete: // remove next character or all selected characters
                        if (j.AnyControlheld)
                            SelectUntilNextWhitespace(false);
                        if (_selectionLength != 0)
                        {
                            DeleteSelected();
                            break;
                        }
                        if (_cursorPosition >= _fullText.Length) break;
                        if (_fullText.Length < 1) break;

                        _invalidateRenderer = true;
                        Text = _fullText.Substring(0, _cursorPosition) + _fullText.Substring(_cursorPosition + 1);
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.A: // select all
                        if (!j.AnyControlheld) break;
                        _cursorPosition = 0;
                        _selectionLength = _fullText.Length;
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.X: // cut selection out
                        if (!j.AnyControlheld) break;
                        if (_selectionLength == 0) break;

                        SelectionToClipboard();
                        DeleteSelected();
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.C: // copy selection to clipboard
                        if (!j.AnyControlheld) break;

                        SelectionToClipboard();
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.V: // paste clipboard
                        if (!j.AnyControlheld) break;

                        string clipboard = MainContext.Instance.ClipboardString;
                        if (clipboard.Length == 0)
                        {
                            DeleteSelected();
                            break;
                        }
                        _invalidateRenderer = true;
                        Text = _fullText.Substring(0, _selectionMin) + clipboard + _fullText.Substring(_selectionMax);
                        _cursorPosition = _selectionMin + clipboard.Length;
                        _selectionLength = 0;
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left:
                        if (j.AnyShiftHeld)
                        {
                            if (j.AnyControlheld)
                            {
                                SelectUntilNextWhitespace(true);
                                break;
                            }
                            if (_selectionLength > 0 || _selectionMin > 0)
                                _selectionLength--;
                            break;
                        }
                        if (_selectionLength == 0)
                        {
                            if (j.AnyControlheld)
                                _cursorPosition = IndexOfNextWhitespace(_cursorPosition, true);
                            else _cursorPosition = int.Max(0, _cursorPosition - 1);
                        }
                        else
                        {
                            _cursorPosition = _selectionMin;
                            _selectionLength = 0;
                        }
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right:
                        if (j.AnyShiftHeld)
                        {
                            if (j.AnyControlheld)
                            {
                                SelectUntilNextWhitespace(false);
                                break;
                            }
                            if (_selectionLength < 0 || _selectionMax < _fullText.Length)
                                _selectionLength++;
                            break;
                        }
                        if (_selectionLength == 0)
                        {
                            if (j.AnyControlheld)
                                _cursorPosition = IndexOfNextWhitespace(_cursorPosition, false);
                            else _cursorPosition = int.Min(_fullText.Length, _cursorPosition + 1);
                        }
                        else
                        {
                            _cursorPosition = _selectionMax;
                            _selectionLength = 0;
                        }
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Home:
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up:
                        _cursorPosition = 0;
                        _selectionLength = 0;
                        _invalidateRenderer = true;
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.End:
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down:
                        _cursorPosition = _fullText.Length;
                        _selectionLength = 0;
                        _invalidateRenderer = true;
                        break;

                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter:
                    case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEnter:
                        _cursorPosition = -1;
                        _shownTextOffset = 0;
                        _selectionLength = 0;
                        _invalidateRenderer = true;
                        _fieldSelected = false;
                        OnEntered?.Invoke();
                        break;
                }
                continue;
            }
            else
            {
                if (_cursorPosition >= _fullText.Length && _selectionLength == 0)
                {
                    if (j.TryGetAsChar(out char c)) Text += c;
                    else Text += j.GetAsString();
                    _cursorPosition = _fullText.Length;
                    _selectionLength = 0;
                }
                else
                {
                    _invalidateRenderer = true;
                    string added = j.GetAsString();
                    Text = _fullText.Substring(0, _selectionMin) + added + _fullText.Substring(_selectionMax);
                    _cursorPosition = _selectionMin + added.Length;
                    _selectionLength = 0;
                }
            }
        }
    }

    void SelectUntilNextWhitespace(bool backwards)
    {
        int startPos = _cursorPosition;
        if (_selectionLength < 0)
            startPos = _selectionMin;
        if (_selectionLength > 0)
            startPos = _selectionMax;

        int index = IndexOfNextWhitespace(startPos, backwards);
        _selectionLength = index - _cursorPosition;
    }

    int IndexOfNextWhitespace(int indexToSearchFrom, bool backwards)
    {
        int increment = backwards ? -1 : 1;
        bool encounteredNormalChar = false;
        if (!((uint)indexToSearchFrom < _fullText.Length) || backwards)
            indexToSearchFrom += increment;

        while ((uint)indexToSearchFrom < _fullText.Length)
        {
            var ch = _fullText[indexToSearchFrom];

            if (char.IsWhiteSpace(ch))
            {
                if (encounteredNormalChar)
                    return int.Clamp(indexToSearchFrom + (backwards ? 1 : 0), 0, _fullText.Length);
            }
            else encounteredNormalChar = true;

            indexToSearchFrom += increment;
        }
        return int.Clamp(indexToSearchFrom, 0, _fullText.Length);
    }

    void DeleteSelected()
    {
        if (_selectionLength == 0) return;

        int start = _selectionMin;
        int end = _selectionMax;
        _cursorPosition = int.Max(0,start);
        _selectionLength = 0;
        _invalidateRenderer = true;
        Text = _fullText.Substring(0, start) + _fullText.Substring(end);
    }

    void SelectionToClipboard()
    {
        if (_selectionLength == 0)
            return;

        if ((uint)_selectionMax > _fullText.Length || (uint)_selectionMin >= _fullText.Length)
            return;

        MainContext.Instance.ClipboardString = _fullText.Substring(_selectionMin, _selectionMax - _selectionMin);
    }

    int GetCharOffsetFromMousePos(float mouseX)
    {
        float minX = LastGlobalShape.Min.X;
        float sizeX = LastGlobalShape.Size.X;
        mouseX -= minX;
        mouseX /= sizeX; // range 0..1
        mouseX *= Length;
        mouseX += .2f; // select slightly to the right
        return int.Clamp((int)mouseX + _shownTextOffset, 0, _fullText.Length);
    }

    protected override UIElementSize GetSizeConstraints()
    {
        UpdateCursor();

        if (_invalidateRenderer)
            ForceUpdateRenderer();

        return TextRenderer.LastSizeConstraints;
    }
}