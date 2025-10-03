using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer.Semantic;

public interface INamedSymbol
{
    public SyntaxToken Identifier { get; }

    public SyntaxNode Reference { get; }

    public SymbolKind Kind { get; }
}
