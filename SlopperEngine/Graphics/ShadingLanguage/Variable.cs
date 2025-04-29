namespace SlopperEngine.Graphics.ShadingLanguage;

public class Variable
{
    public readonly string Modifier;
    public readonly string Type;
    public readonly string Name;
    public Variable(string modifier, string type, string name)
    {
        Modifier = modifier;
        Type = type;
        Name = name;
    }
}