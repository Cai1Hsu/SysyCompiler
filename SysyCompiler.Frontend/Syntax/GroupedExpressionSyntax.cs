namespace SysyCompiler.Frontend.Syntax;

public class GroupedExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public SyntaxToken OpenParenToken { get; }

    [SyntaxMember]
    public ExpressionSyntax Expression { get; }

    [SyntaxMember]
    public SyntaxToken CloseParenToken { get; }

    public GroupedExpressionSyntax(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
    {
        OpenParenToken = openParenToken;
        Expression = expression;
        CloseParenToken = closeParenToken;
    }

    public override SyntaxKind Kind => SyntaxKind.GroupedExpression;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(OpenParenToken), OpenParenToken),
        new(nameof(Expression), Expression),
        new(nameof(CloseParenToken), CloseParenToken)
    };
}