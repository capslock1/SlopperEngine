using System.Collections.ObjectModel;

namespace SlopperEngine.Graphics.ShadingLanguage;

/// <summary>
/// Scans all words/tokens in the source string for a sloppershader.
/// </summary>
public class TokenScanner
{
    public readonly string Source;
    public ReadOnlyCollection<Token> Tokens => _tokens.AsReadOnly();

    List<Token> _tokens;

    public TokenScanner(string source)
    {
        Source = source;
        _tokens = Scan(source);
    }

    /// <summary>
    /// Gets the name of a token.
    /// </summary>
    /// <param name="token">A token created by this scanner.</param>
    /// <returns>The name of the token.</returns>
    public string GetTokenName(Token token) => Source.Substring(token.PositionInSource, token.Length);

    List<Token> Scan(string source)
    {
        List<Token> tokens = new(64); //expecting at least 64 for the average shader

        Token currentWord = new();

        bool comment = false;
        bool multiLineComment = false;

        int currentLineNumber = 1; //cause files are 1 start indexed
        int lineStartIndex = 0;

        int currentIndex = 0;
        for(; currentIndex<source.Length; currentIndex++)
        {
            char ch = source[currentIndex];
            char nextch = currentIndex+1 >= source.Length ? ' ' : source[currentIndex+1];

            if(ch == '\n')
            {
                currentLineNumber++;
                lineStartIndex = currentIndex;
                comment = false;
            }
            //handle commenting
            if(comment || multiLineComment)
            {
                if(ch == '*' && nextch == '/')
                {
                    multiLineComment = false;
                    currentIndex++;
                }
                continue;
            }
            bool startingComment = false;
            if(ch == '/')
            {
                char next = nextch;
                if(next == '*')
                    multiLineComment = true;
                if(next == '/' && !multiLineComment)
                    comment = true;

                startingComment = multiLineComment || comment;
            }
            if(startingComment)
            {
                EndWord();
                //increment currentIndex because the next one is already checked
                currentIndex++;
                continue;
            }

            if(ch.FitsInWord(currentWord.Type))
            {
                //the ch is part of the word, so contain it and continue
                currentWord.Length++;
                continue;
            }

            //the ch is not part of the current word - end it, and start a new one
            EndWord();
            TokenType newType = ch.StartWord();
            if(ch == '.')
            {
                if(nextch.StartWord() == TokenType.Literal)
                    newType = TokenType.Literal;
            }
            currentWord = new(newType, currentIndex, 1, currentLineNumber, currentIndex - lineStartIndex);
            if(newType == TokenType.None)
            {
                if(!char.IsWhiteSpace(ch))
                    Transpiler.AddError($"Scanner: unknown character '{ch}' at ({currentWord.LineNumber} : {currentWord.CharNumber})");
            }
        }
        EndWord();

        return tokens;

        void EndWord()
        {
            if(currentWord.Type == TokenType.None) return;
            if(currentWord.Type == TokenType.Operator)
            {
                ParseOperator(ref currentWord);
                return;
            }
            currentWord.SpecificType = TokenTypes.GetSpecificTokenType(ref currentWord, source);
            if(currentWord.Type == TokenType.Identifier)
            {
                if(currentWord.Length >= 3)
                {
                    var substr = source.Substring(currentWord.PositionInSource, 3);
                    if(substr == "gl_" || substr == "SL_")
                        Transpiler.AddError($"Scanner: use of reserved prefix \"{substr}\" at ({currentWord.LineNumber} : {currentWord.CharNumber})");
                }
                if(currentWord.SpecificType != SpecificTokenType.NotIndexed)
                    currentWord.Type = TokenType.Keyword;
            }
            tokens.Add(currentWord);
        }

        void ParseOperator(ref Token tok)
        {
            int tokenCount = tok.Length;
            int loc = tok.PositionInSource;
            if(tokenCount == 1)
            {
                tok.SpecificType = TokenTypes.FromSingleCharOperator(source[loc]);
                tokens.Add(tok);
                return;
            }
            if(tokenCount == 2)
            {
                var typ = TokenTypes.FromTwoCharOperator(source[loc], source[loc+1]);
                if(typ != SpecificTokenType.NotIndexed)
                {
                    tok.SpecificType = typ;
                    tokens.Add(tok);
                    return;
                }
                tok.Length = 1;
                tok.SpecificType = TokenTypes.FromSingleCharOperator(source[loc]);
                tokens.Add(tok);
                tokens.Add(new(TokenType.Operator, loc+1, 1, tok.LineNumber, tok.CharNumber+1)
                    {SpecificType = TokenTypes.FromSingleCharOperator(source[loc+1])});
                return;
            }
            var type = TokenTypes.FromThreeCharOperator(source[loc], source[loc+1], source[loc+2]);
            if(type != SpecificTokenType.NotIndexed)
            {
                tok.SpecificType = type;
                tokens.Add(tok);
                return;
            }
            var split = new Token(tok.Type, tok.PositionInSource+2, tok.Length-2, tok.LineNumber, tok.CharNumber+2);
            tok.Length = 2;
            ParseOperator(ref tok);
            ParseOperator(ref split);
            return;
        }
    }
}