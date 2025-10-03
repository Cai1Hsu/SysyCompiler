using System.Diagnostics;
using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Frontend;

public abstract class StackSyntaxVisitor<T> : SyntaxVisitor<T>, IStackSyntaxVisitor
{
    public Stack<SyntaxNode> AnalysisStack { get; } = new();

    public override T? Visit(SyntaxNode node, T? val)
    {
        this.Push(node);
        var result = base.Visit(node, val);
        var popped = this.Pop();
        Debug.Assert(ReferenceEquals(popped, node));
        return result;
    }
}