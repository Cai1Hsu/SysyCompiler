
namespace SysyCompiler.Frontend.Syntax;

public class ArrayInitializerSyntax : InitializerSyntax
{
    [SyntaxMember]
    public SyntaxToken OpenBraceToken { get; }

    [SyntaxMember]
    public SyntaxList<ArrayInitializerItemSyntax> Items { get; }

    [SyntaxMember]
    public SyntaxToken CloseBraceToken { get; }

    public override SyntaxKind Kind => SyntaxKind.ArrayInitializer;

    public ArrayInitializerSyntax(SyntaxToken openBraceToken, SyntaxList<ArrayInitializerItemSyntax> items, SyntaxToken closeBraceToken)
    {
        OpenBraceToken = openBraceToken;
        Items = items;
        CloseBraceToken = closeBraceToken;
    }

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(OpenBraceToken), OpenBraceToken),
        new(nameof(Items), Items),
        new(nameof(CloseBraceToken), CloseBraceToken)
    };
}