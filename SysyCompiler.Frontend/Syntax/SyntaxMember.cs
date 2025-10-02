namespace SysyCompiler.Frontend.Syntax;

public readonly struct SyntaxMember
{
    public string Name { get; }
    public object? Value { get; }

    public SyntaxMember(string name, object? value)
    {
        Name = name;
        Value = value;
    }
}