namespace SysyCompiler.Frontend.Syntax;

public class ControlFlowStatementSyntax : StatementSyntax
{
    public SyntaxToken ControlFlowToken { get; }

    public SyntaxToken SemicolonToken { get; }

    public ControlFlowStatementSyntax(SyntaxToken controlFlowToken, SyntaxToken semicolonToken)
    {
        ControlFlowToken = controlFlowToken;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ControlFlowStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(ControlFlowToken), ControlFlowToken),
        new(nameof(SemicolonToken), SemicolonToken)
    };
}