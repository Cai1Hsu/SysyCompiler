namespace SysyCompiler.Analyzer.Semantic;

public class SymbolScope
{
    public IReadOnlyDictionary<string, INamedSymbol> Symbols => symbols;

    private readonly Dictionary<string, INamedSymbol> symbols = new();

    public INamedSymbol? this[string name]
        => Symbols.TryGetValue(name, out var symbol) ? symbol : null;

    public bool TryDeclare(INamedSymbol symbol)
    {
        if (symbols.ContainsKey(symbol.Identifier.Text))
        {
            return false;
        }

        symbols[symbol.Identifier.Text] = symbol;
        return true;
    }
}
