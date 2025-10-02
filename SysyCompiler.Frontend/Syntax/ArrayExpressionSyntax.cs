
namespace SysyCompiler.Frontend.Syntax;

public class ArrayExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public ExpressionSyntax Base { get; }

    [SyntaxMember]
    public ArrayDimensionSyntax Index { get; }

    public ArrayExpressionSyntax(ExpressionSyntax @base, ArrayDimensionSyntax index)
    {
        Base = @base;
        Index = index;
    }

    public override SyntaxKind Kind => SyntaxKind.ArrayExpression;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Base), Base),
        new(nameof(Index), Index)
    };
}