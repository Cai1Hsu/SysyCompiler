using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Frontend;

public interface IStackSyntaxVisitor
{
    public Stack<SyntaxNode> AnalysisStack { get; }
}
