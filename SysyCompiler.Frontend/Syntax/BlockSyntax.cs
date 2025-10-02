namespace SysyCompiler.Frontend.Syntax;

public class BlockSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxToken OpenBraceToken { get; }

    [SyntaxMember]
    public SyntaxList<StatementSyntax> Statements { get; }

    [SyntaxMember]
    public SyntaxToken CloseBraceToken { get; }

    public BlockSyntax(SyntaxToken openBraceToken, SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        CloseBraceToken = closeBraceToken;
    }

    public override SyntaxKind Kind => SyntaxKind.Block;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(OpenBraceToken), OpenBraceToken),
        new(nameof(Statements), Statements),
        new(nameof(CloseBraceToken), CloseBraceToken)
    };
}