namespace SysyCompiler.Frontend.Syntax;

public class CompilationUnitSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxList<MemberDeclarationSyntax> Members { get; }

    [SyntaxMember]
    public SyntaxToken EndOfFileToken { get; }

    public CompilationUnitSyntax(SyntaxList<MemberDeclarationSyntax> members, SyntaxToken endOfFileToken)
    {
        Members = members;
        EndOfFileToken = endOfFileToken;
    }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Members), Members),
        new(nameof(EndOfFileToken), EndOfFileToken)
    };
}