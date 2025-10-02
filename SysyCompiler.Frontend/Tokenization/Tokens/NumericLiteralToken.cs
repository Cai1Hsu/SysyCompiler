namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class NumericLiteralToken : IToken
{
    public TokenKind Kind { get; }
    public TextSpan Span { get; set; }

    public string? RawText => Text;

    public string Text { get; }

    public NumericLiteralToken(string text, TokenKind kind)
    {
        Text = text;
        Kind = kind;
    }

    public override string ToString()
    {
        return $"{nameof(NumericLiteralToken)}({RawText}, {Kind})";
    }
}