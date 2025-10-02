namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class IdentifierToken : IToken
{
    public TokenKind Kind => TokenKind.Identifier;
    public TextSpan Span { get; set; }

    public string? RawText => Text;

    public string Text { get; }

    public IdentifierToken(string name)
    {
        Text = name;
    }

    public override string ToString()
    {
        return $"Identifier({RawText})";
    }
}