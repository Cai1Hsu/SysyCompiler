namespace SysyCompiler.Frontend.Syntax;

public class ParameterListSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxList<ParameterItemSyntax> Items { get; }

    public override SyntaxKind Kind => SyntaxKind.ParameterList;
    public ParameterListSyntax(SyntaxList<ParameterItemSyntax> items)
    {
        Items = items;
    }

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Items), Items)
    };
}