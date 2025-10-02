namespace SysyCompiler.Frontend.Tokenization.Tokens;

public class EndOfFileToken : IToken
{
    public TokenKind Kind => TokenKind.EndOfFile;
    public TextSpan Span { get; set; }
    public string? RawText => null;

    public string Text => string.Empty;

    public override string ToString()
    {
        return $"{nameof(EndOfFileToken)}";
    }
}