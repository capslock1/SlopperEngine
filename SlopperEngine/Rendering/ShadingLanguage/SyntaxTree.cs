namespace SlopperEngine.Rendering.ShadingLanguage;

/// <summary>
/// Describes the code in a .sesl file in an abstract and more usable way.
/// </summary>
public class SyntaxTree
{
    public List<Variable> vertIn = new();
    public List<Variable> vertOut = new();
    public List<Variable> vertToPix = new();
    public List<Variable> pixOut = new();
    public List<Variable> uniform = new();
    public List<Function> otherFunctions = new();
    public Function? vertex;
    public Function? pixel;
    public readonly TokenScanner Scanner;
    public string CurrentLineAndChar => $"({_currentLine}, {_currentChar})";

    private int _currentLine = -1;
    private int _currentChar = -1;

    public SyntaxTree(TokenScanner scanner)
    {
        Scanner = scanner;
        Parse();
    }

    void Parse()
    {
        int scopeDepth = 0;
        Function? CurrentFunction = null;
        List<Token> currentSentence = new();
        foreach(Token token in Scanner.Tokens)
        {
            if(token.SpecificType == SpecificTokenType.BracketOpen)
                scopeDepth++;
            if(token.SpecificType == SpecificTokenType.BracketClose)
            {
                scopeDepth--;
                if(scopeDepth == 0)
                {
                    CurrentFunction?.Finish();
                    CurrentFunction = null;
                    currentSentence.Clear();
                    continue;
                }
            }

            if(scopeDepth > 0)
            {
                if(CurrentFunction == null)
                {
                    string ret = Scanner.GetTokenName(currentSentence[0]);
                    string name = Scanner.GetTokenName(currentSentence[1]);
                    List<string> args = new();
                    Token tok = currentSentence[3];
                    int tokIndex = 3;
                    while(tok.SpecificType != SpecificTokenType.ParenthesisClose)
                    {
                        args.Add(Scanner.GetTokenName(tok));
                        tokIndex++;
                        tok = currentSentence[tokIndex];
                    }
                    CurrentFunction = new(ret, name, args);
                    currentSentence.Clear();
                    if(name == "vertex")
                    {
                        vertex = CurrentFunction;
                        continue;
                    }
                    if(name == "pixel")
                    {
                        pixel = CurrentFunction;
                        continue;
                    }
                    otherFunctions.Add(CurrentFunction);
                    continue;
                }

                CurrentFunction.AddString(Scanner.GetTokenName(token));
                continue;
            }

            currentSentence.Add(token);

            _currentChar = token.CharNumber;
            _currentLine = token.LineNumber;

            if(token.SpecificType == SpecificTokenType.Semicolon)
            {
                //i dont even care anymore
                string modifier = Scanner.GetTokenName(currentSentence[0]);
                string type = Scanner.GetTokenName(currentSentence[1]);
                string name = Scanner.GetTokenName(currentSentence[2]);
                Variable res = new(modifier, type, name);
                switch(modifier)
                {
                    case "vertOut": vertOut.Add(res);
                    break;
                    case "vertIn": vertIn.Add(res);
                    break;
                    case "vertToPix": vertToPix.Add(res);
                    break;
                    case "pixOut": pixOut.Add(res);
                    break;
                    case "uniform": uniform.Add(res);
                    break;
                }
                currentSentence.Clear();
            }
        }
    }
}