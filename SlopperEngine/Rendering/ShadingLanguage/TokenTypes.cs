namespace SlopperEngine.Rendering.ShadingLanguage;

//aspecific token types - mostly helper type stuff
public enum TokenType
{
    Identifier, //function names, variable names, type names 
    Operator, //everything in char.IsOperator()
    Literal, //numbers (and would be strings if shaders had those (lol))
    Seperator, // (){}[];,.
    Keyword, //if, else, for, whatever
    None, //invalid words, spaces
}


//here come the more specific types - these are for syntactical analysis to worry about
public enum SpecificTokenType
{
    NotIndexed,

    //literals
    Number, 
    True, False,
    
    //seperators
    ParenthesisOpen, ParenthesisClose, 
    BracketOpen, BracketClose, 
    SquareBracketOpen, SquareBracketClose,
    Semicolon, Comma, Period,

    //keywords
    For, While, 
    If, Else, 
    Switch, Case, Break,
    Return, 
    Struct, 

    //one char operators
    Assign, 
    Plus, Minus, Multiply, Divide, Modulo, 
    GreaterThan, SmallerThan, 
    AND, OR, XOR, Tilde, Negation, //NOT is ~ (for ints), Negation is ! (for bools)
    QuestionMark, Colon,

    //two char operators
    Equality, Inequality,
    Increment, Decrement,
    AndAnd, OrOr, XorXor,
    ShiftLeft, ShiftRight,
    GreaterOrEqual, SmallerOrEqual,
    AdditionAssign, SubtractionAssign, MultiplyAssign, DivideAssign, ModuloAssign,
    ANDAssign, ORAssign, XORAssign, TildeAssign,

    //three char operators
    ShiftLeftAssign, ShiftRightAssign,
}


/// <summary>
/// Offers static methods for interacting with token types.
/// </summary>
public static class TokenTypes
{
    /// <summary>
    /// Gets the SpecificTokenType of a token, taking only its TokenType and string into account.
    /// </summary>
    /// <param name="toSet">The token to detect the specific type of.</param>
    /// <param name="source">The source code.</param>
    public static SpecificTokenType GetSpecificTokenType(ref Token toSet, string source)
    {
        string substr;
        if(toSet.Type == TokenType.None) return SpecificTokenType.NotIndexed;
        if(toSet.Type == TokenType.Operator) return SpecificTokenType.NotIndexed;
        if(toSet.Type == TokenType.Literal) 
        {
            char startChar = source[toSet.PositionInSource];
            if(startChar == '.' || char.IsDigit(startChar))
                return SpecificTokenType.Number;
            switch(toSet.Length)
            {
                default:break;
                case 4:
                substr = source.Substring(toSet.PositionInSource, toSet.Length);
                if(substr == "true" || substr == "True")
                    return SpecificTokenType.True;
                break;
                case 5:
                substr = source.Substring(toSet.PositionInSource, 5);
                if(substr == "false" || substr == "False")
                    return SpecificTokenType.False;
                break;
            }
            return SpecificTokenType.NotIndexed;
        }
        switch(toSet.Length)
        {
            default: break;
            case 1:
            switch(source[toSet.PositionInSource])
            {
                default:break;
                case '(': return SpecificTokenType.ParenthesisOpen;
                case ')': return SpecificTokenType.ParenthesisClose;
                case '{': return SpecificTokenType.BracketOpen;
                case '}': return SpecificTokenType.BracketClose;
                case '[': return SpecificTokenType.SquareBracketOpen;
                case ']': return SpecificTokenType.SquareBracketClose;
                case ';': return SpecificTokenType.Semicolon;
                case ',': return SpecificTokenType.Comma;
                case '.': return SpecificTokenType.Period;
            }
            break;
            case 2:
            if(source.Substring(toSet.PositionInSource, 2) == "if")
                return SpecificTokenType.If;
            break;
            case 3:
            if(source.Substring(toSet.PositionInSource, 3) == "for")
                return SpecificTokenType.For;
            break;
            case 4:
            substr = source.Substring(toSet.PositionInSource, 4);
            if(substr == "else")
                return SpecificTokenType.Else;
            if(substr == "case")
                return SpecificTokenType.Case;
            break;
            case 5:
            substr = source.Substring(toSet.PositionInSource, 5);
            if(substr == "while")
                return SpecificTokenType.While;
            if(substr == "break")
                return SpecificTokenType.Break;
            break;
            case 6:
            substr = source.Substring(toSet.PositionInSource, 6);
            if(substr == "return")
                return SpecificTokenType.Return;
            if(substr == "struct")
                return SpecificTokenType.Struct;
            if(substr == "switch")
                return SpecificTokenType.Switch;
            break;
        }
        return SpecificTokenType.NotIndexed;
    }
    
    /// <summary>
    /// Gets a SpecificTokenType associated with operators of length 3.
    /// </summary>
    /// <returns>NotIndexed if this isnt a recognised operator.</returns>
    public static SpecificTokenType FromThreeCharOperator(char c1, char c2, char c3)
    {
        if(c1 != c2) return SpecificTokenType.NotIndexed;
        if(c3 != '=') return SpecificTokenType.NotIndexed;

        if(c1 == '<') return SpecificTokenType.ShiftLeftAssign;
        if(c1 == '>') return SpecificTokenType.ShiftRightAssign;
        
        return SpecificTokenType.NotIndexed;
    }
    /// <summary>
    /// Gets a SpecificTokenType associated with operators of length 2.
    /// </summary>
    /// <returns>NotIndexed if this isnt a recognised operator.</returns>
    public static SpecificTokenType FromTwoCharOperator(char c1, char c2)
    {
        if(c2 == '=')
        {
            switch(c1)
            {
                case '=': return SpecificTokenType.Equality;
                case '!': return SpecificTokenType.Inequality;

                case '>': return SpecificTokenType.GreaterOrEqual;
                case '<': return SpecificTokenType.SmallerOrEqual;

                case '+': return SpecificTokenType.AdditionAssign;
                case '-': return SpecificTokenType.SubtractionAssign;
                case '*': return SpecificTokenType.MultiplyAssign;
                case '/': return SpecificTokenType.DivideAssign;
                case '%': return SpecificTokenType.ModuloAssign;

                case '&': return SpecificTokenType.ANDAssign;
                case '|': return SpecificTokenType.ORAssign;
                case '^': return SpecificTokenType.XORAssign;
                case '~': return SpecificTokenType.TildeAssign;

                default: return SpecificTokenType.NotIndexed;
            }
        }
        if(c1 == c2)
        {
            switch(c1)
            {
                default: break;
                case '+': return SpecificTokenType.Increment;
                case '-': return SpecificTokenType.Decrement;

                case '&': return SpecificTokenType.AndAnd;
                case '|': return SpecificTokenType.OrOr;
                case '^': return SpecificTokenType.XorXor;

                case '<': return SpecificTokenType.ShiftLeft;
                case '>': return SpecificTokenType.ShiftRight;
            }
        }
        return SpecificTokenType.NotIndexed;
    }
    /// <summary>
    /// Gets a SpecificTokenType associated with the character.
    /// </summary>
    /// <returns>NotIndexed if this isnt a recognised operator.</returns>
    public static SpecificTokenType FromSingleCharOperator(char Operator)
    {
        switch(Operator)
        {
            case '=': return SpecificTokenType.Assign;
            
            case '+': return SpecificTokenType.Plus;
            case '-': return SpecificTokenType.Minus;
            case '*': return SpecificTokenType.Multiply;
            case '/': return SpecificTokenType.Divide;
            case '%': return SpecificTokenType.Modulo;

            case '>': return SpecificTokenType.GreaterThan;
            case '<': return SpecificTokenType.SmallerThan;

            case '&': return SpecificTokenType.AND;
            case '|': return SpecificTokenType.OR;
            case '^': return SpecificTokenType.XOR;
            case '~': return SpecificTokenType.Tilde;
            case '!': return SpecificTokenType.Negation;

            case '?': return SpecificTokenType.QuestionMark;
            case ':': return SpecificTokenType.Colon;
        }
        return SpecificTokenType.NotIndexed;
    }
}