
namespace SlopperEngine.Graphics.ShadingLanguage;

/// <summary>
/// Thrown by the .sesl parser when any errors in syntax occur.
/// </summary>
[Serializable]
public class ParseFailedException : Exception
{
    public List<string> Errors;
    public ParseFailedException(List<string> errors)
    {
        Errors = errors;
    }
    public override string Message {
        get{
            string baseMessage = base.Message;
            baseMessage += "\nParsing errors: \n";
            baseMessage += string.Join("\n", Errors);
            return baseMessage;
        }
    }
}