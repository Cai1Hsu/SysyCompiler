using System.Collections;

namespace SysyCompiler.Frontend.Tokenization;

public class TokenSourceEnumerator : IEnumerator<IToken>
{
    private readonly ITokenSource tokenSource;

    public IToken Current { get; private set; } = null!;

    object IEnumerator.Current => Current;

    public TokenSourceEnumerator(ITokenSource tokenSource)
    {
        this.tokenSource = tokenSource;
    }

    public bool MoveNext()
    {
        var token = tokenSource.NextToken();

        if (token is null)
            return false;

        if (Current is not null // first token
            && Current.Kind == token.Kind && token.Kind == TokenKind.EndOfFile)
            return false;

        Current = token;
        return true;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
    }
}