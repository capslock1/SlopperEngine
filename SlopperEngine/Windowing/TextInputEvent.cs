
namespace SlopperEngine.Windowing;

/// <summary>
/// A text input event, used when text is entered normally.
/// </summary>
/// <param name="Origin">The window that raised the event. </param>
/// <param name="Character">The character that was entered.</param>
/// <param name="LeftControlHeld">Whether the left control key was held.</param>
/// <param name="LeftShiftHeld">Whether the left shift key was held.</param>
/// <param name="LeftAltHeld">Whether the left alt key was held.</param>
/// <param name="RightControlHeld">Whether the right control key was held.</param>
/// <param name="RightShiftHeld">Whether the right shift key was held.</param>
/// <param name="RightAltHeld">Whether the right alt key was held.</param>
/// <param name="SuperKeyHeld">Whether the windows/command key was held.</param>
public readonly record struct TextInputEvent(
    Window Origin,
    int UnicodeCharacter,
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
    /// Tries to get the input event as a character. May be capitalised, if shift is held.
    /// </summary>
    /// <param name="character">The character stored in the event.</param>
    /// <returns>Whether or not the character is a valid single char.</returns>
    public bool TryGetAsChar(out char character)
    {
        character = (char)UnicodeCharacter;
        return UnicodeCharacter < char.MaxValue;
    }

    /// <summary>
    /// Gets the input event as a string, for if the value is comprised of multiple char instances (in case of emojis, for example).
    /// </summary>
    public string GetAsString() => char.ConvertFromUtf32(UnicodeCharacter);
}