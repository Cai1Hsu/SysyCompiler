namespace SysyCompiler.Frontend.Syntax;

/// <summary>
/// A syntax node that represents a reference to a variable or array element.
/// </summary>
public sealed class ReferenceExpressionSyntax : ExpressionSyntax
{
    [SyntaxMember]
    public SyntaxToken Identifier { get; }

    public ReferenceExpressionSyntax(SyntaxToken identifier)
    {
        Identifier = identifier;
    }

    public override SyntaxKind Kind => SyntaxKind.ReferenceExpression;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Identifier), Identifier)
    };
}