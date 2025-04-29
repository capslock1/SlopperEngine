using System.Runtime.CompilerServices;

namespace SlopperEngine.Graphics.ShadingLanguage;

/// <summary>
/// Mostly extensions for chars, to assist the scanning stage of the parser.
/// </summary>
public static class TypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanStartIdentifier(this char c)
    {
        if(char.IsLetter(c)) return true;
        if(c == '_') return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllowedInIdentifier(this char c)
    {
        if(char.IsLetterOrDigit(c)) return true;
        if(c == '_') return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AllowedInConstant(this char c)
    {
        if(char.IsLetterOrDigit(c)) return true;
        if(c == '_') return true;
        if(c == '.') return true;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOperator(this char c)
    {
        switch(c)
        {
            default:
            return false;
            case '=': //equals

            case '+': //usual arithmetic
            case '-':
            case '*':
            case '/':
            case '%':

            case '>': //comparison
            case '<':

            case '&': //bitwise
            case '|':
            case '~':
            case '^': 

            case '?': //ternary operator
            case ':': //and also switch statement i suppose
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSeperator(this char c)
    {
        switch(c)
        {
            default: return false;
            case '(':
            case ')':
            case '{':
            case '}':
            case '[':
            case ']':
            case ';':
            case ',':
            case '.':
            return true;
        }
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TokenType StartWord(this char c)
    {
        if(c.CanStartIdentifier()) return TokenType.Identifier;
        if(char.IsDigit(c)) return TokenType.Literal;
        if(c.IsOperator()) return TokenType.Operator;
        if(c.IsSeperator()) return TokenType.Seperator;
        return TokenType.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FitsInWord(this char c, TokenType word)
    {
        switch(word)
        {
            default:
            return false;
            case TokenType.Identifier:
            return c.AllowedInIdentifier();
            case TokenType.Operator:
            return c.IsOperator();
            case TokenType.Literal:
            return c.AllowedInConstant();
        }
    }
}