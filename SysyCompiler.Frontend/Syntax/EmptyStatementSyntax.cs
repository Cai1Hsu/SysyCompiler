namespace SysyCompiler.Frontend.Syntax;

public class EmptyStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public SyntaxToken SemicolonToken { get; }

    public EmptyStatementSyntax(SyntaxToken semicolonToken)
    {
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.EmptyStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(SemicolonToken), SemicolonToken)
    };
}
