namespace SysyCompiler.Frontend.Syntax;

public class FunctionCallExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public ExpressionSyntax Callee { get; }

    [SyntaxMember]
    public SyntaxToken OpenParenToken { get; }

    [SyntaxMember]
    public ArgumentListSyntax ArgumentList { get; }

    [SyntaxMember]
    public SyntaxToken CloseParenToken { get; }

    public FunctionCallExpressionSyntax(ExpressionSyntax callee, SyntaxToken openParenToken, ArgumentListSyntax argumentList, SyntaxToken closeParenToken)
    {
        Callee = callee;
        OpenParenToken = openParenToken;
        ArgumentList = argumentList;
        CloseParenToken = closeParenToken;
    }

    public override SyntaxKind Kind => SyntaxKind.FunctionCallExpression;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Callee), Callee),
        new(nameof(OpenParenToken), OpenParenToken),
        new(nameof(ArgumentList), ArgumentList),
        new(nameof(CloseParenToken), CloseParenToken)
    };
}
