namespace SysyCompiler.Frontend.Syntax;

public static class SyntaxNodeExtensions
{
    public static IEnumerable<SyntaxNode> GetChildrenSubtree(this SyntaxNode node)
    {
        foreach (var member in node.GetMembers())
        {
            if (member.Value is SyntaxNode childNode)
            {
                foreach (var grandChild in childNode.GetChildrenSubtree())
                {
                    yield return grandChild;
                }

                yield return childNode;
            }
            else if (member.Value is IEnumerable<SyntaxNode> childNodes)
            {
                foreach (var child in childNodes)
                {
                    foreach (var grandChild in child.GetChildrenSubtree())
                    {
                        yield return grandChild;
                    }

                    yield return child;
                }
            }
        }
    }
}