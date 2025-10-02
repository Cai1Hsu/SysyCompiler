namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class KeywordToken : IToken
{
    public TokenKind Kind { get; }
    public TextSpan Span { get; set; }

    public string? RawText => null;

    public string Text => Kind.GetText();

    public KeywordToken(TokenKind kind)
    {
        Kind = kind;
    }

    public override string ToString()
    {
        return $"{Kind}Keyword";
    }
}