using System;
using System.Diagnostics.CodeAnalysis;

namespace SlopperEngine.Graphics.ShadingLanguage;

/// <summary>
/// The description of a token, with essential information for the compiler.
/// </summary>
public struct Token : IEquatable<Token>
{
    public TokenType Type;
    public SpecificTokenType SpecificType = SpecificTokenType.NotIndexed;
    public int PositionInSource;
    public int Length;
    public int LineNumber;
    public int CharNumber;
    public Token()
    {
        Type = TokenType.None;
        PositionInSource = -1;
        Length = -1;
        LineNumber = -1;
        CharNumber = -1;
    }
    public Token(TokenType t, int posInSource, int len, int line, int charN)
    {
        Type = t;
        PositionInSource = posInSource;
        Length = len;
        LineNumber = line;
        CharNumber = charN;
    }

    public bool Equals(Token other) => other.Type == Type && other.SpecificType == SpecificType && PositionInSource == other.PositionInSource && other.Length == Length; // no need to implement linenumber and charnumber
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Token t && t.Equals(this);
    public override int GetHashCode() => (Type, SpecificType, PositionInSource, Length).GetHashCode();
}