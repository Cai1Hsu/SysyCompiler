using System.Diagnostics;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer.Tests;

public partial class OnlineJudgeAdapterTests
{
    private T ParseNode<T>(string source, Func<RecursiveDescentParser, T> parse)
    {
        var tokenizer = new Lexer(source);
        var parser = new RecursiveDescentParser(tokenizer);

        return parse(parser);
    }

    public OnlineJudgeOutput? ParseOutput(string source, Func<RecursiveDescentParser, SyntaxNode> parse)
        => ParseOutput(ParseNode(source, parse));

    public OnlineJudgeOutput? ParseOutput(SyntaxNode root)
    {
        var adapter = new OnlineJudgeAdapter();
        var output = new OnlineJudgeOutput();
        return root.Accept(adapter, output);
    }

    [DebuggerHidden]
    private void AssertNodes(OnlineJudgeOutput? output, params VirtualNode[] expected)
    {
        var actual = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();
        Assert.That(actual, Is.EqualTo(expected), $"Expected: {string.Join(", ", expected)}\nActual: {string.Join(", ", actual)}");
    }
}