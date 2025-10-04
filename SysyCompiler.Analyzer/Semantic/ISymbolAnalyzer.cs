using System.Diagnostics;
using SysyCompiler.Frontend;

namespace SysyCompiler.Analyzer.Semantic;

public interface ISymbolAnalyzer: IStackSyntaxVisitor
{
    public Stack<SymbolScope> Scopes { get; }

    public SymbolScope? CurrentScope => Scopes.Count > 0 ? Scopes.Peek() : null;

    ref struct SymbolScopeGuard : IDisposable
    {
        private readonly ISymbolAnalyzer analyzer;

        private readonly SymbolScope newScope;

        private SymbolScopeGuard(ISymbolAnalyzer analyzer, SymbolScope newScope)
        {
            this.analyzer = analyzer;
            this.newScope = newScope;
        }

        public static SymbolScopeGuard Create(ISymbolAnalyzer analyzer)
        {
            var newScope = new SymbolScope();
            analyzer.Scopes.Push(newScope);
            return new SymbolScopeGuard(analyzer, newScope);
        }

        public void Dispose()
        {
            var popped = analyzer.Scopes.Pop();

            Debug.Assert(popped == newScope);
        }
    }
}
