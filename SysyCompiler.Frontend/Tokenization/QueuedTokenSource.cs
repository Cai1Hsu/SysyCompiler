using System.Collections;

namespace SysyCompiler.Frontend.Tokenization;

public class QueuedTokenSource : IBufferedTokenSource, IDisposable
{
    private readonly IEnumerator<IToken> tokenEnumerator;
    private readonly Queue<IToken> queue = new();
    private bool isEnumeratorExhausted = false;

    public QueuedTokenSource(ITokenSource tokenSource)
        : this((IEnumerable<IToken>)tokenSource)
    {
    }

    public QueuedTokenSource(IEnumerable<IToken> tokens)
    {
        tokenEnumerator = tokens.GetEnumerator();
    }

    private IToken? GetNextTokenFromEnumerator()
    {
        if (isEnumeratorExhausted)
            return null;

        if (tokenEnumerator.MoveNext())
        {
            return tokenEnumerator.Current;
        }
        else
        {
            isEnumeratorExhausted = true;
            return null;
        }
    }

    public IToken? NextToken()
    {
        if (queue.Count > 0)
        {
            return queue.Dequeue();
        }

        return GetNextTokenFromEnumerator();
    }

    public IToken? PeekAt(int lookahead = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(lookahead);

        // Try to fill the queue until it has enough tokens
        while (queue.Count <= lookahead && !isEnumeratorExhausted)
        {
            var token = GetNextTokenFromEnumerator();
            if (token == null) break;
            queue.Enqueue(token);
        }

        if (lookahead < queue.Count)
        {
            return queue.ElementAt(lookahead);
        }

        return null;
    }

    public bool HasToken(int lookahead = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(lookahead);

        // Try to fill the queue until it has enough tokens
        while (queue.Count <= lookahead && !isEnumeratorExhausted)
        {
            var token = GetNextTokenFromEnumerator();
            if (token == null) break;
            queue.Enqueue(token);
        }

        return lookahead < queue.Count;
    }

    public IEnumerator<IToken> GetEnumerator()
        => new TokenSourceEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public void Dispose()
    {
        tokenEnumerator?.Dispose();
    }
}