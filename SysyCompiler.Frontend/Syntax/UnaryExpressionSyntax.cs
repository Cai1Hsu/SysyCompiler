namespace SysyCompiler.Frontend.Syntax;

public class UnaryExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public UnaryOperatorSyntax Operator { get; }

    [SyntaxMember]
    public ExpressionSyntax Operand { get; }

    public UnaryExpressionSyntax(UnaryOperatorSyntax operatorToken, ExpressionSyntax operand)
    {
        Operator = operatorToken;
        Operand = operand;
    }

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Operator), Operator),
        new(nameof(Operand), Operand)
    };
}