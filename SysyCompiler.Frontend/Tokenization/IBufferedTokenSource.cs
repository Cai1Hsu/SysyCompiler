namespace SysyCompiler.Frontend.Tokenization;

/// <summary>
/// A token source that supports buffering tokens for lookahead.
/// </summary>
public interface IBufferedTokenSource : ITokenSource, IDisposable
{
    /// <summary>
    /// Peek at the token at the specified lookahead position without consuming it.
    /// </summary>
    /// <param name="lookahead">The lookahead position. 0 means the current token.</param>
    /// <returns>The token at the specified lookahead position, or null if the lookahead position is out of range.</returns>
    public IToken? PeekAt(int lookahead = 0);

    public IToken? Peek => PeekAt(0);

    public bool HasToken(int lookahead = 0);

    public bool IsMatch(int lookahead = 0, params ReadOnlySpan<TokenKind> kinds)
    {
        var token = PeekAt(lookahead);
        if (token == null) return false;

        foreach (var kind in kinds)
        {
            if (token.Kind == kind) return true;
        }

        return false;
    }

    public bool IsMatch(int lookahead = 0, params ReadOnlySpan<TokenKind[]> groups)
    {
        for (int i = 0; i < groups.Length; i++)
        {
            var pattern = groups[i].AsSpan();

            if (pattern.Length == 0 && groups.Length > 1) return true; // Empty group should not match anything

            if (!IsMatch(lookahead + i, pattern))
                return false;
        }

        return true;
    }
}