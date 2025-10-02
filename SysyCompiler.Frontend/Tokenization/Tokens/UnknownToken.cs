namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class UnknownToken : IToken
{
    public TokenKind Kind => TokenKind.Unknown;
    public TextSpan Span { get; set; }

    public string? RawText => Text;

    public string Text { get; }

    public UnknownToken(string text)
    {
        Text = text;
    }

    public override string ToString()
    {
        return $"{nameof(UnknownToken)}({RawText})";
    }
}
