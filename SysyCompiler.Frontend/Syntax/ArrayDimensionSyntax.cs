namespace SysyCompiler.Frontend.Syntax;

public class ArrayDimensionSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxToken OpenBracketToken { get; }

    [SyntaxMember]
    public ExpressionSyntax? Expression { get; }

    [SyntaxMember]
    public SyntaxToken? CloseBracketToken { get; }

    public override SyntaxKind Kind => SyntaxKind.ArrayDimension;

    public ArrayDimensionSyntax(SyntaxToken leftBracketToken, SyntaxToken? rightBracketToken, ExpressionSyntax? expression = null)
    {
        OpenBracketToken = leftBracketToken;
        CloseBracketToken = rightBracketToken;
        Expression = expression;
    }

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(OpenBracketToken), OpenBracketToken),
        new(nameof(Expression), Expression),
        new(nameof(CloseBracketToken), CloseBracketToken)
    };
}
