namespace SysyCompiler.Frontend.Syntax;

public class WhileStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public SyntaxToken WhileKeyword { get; }

    [SyntaxMember]
    public SyntaxToken OpenParenToken { get; }

    [SyntaxMember]
    public ExpressionSyntax Condition { get; }

    [SyntaxMember]
    public SyntaxToken? CloseParenToken { get; }

    [SyntaxMember]
    public StatementSyntax Body { get; }

    public WhileStatementSyntax(
        SyntaxToken whileKeyword,
        SyntaxToken openParenToken,
        ExpressionSyntax condition,
        SyntaxToken? closeParenToken,
        StatementSyntax body)
    {
        WhileKeyword = whileKeyword;
        OpenParenToken = openParenToken;
        Condition = condition;
        CloseParenToken = closeParenToken;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(WhileKeyword), WhileKeyword),
        new(nameof(OpenParenToken), OpenParenToken),
        new(nameof(Condition), Condition),
        new(nameof(CloseParenToken), CloseParenToken),
        new(nameof(Body), Body)
    };
}
