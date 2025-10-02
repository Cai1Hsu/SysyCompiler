namespace SysyCompiler.Frontend.Syntax;

public class LocalDeclarationStatementSyntax : StatementSyntax
{
    [SyntaxMember]
    public VariableDeclarationSyntax Declaration { get; }

    public LocalDeclarationStatementSyntax(VariableDeclarationSyntax declaration)
    {
        Declaration = declaration;
    }

    public override SyntaxKind Kind => SyntaxKind.LocalDeclarationStatement;

    public override SyntaxMember[] GetMembers()
        => new SyntaxMember[] { new(nameof(Declaration), Declaration) };
}