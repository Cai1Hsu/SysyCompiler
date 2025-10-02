namespace SysyCompiler.Frontend.Syntax;

public class IfStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public SyntaxToken IfKeyword { get; }

    [SyntaxMember]
    public SyntaxToken OpenParenToken { get; }

    [SyntaxMember]
    public ExpressionSyntax Condition { get; }

    [SyntaxMember]
    public SyntaxToken CloseParenToken { get; }

    [SyntaxMember]
    public StatementSyntax ThenStatement { get; }

    [SyntaxMember]
    public SyntaxToken? ElseKeyword { get; }

    [SyntaxMember]
    public StatementSyntax? ElseStatement { get; }

    public IfStatementSyntax(
        SyntaxToken ifKeyword,
        SyntaxToken openParenToken,
        ExpressionSyntax condition,
        SyntaxToken closeParenToken,
        StatementSyntax thenStatement,
        SyntaxToken? elseKeyword,
        StatementSyntax? elseStatement)
    {
        IfKeyword = ifKeyword;
        OpenParenToken = openParenToken;
        Condition = condition;
        CloseParenToken = closeParenToken;
        ThenStatement = thenStatement;
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public override SyntaxKind Kind => SyntaxKind.IfStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(IfKeyword), IfKeyword),
        new(nameof(OpenParenToken), OpenParenToken),
        new(nameof(Condition), Condition),
        new(nameof(CloseParenToken), CloseParenToken),
        new(nameof(ThenStatement), ThenStatement),
        new(nameof(ElseKeyword), ElseKeyword),
        new(nameof(ElseStatement), ElseStatement)
    };
}
