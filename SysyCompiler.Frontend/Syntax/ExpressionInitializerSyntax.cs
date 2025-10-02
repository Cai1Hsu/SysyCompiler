namespace SysyCompiler.Frontend.Syntax;

public class ExpressionInitializerSyntax : InitializerSyntax
{

    [SyntaxMember]
    public ExpressionSyntax Expression { get; }

    public ExpressionInitializerSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    public override SyntaxKind Kind => SyntaxKind.ExpressionInitializer;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Expression), Expression)
    };
}