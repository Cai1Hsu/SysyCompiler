namespace SysyCompiler.Frontend.Syntax;

public class LiteralExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public SyntaxToken Token { get; }

    public LiteralExpressionSyntax(SyntaxToken token)
    {
        Token = token;
    }

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

    public override SyntaxMember[] GetMembers()
        => new SyntaxMember[] { new(nameof(Token), Token) };
}