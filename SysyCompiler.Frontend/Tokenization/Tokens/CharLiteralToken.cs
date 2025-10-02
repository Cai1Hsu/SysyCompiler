namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class CharLiteralToken : IToken
{
    public TokenKind Kind { get; }
    public TextSpan Span { get; set; }

    public string? RawText => Text;

    public string Text { get; }

    public CharLiteralToken(string text)
    {
        Text = text;
        Kind = TokenKind.CharLiteral;
    }

    public override string ToString()
    {
        return $"{nameof(CharLiteralToken)}({RawText})";
    }
}