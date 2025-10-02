namespace SysyCompiler.Frontend.Syntax;

public class ArrayDimensionSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxToken LeftBracketToken { get; }

    [SyntaxMember]
    public ExpressionSyntax? Expression { get; }

    [SyntaxMember]
    public SyntaxToken RightBracketToken { get; }

    public override SyntaxKind Kind => SyntaxKind.ArrayDimension;

    public ArrayDimensionSyntax(SyntaxToken leftBracketToken, SyntaxToken rightBracketToken, ExpressionSyntax? expression = null)
    {
        LeftBracketToken = leftBracketToken;
        RightBracketToken = rightBracketToken;
        Expression = expression;
    }

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(LeftBracketToken), LeftBracketToken),
        new(nameof(Expression), Expression),
        new(nameof(RightBracketToken), RightBracketToken)
    };
}
