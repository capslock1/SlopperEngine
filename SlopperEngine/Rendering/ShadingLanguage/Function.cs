using System.CodeDom.Compiler;
using System.Text;

namespace SlopperEngine.Rendering.ShadingLanguage;

public class Function
{
    public readonly string ReturnType;
    public readonly string Name;
    public readonly List<string> Arguments;
    public string Contents {get; private set;} = "";

    StringBuilder? _builder = new();

    public Function(string returnType, string name, List<string> arguments )
    {
        ReturnType = returnType;
        Name = name;
        Arguments = arguments;
    }

    public void Write(IndentedTextWriter output)
    {
        output.Write(ReturnType);
        output.Write(' ');
        output.Write(Name);
        output.Write('(');
        for(int i = 0; i<Arguments.Count; i++)
        {
            output.Write(Arguments[i]);
            output.Write(' ');
        }
        output.Write("){");
        output.Write(Contents);
        output.WriteLine('}');
    }

    public void AddString(string toAdd)
    {
        _builder?.Append(toAdd);
        _builder?.Append(' ');
    }

    public void Finish()
    {
        if(_builder == null) return;
        Contents = _builder.ToString();
        _builder = null;
    }
}