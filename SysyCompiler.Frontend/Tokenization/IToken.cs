namespace SysyCompiler.Frontend.Tokenization;

public interface IToken
{
    public TextSpan Span { get; set; }

    public TokenKind Kind { get; }

    public string Text { get; }

    public string? RawText { get; }
}