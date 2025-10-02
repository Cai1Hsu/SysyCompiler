namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class StringLiteralToken : IToken
{

    public TokenKind Kind => TokenKind.StringLiteral;
    public TextSpan Span { get; set; }

    public string? RawText => Text;

    public string Text { get; }

    public StringLiteralToken(string text)
    {
        Text = text;
    }

    public override string ToString()
    {
        return $"{nameof(StringLiteralToken)}({RawText})";
    }
}