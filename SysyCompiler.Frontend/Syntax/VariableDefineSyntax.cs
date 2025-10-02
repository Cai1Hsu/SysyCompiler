namespace SysyCompiler.Frontend.Syntax;

public class VariableDefineSyntax : SyntaxNode
{
    [SyntaxMember]
    public SyntaxToken Identifier { get; }

    [SyntaxMember]
    public SyntaxList<ArrayDimensionSyntax>? ArrayDimensions { get; }

    [SyntaxMember]
    public SyntaxToken? AssignToken { get; }

    [SyntaxMember]
    public InitializerSyntax? Initializer { get; }

    [SyntaxMember]
    public SyntaxToken? CommaToken { get; }

    public VariableDefineSyntax(
        SyntaxToken identifier,
        InitializerSyntax? initializer,
        SyntaxToken? assignToken = null,
        SyntaxList<ArrayDimensionSyntax>? arrayDimensions = null,
        SyntaxToken? commaToken = null)
    {
        Identifier = identifier;
        Initializer = initializer;
        AssignToken = assignToken;
        ArrayDimensions = arrayDimensions;
        CommaToken = commaToken;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDefine;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Identifier), Identifier),
        new(nameof(ArrayDimensions), ArrayDimensions),
        new(nameof(AssignToken), AssignToken),
        new(nameof(Initializer), Initializer),
        new(nameof(CommaToken), CommaToken)
    };
}
