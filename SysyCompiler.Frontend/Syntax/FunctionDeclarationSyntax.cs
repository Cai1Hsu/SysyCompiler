namespace SysyCompiler.Frontend.Syntax;

public sealed class FunctionDeclarationSyntax : MemberDeclarationSyntax
{
    [SyntaxMember]
    public TypeSyntax ReturnType { get; }

    [SyntaxMember]
    public SyntaxToken Identifier { get; }

    [SyntaxMember]
    public SyntaxToken OpenParenToken { get; }

    [SyntaxMember]
    public ParameterListSyntax? ParameterList { get; }

    [SyntaxMember]
    public SyntaxToken? CloseParenToken { get; }

    [SyntaxMember]
    public BlockSyntax Body { get; set; }

    public FunctionDeclarationSyntax(
        TypeSyntax returnType,
        SyntaxToken identifier,
        SyntaxToken openParenToken,
        ParameterListSyntax? parameterList,
        SyntaxToken? closeParenToken,
        BlockSyntax body)
    {
        ReturnType = returnType;
        Identifier = identifier;
        OpenParenToken = openParenToken;
        ParameterList = parameterList;
        CloseParenToken = closeParenToken;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(ReturnType), ReturnType),
        new(nameof(Identifier), Identifier),
        new(nameof(OpenParenToken), OpenParenToken),
        new(nameof(ParameterList), ParameterList),
        new(nameof(CloseParenToken), CloseParenToken),
        new(nameof(Body), Body)
    };
}