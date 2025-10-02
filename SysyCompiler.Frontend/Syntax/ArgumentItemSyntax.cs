namespace SysyCompiler.Frontend.Syntax;

public class ArgumentItemSyntax : SyntaxNode
{
    [SyntaxMember]
    public ExpressionSyntax Expression { get; }

    [SyntaxMember]
    public SyntaxToken? CommaToken { get; init; }

    public ArgumentItemSyntax(ExpressionSyntax expression, SyntaxToken? commaToken = null)
    {
        Expression = expression;
        CommaToken = commaToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ArgumentItem;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Expression), Expression),
        new(nameof(CommaToken), CommaToken)
    };
}