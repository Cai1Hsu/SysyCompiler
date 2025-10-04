using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer.Semantic;

public class FunctionSymbol : INamedSymbol
{
    public SyntaxToken Identifier => Declaration.Identifier;

    SyntaxNode INamedSymbol.Reference => Declaration;

    public FunctionDeclarationSyntax Declaration { get; }

    public SymbolKind Kind => SymbolKind.Function;

    public FunctionType Type { get; }

    ISymbolType INamedSymbol.Type => Type;

    public FunctionSymbol(FunctionDeclarationSyntax declaration, FunctionType type)
    {
        Declaration = declaration;
        Type = type;
    }
}
