namespace SysyCompiler.Frontend.Syntax;

public class ArgumentListSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxList<ArgumentItemSyntax> Items { get; }

    public ArgumentListSyntax(SyntaxList<ArgumentItemSyntax> items)
    {
        Items = items;
    }

    public override SyntaxKind Kind => SyntaxKind.ArgumentList;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Items), Items)
    };
}
