namespace SysyCompiler.Frontend.Syntax;

public class ExpressionStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public ExpressionSyntax Expression { get; }

    [SyntaxMember]
    public SyntaxToken SemicolonToken { get; }

    public ExpressionStatementSyntax(ExpressionSyntax expression, SyntaxToken semicolonToken)
    {
        Expression = expression;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Expression), Expression),
        new(nameof(SemicolonToken), SemicolonToken)
    };
}
