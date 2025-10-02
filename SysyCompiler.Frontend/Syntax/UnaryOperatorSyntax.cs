namespace SysyCompiler.Frontend.Syntax;

public class UnaryOperatorSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxToken Token { get; }

    public UnaryOperatorSyntax(SyntaxToken operatorToken)
    {
        Token = operatorToken;
    }

    public override SyntaxKind Kind => SyntaxKind.UnaryOperator;

    public override SyntaxMember[] GetMembers()
        => new SyntaxMember[] { new(nameof(Token), Token) };
}
