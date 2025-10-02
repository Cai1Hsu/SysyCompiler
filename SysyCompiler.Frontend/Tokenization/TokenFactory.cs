namespace SysyCompiler.Frontend.Tokenization;

public class TokenFactory
{
    public int TokenStart { get; set; } = 0;
    public int CurrentPosition { get; set; } = 0;

    private readonly string sourceCode;

    public TokenFactory(string sourceCode)
    {
        this.sourceCode = sourceCode;
    }

    public bool IsAtEnd => CurrentPosition >= sourceCode.Length;

    public void BeginToken()
        => TokenStart = CurrentPosition;

    public ReadOnlySpan<char> TokenString
        => IsAtEnd
            ? sourceCode.AsSpan(TokenStart)
            : sourceCode.AsSpan(TokenStart, CurrentPosition - TokenStart);

    internal T InitializeToken<T>(T token) where T : IToken
    {
        token.Span = new TextSpan(TokenStart, CurrentPosition);
        return token;
    }
}