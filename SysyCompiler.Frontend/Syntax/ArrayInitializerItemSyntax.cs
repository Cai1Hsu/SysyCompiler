namespace SysyCompiler.Frontend.Syntax;

public class ArrayInitializerItemSyntax : SyntaxNode
{
    [SyntaxMember]
    public InitializerSyntax Initializer { get; }

    [SyntaxMember]
    public SyntaxToken? CommaToken { get; }

    public ArrayInitializerItemSyntax(InitializerSyntax initializer, SyntaxToken? commaToken = null)
    {
        Initializer = initializer;
        CommaToken = commaToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ArrayInitializerItem;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Initializer), Initializer),
        new(nameof(CommaToken), CommaToken)
    };
}