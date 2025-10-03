namespace SysyCompiler.Frontend.Syntax;

public class VariableDeclarationSyntax : MemberDeclarationSyntax
{
    [SyntaxMember]
    public ModifierSyntax? Modifier { get; }

    [SyntaxMember]
    public TypeSyntax Type { get; }

    [SyntaxMember]
    public SyntaxList<VariableDefineSyntax>? VariableDefines => define as SyntaxList<VariableDefineSyntax>;

    [SyntaxMember]
    public VariableDefineSyntax? VariableDefine => define as VariableDefineSyntax;

    private object define;

    [SyntaxMember]
    public SyntaxToken? SemicolonToken { get; }

    public VariableDeclarationSyntax(TypeSyntax type, VariableDefineSyntax variableDefine, SyntaxToken? semicolonToken, ModifierSyntax? modifier = null)
    {
        Modifier = modifier;
        Type = type;
        define = variableDefine;
        SemicolonToken = semicolonToken;
    }

    public VariableDeclarationSyntax(TypeSyntax type, SyntaxList<VariableDefineSyntax> variableDefines, SyntaxToken? semicolonToken, ModifierSyntax? modifier = null)
    {
        Modifier = modifier;
        Type = type;
        define = variableDefines;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Modifier), Modifier),
        new(nameof(Type), Type),
        new(nameof(VariableDefines), VariableDefines),
        new(nameof(VariableDefine), VariableDefine),
        new(nameof(SemicolonToken), SemicolonToken)
    };
}