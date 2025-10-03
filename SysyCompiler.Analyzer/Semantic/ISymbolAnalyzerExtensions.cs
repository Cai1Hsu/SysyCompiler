using static SysyCompiler.Analyzer.Semantic.ISymbolAnalyzer;

namespace SysyCompiler.Analyzer.Semantic;

public static class ISymbolAnalyzerExtensions
{
    public static SymbolScopeGuard CreateScope(this ISymbolAnalyzer analyzer)
        => SymbolScopeGuard.Create(analyzer);

    public static bool IsGlobalScope(this ISymbolAnalyzer analyzer)
        => analyzer.Scopes.Count is 1;

    public static INamedSymbol? ResolveLocal(this ISymbolAnalyzer analyzer, string name)
        => analyzer.CurrentScope?[name];

    public static INamedSymbol? ResolveGlobal(this ISymbolAnalyzer analyzer, string name)
    {
        foreach (var scope in analyzer.Scopes)
        {
            if (scope[name] is INamedSymbol symbol)
            {
                return symbol;
            }
        }

        return null;
    }
}
