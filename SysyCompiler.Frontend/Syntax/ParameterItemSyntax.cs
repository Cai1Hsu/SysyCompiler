namespace SysyCompiler.Frontend.Syntax;

public class ParameterItemSyntax : SyntaxNode
{
    [SyntaxMember]
    public TypeSyntax Type { get; }

    [SyntaxMember]
    public SyntaxToken Identifier { get; }

    [SyntaxMember]
    public SyntaxList<ArrayDimensionSyntax>? ArrayDimensions { get; }

    [SyntaxMember]
    public SyntaxToken? CommaToken { get; }

    public ParameterItemSyntax(TypeSyntax type, SyntaxToken identifier, SyntaxList<ArrayDimensionSyntax>? arrayDimensions = null, SyntaxToken? comma = null)
    {
        Type = type;
        Identifier = identifier;
        ArrayDimensions = arrayDimensions;
        CommaToken = comma;
    }

    public override SyntaxKind Kind => SyntaxKind.Parameter;

    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(Type), Type),
        new(nameof(Identifier), Identifier),
        new(nameof(ArrayDimensions), ArrayDimensions),
        new(nameof(CommaToken), CommaToken)
    };
}