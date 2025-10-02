namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class OperatorToken : IToken
{
    public TokenKind Kind { get; }
    public TextSpan Span { get; set; }

    public string? RawText => null;

    public string Text => Kind.GetText();

    public OperatorToken(TokenKind kind)
    {
        Kind = kind;
    }

    public override string ToString()
    {
        return $"{Kind}Token";
    }
}
