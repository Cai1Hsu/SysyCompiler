using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer;

public class OnlineJudgeOutput
{
    private readonly List<object> lines = new();

    public IReadOnlyList<object> Lines => lines;

    public IEnumerable<string> StringLines()
        => lines.Select(line => line switch
        {
            VirtualToken vt => vt.ToString(),
            VirtualNode vn => $"<{vn}>",
            _ => throw new InvalidOperationException()
        });

    public void AddToken(SyntaxToken token)
    {
        if (VirtualToken.Create(token) is VirtualToken vtoken)
        {
            lines.Add(vtoken);
        }
    }

    public void AddNode(VirtualNode vnode)
    {
        lines.Add(vnode);
    }
}
