using System.Diagnostics;
using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Frontend;

public abstract class StackSyntaxVisitor<T> : SyntaxVisitor<T>
{
    public Stack<SyntaxNode> AnalysisStack { get; } = new();

    public void Push(SyntaxNode node) => AnalysisStack.Push(node);

    public SyntaxNode Pop() => AnalysisStack.Pop();

    public SyntaxNode? Peek(int distance = 0) => AnalysisStack.Count > distance ? AnalysisStack.ElementAt(distance) : null;

    public TNode? GetClosest<TNode>(int searchLimit = int.MaxValue)
        where TNode : SyntaxNode
        => AnalysisStack.Take(searchLimit).OfType<TNode>().FirstOrDefault();

    public override T? Visit(SyntaxNode node, T? val)
    {
        Push(node);
        var result = base.Visit(node, val);
        var popped = Pop();
        Debug.Assert(ReferenceEquals(popped, node));
        return result;
    }
}