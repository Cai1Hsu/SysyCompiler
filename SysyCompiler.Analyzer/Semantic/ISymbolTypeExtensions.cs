using System.Collections.Immutable;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;
using static SysyCompiler.Analyzer.OnlineJudgeCodeAnalyzer;

namespace SysyCompiler.Analyzer.Semantic;

public static class ISymbolTypeExtensions
{
    public static bool IsArray(this ISymbolType type)
        => type.ArrayRank > 0;

    public static bool IsScalar(this ISymbolType type)
        => type.ArrayRank == 0;

    public static ISymbolType? Parse(LiteralExpressionSyntax literal) => literal.Token.TokenKind switch
    {
        TokenKind.CharLiteral => new PrimitiveType(PrimitiveTypeKind.Char),
        TokenKind.StringLiteral => new PrimitiveType(PrimitiveTypeKind.String),
        TokenKind.BinaryIntLiteral
        or TokenKind.OctalIntLiteral
        or TokenKind.DecimalIntLiteral
        or TokenKind.HexIntLiteral => new PrimitiveType(PrimitiveTypeKind.Int),
        _ => null,
    };

    public static FunctionType? Parse(FunctionDeclarationSyntax functionDeclaration, ImmutableArray<ISymbolType>? parameterTypes = null)
    {
        var returnType = Parse(functionDeclaration.ReturnType, 0);

        if (returnType is null)
            return null;

        return new FunctionType(returnType, parameterTypes);
    }

    public static ISymbolType? Parse(TypeSyntax typeSyntax, int arrayRank = 0)
    {
        ScalarType? baseType = typeSyntax.Token.TokenKind switch
        {
            TokenKind.Int => new PrimitiveType(PrimitiveTypeKind.Int),
            TokenKind.Char => new PrimitiveType(PrimitiveTypeKind.Char),
            TokenKind.Void => new PrimitiveType(PrimitiveTypeKind.Void),
            TokenKind.Identifier => new CustomType(typeSyntax.Token),
            _ => null,
        };

        if (baseType is null)
            return null;

        return arrayRank > 0 ? new ArrayType(baseType, arrayRank) : baseType;
    }
}
