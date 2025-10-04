using System.Collections.Immutable;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer.Semantic;

public class VariableSymbol : INamedSymbol
{
    public ModifierSyntax? Modifier { get; }

    public SyntaxToken Identifier { get; }

    public SyntaxNode Reference { get; }

    public SymbolKind Kind => Modifier?.Token.TokenKind switch
    {
        TokenKind.Const => SymbolKind.Constant,
        _ => SymbolKind.Variable,
    };

    public ISymbolType Type { get; }

    public VariableSymbol(SyntaxToken identifier, SyntaxNode reference, ISymbolType type, ModifierSyntax? modifier)
    {
        Identifier = identifier;
        Reference = reference;
        Modifier = modifier;
        Type = type;
    }
}