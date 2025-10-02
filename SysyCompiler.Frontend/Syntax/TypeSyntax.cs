namespace SysyCompiler.Frontend.Syntax;

public class TypeSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.Type;

    [SyntaxMember]
    public SyntaxToken Token { get; }

    public TypeSyntax(SyntaxToken typeToken)
    {
        Token = typeToken;
    }

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Token), Token)
    };
}