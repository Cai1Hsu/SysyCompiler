namespace SysyCompiler.Frontend.Syntax;

public class ReturnStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public SyntaxToken ReturnKeyword { get; }

    [SyntaxMember]
    public ExpressionSyntax? Expression { get; }

    [SyntaxMember]
    public SyntaxToken SemicolonToken { get; }

    public ReturnStatementSyntax(SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(ReturnKeyword), ReturnKeyword),
        new(nameof(Expression), Expression),
        new(nameof(SemicolonToken), SemicolonToken)
    };
}
