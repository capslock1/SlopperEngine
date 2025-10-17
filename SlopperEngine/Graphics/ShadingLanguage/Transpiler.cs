

using System.Collections.Generic;

namespace SlopperEngine.Graphics.ShadingLanguage;

/// <summary>
/// Parses .sesl source code into SlopperShader objects.
/// </summary>
public static class Transpiler
{
    static List<string> _errors = new();
    static bool _currentlyParsing = false;

    /// <summary>
    /// Adds an error to the list. Will be thrown once parsing is complete.
    /// </summary>
    public static void AddError(string error)
    {
        if(_currentlyParsing)
            _errors.Add(error);
        else throw new ParseFailedException(["Some parsing error was thrown while not parsing: "+error]);
    }

    /// <summary>
    /// For when the parsing error is so bad you dont even know how to continue parsing. Immediately throws this and previous errors.
    /// </summary>
    public static void AddCriticalError(string error)
    {
        _errors.Add(error);
        throw new ParseFailedException(_errors);
    }

    /// <summary>
    /// Parses the source code from a .sesl file, and creates a SlopperShader.
    /// </summary>
    /// <param name="source">The source code to use.</param>
    /// <exception cref="ParseFailedException"></exception>
    public static SyntaxTree Parse(string source)
    {
        _currentlyParsing = true;

        //let me break it down for you mark
        TokenScanner scanner = new(source);
        //foreach(var tok in scanner.Tokens)
        //    Console.WriteLine($"{tok.type}, {tok.specificType}: {scanner.GetTokenName(tok)}");

        SyntaxTree global = new(scanner);

        if(_errors.Count != 0)
        {
            var res = new ParseFailedException(_errors);
            _errors = new();
            throw res;
        }

        _currentlyParsing = false;

        return global;
    }

}