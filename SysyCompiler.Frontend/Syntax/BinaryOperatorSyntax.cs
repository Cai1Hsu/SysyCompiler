namespace SysyCompiler.Frontend.Syntax;

public class BinaryOperatorSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxToken Token { get; }

    public BinaryOperatorSyntax(SyntaxToken operatorToken)
    {
        Token = operatorToken;
    }

    public override SyntaxKind Kind => SyntaxKind.BinaryOperator;

    public override SyntaxMember[] GetMembers()
        => new SyntaxMember[] { new(nameof(Token), Token) };
}