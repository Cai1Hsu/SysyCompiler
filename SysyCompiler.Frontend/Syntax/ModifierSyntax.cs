namespace SysyCompiler.Frontend.Syntax;

public class ModifierSyntax : SyntaxNode
{

    [SyntaxMember]
    public SyntaxToken Token { get; }

    public ModifierSyntax(SyntaxToken modifierToken)
    {
        Token = modifierToken;
    }

    public override SyntaxKind Kind => SyntaxKind.Modifier;

    public override SyntaxMember[] GetMembers()
        => new SyntaxMember[] { new(nameof(Token), Token) };
}