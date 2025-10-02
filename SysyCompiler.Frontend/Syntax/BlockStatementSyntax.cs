namespace SysyCompiler.Frontend.Syntax;

public class BlockStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public BlockSyntax Block { get; }

    public BlockStatementSyntax(BlockSyntax block)
    {
        Block = block;
    }

    public override SyntaxKind Kind => SyntaxKind.BlockStatement;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Block), Block)
    };
}
