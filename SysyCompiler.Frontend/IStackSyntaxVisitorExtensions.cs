using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Frontend;

public static class IStackSyntaxVisitorExtensions
{
    public static void Push(this IStackSyntaxVisitor visitor, SyntaxNode node)
        => visitor.AnalysisStack.Push(node);

    public static SyntaxNode Pop(this IStackSyntaxVisitor visitor) => visitor.AnalysisStack.Pop();

    public static SyntaxNode? Peek(this IStackSyntaxVisitor visitor, int distance = 0) => visitor.AnalysisStack.Count > distance ? visitor.AnalysisStack.ElementAt(distance) : null;

    public static TNode? GetClosest<TNode>(this IStackSyntaxVisitor visitor, Func<TNode, bool>? predicate = null, int searchLimit = int.MaxValue)
            where TNode : SyntaxNode
    {
        var typeFiltered = visitor.AnalysisStack.Take(searchLimit).OfType<TNode>();

        if (predicate is null)
        {
            return typeFiltered.FirstOrDefault();
        }
        else
        {
            return typeFiltered.FirstOrDefault(predicate);
        }
    }
}