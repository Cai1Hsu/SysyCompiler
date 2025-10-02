namespace SysyCompiler.Frontend.Syntax;

public class BinaryExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public ExpressionSyntax Left { get; }

    [SyntaxMember]
    public BinaryOperatorSyntax Operator { get; }

    [SyntaxMember]
    public ExpressionSyntax Right { get; }

    public BinaryExpressionSyntax(ExpressionSyntax left, BinaryOperatorSyntax operatorToken, ExpressionSyntax right)
    {
        Left = left;
        Operator = operatorToken;
        Right = right;
    }

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Left), Left),
        new(nameof(Operator), Operator),
        new(nameof(Right), Right)
    };
}