
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.Windowing;

/// <summary>
/// A text input event, used when text is entered normally.
/// </summary>
/// <param name="Origin">The window that raised the event. </param>
/// <param name="CharacterAsUnicode">The character that was entered as a UTF-32 code point. Valid when ctrl is not held.</param>
/// <param name="CharacterAsKey">The character that was entered as a GLFW key. Valid when ctrl is held.</param>
/// <param name="LeftControlHeld">Whether the left control key was held.</param>
/// <param name="LeftShiftHeld">Whether the left shift key was held.</param>
/// <param name="LeftAltHeld">Whether the left alt key was held.</param>
/// <param name="RightControlHeld">Whether the right control key was held.</param>
/// <param name="RightShiftHeld">Whether the right shift key was held.</param>
/// <param name="RightAltHeld">Whether the right alt key was held.</param>
/// <param name="SuperKeyHeld">Whether the windows/command key was held.</param>
public readonly record struct TextInputEvent(
    Window Origin,
    int CharacterAsUnicode,
    Keys CharacterAsKey,
    bool LeftControlHeld,
    bool LeftShiftHeld,
    bool LeftAltHeld,
    bool RightControlHeld,
    bool RightShiftHeld,
    bool RightAltHeld,
    bool SuperKeyHeld
)
{
    /// <summary>
    /// Whether the left or right control keys are held.
    /// </summary>
    public bool AnyControlheld => LeftControlHeld || RightControlHeld;
    /// <summary>
    /// Whether the left or right shift keys are held.
    /// </summary>
    public bool AnyShiftHeld => LeftShiftHeld || RightShiftHeld;
    /// <summary>
    /// Whether the left or right alt keys are held.
    /// </summary>
    public bool AnyAltHeld => LeftAltHeld || RightAltHeld;

    /// <summary>
    /// Whether or not CharacterAsKey contains the value of the event. Generally applies when ctrl or super is held.
    /// </summary>
    public bool CharacterIsAsKey => CharacterAsUnicode == -1;

    /// <summary>
    /// Whether or not CharacterAsUnicode contains the value of the event. Generally applies when ctrl or super are not held.
    /// </summary>
    public bool CharacterIsAsUnicode => CharacterAsUnicode != -1;

    /// <summary>
    /// Tries to get the input event as a character. May be capitalised, if shift is held.
    /// </summary>
    /// <param name="character">The character stored in the event.</param>
    /// <returns>Whether or not the character is a valid single char.</returns>
    public bool TryGetAsChar(out char character)
    {
        character = (char)CharacterAsUnicode;
        return CharacterAsUnicode < char.MaxValue && CharacterAsUnicode >= 0;
    }

    /// <summary>
    /// Gets the input event as a string, for if the value is comprised of multiple char instances (in case of emojis, for example).
    /// </summary>
    public string GetAsString() => char.ConvertFromUtf32(CharacterAsUnicode);
}