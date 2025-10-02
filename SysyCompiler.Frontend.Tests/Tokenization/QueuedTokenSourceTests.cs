using SysyCompiler.Frontend.Tokenization;
using SysyCompiler.Frontend.Tokenization.Tokens;

namespace SysyCompiler.Frontend.Tests.Tokenization;

[TestFixture]
public class QueuedTokenSourceTests
{
    private static IToken CreateTestToken(TokenKind kind, string value = "")
    {
        return kind switch
        {
            TokenKind.Identifier => new IdentifierToken(value),
            TokenKind.DecimalIntLiteral => new NumericLiteralToken(value, TokenKind.DecimalIntLiteral),
            TokenKind.StringLiteral => new StringLiteralToken(value),
            TokenKind.EndOfFile => new EndOfFileToken(),
            _ => new KeywordToken(kind)
        };
    }

    [Test]
    public void NextToken_EmptySource_ReturnsNull()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act
        var result = queuedSource.NextToken();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void NextToken_SingleToken_ReturnsToken()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Identifier, "test");
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = queuedSource.NextToken();

        // Assert
        Assert.That(result, Is.EqualTo(token));
    }

    [Test]
    public void NextToken_MultipleTokens_ReturnsInOrder()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test1");
        var token2 = CreateTestToken(TokenKind.Int);
        var token3 = CreateTestToken(TokenKind.DecimalIntLiteral, "42");
        var queuedSource = new QueuedTokenSource(new[] { token1, token2, token3 });

        // Act & Assert
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token1));
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token2));
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token3));
        Assert.That(queuedSource.NextToken(), Is.Null);
    }

    #region PeekAt Tests

    [Test]
    public void PeekAt_EmptySource_ReturnsNull()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act
        var result = queuedSource.PeekAt(0);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void PeekAt_SingleToken_LookaheadZero_ReturnsToken()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Identifier, "test");
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = queuedSource.PeekAt(0);

        // Assert
        Assert.That(result, Is.EqualTo(token));
    }

    [Test]
    public void PeekAt_MultipleTokens_LookaheadVariousPositions()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test1");
        var token2 = CreateTestToken(TokenKind.Int);
        var token3 = CreateTestToken(TokenKind.DecimalIntLiteral, "42");
        var queuedSource = new QueuedTokenSource(new[] { token1, token2, token3 });

        // Act & Assert
        Assert.That(queuedSource.PeekAt(0), Is.EqualTo(token1));
        Assert.That(queuedSource.PeekAt(1), Is.EqualTo(token2));
        Assert.That(queuedSource.PeekAt(2), Is.EqualTo(token3));
        Assert.That(queuedSource.PeekAt(3), Is.Null);
    }

    [Test]
    public void PeekAt_DoesNotConsumeTokens()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test1");
        var token2 = CreateTestToken(TokenKind.Int);
        var queuedSource = new QueuedTokenSource(new[] { token1, token2 });

        // Act
        var peek1 = queuedSource.PeekAt(0);
        var peek2 = queuedSource.PeekAt(1);
        var peek3 = queuedSource.PeekAt(0); // Peek again at position 0

        // Assert
        Assert.That(peek1, Is.EqualTo(token1));
        Assert.That(peek2, Is.EqualTo(token2));
        Assert.That(peek3, Is.EqualTo(token1)); // Should still be the same token

        // Verify tokens are still available for consumption
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token1));
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token2));
    }

    [Test]
    public void PeekAt_NegativeLookahead_ThrowsException()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queuedSource.PeekAt(-1));
    }

    #endregion

    #region Mixed NextToken and PeekAt Tests

    [Test]
    public void MixedOperations_PeekThenConsume_WorksCorrectly()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test1");
        var token2 = CreateTestToken(TokenKind.Int);
        var token3 = CreateTestToken(TokenKind.DecimalIntLiteral, "42");
        var queuedSource = new QueuedTokenSource(new[] { token1, token2, token3 });

        // Act & Assert
        // Peek ahead
        Assert.That(queuedSource.PeekAt(0), Is.EqualTo(token1));
        Assert.That(queuedSource.PeekAt(1), Is.EqualTo(token2));
        Assert.That(queuedSource.PeekAt(2), Is.EqualTo(token3));

        // Consume tokens
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token1));

        // Peek after consuming one token
        Assert.That(queuedSource.PeekAt(0), Is.EqualTo(token2));
        Assert.That(queuedSource.PeekAt(1), Is.EqualTo(token3));

        // Consume remaining tokens
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token2));
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token3));
        Assert.That(queuedSource.NextToken(), Is.Null);
    }

    [Test]
    public void MixedOperations_ConsumeThenPeek_WorksCorrectly()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test1");
        var token2 = CreateTestToken(TokenKind.Int);
        var token3 = CreateTestToken(TokenKind.DecimalIntLiteral, "42");
        var queuedSource = new QueuedTokenSource(new[] { token1, token2, token3 });

        // Act & Assert
        // Consume first token
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token1));

        // Peek at remaining tokens
        Assert.That(queuedSource.PeekAt(0), Is.EqualTo(token2));
        Assert.That(queuedSource.PeekAt(1), Is.EqualTo(token3));

        // Consume remaining tokens
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token2));
        Assert.That(queuedSource.NextToken(), Is.EqualTo(token3));
    }

    #endregion

    #region IsEnded Tests

    [Test]
    public void IsEnded_EmptySource_ReturnsTrue()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act
        var result = queuedSource.HasToken(0);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsEnded_WithTokens_ReturnsFalse()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Identifier, "test");
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = queuedSource.HasToken(0);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsEnded_WithEOFToken_ReturnsTrue()
    {
        // Arrange
        var eofToken = CreateTestToken(TokenKind.EndOfFile);
        var queuedSource = new QueuedTokenSource(new[] { eofToken });

        // Act
        var result = queuedSource.HasToken(0);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsEnded_LookaheadBeyondTokens_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Identifier, "test");
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = queuedSource.HasToken(1); // Lookahead beyond available tokens

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsEnded_NegativeLookahead_ThrowsException()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queuedSource.HasToken(-1));
    }

    #endregion

    #region IsMatch Tests

    [Test]
    public void IsMatch_MatchingToken_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = ((IBufferedTokenSource)queuedSource).IsMatch(0, TokenKind.Int);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_NonMatchingToken_ReturnsFalse()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = ((IBufferedTokenSource)queuedSource).IsMatch(0, TokenKind.Identifier);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_MultipleKinds_MatchesAny_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = ((IBufferedTokenSource)queuedSource).IsMatch(0, TokenKind.Identifier, TokenKind.Int, TokenKind.Char);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_MultipleKinds_MatchesNone_ReturnsFalse()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var queuedSource = new QueuedTokenSource(new[] { token });

        // Act
        var result = ((IBufferedTokenSource)queuedSource).IsMatch(0, TokenKind.Identifier, TokenKind.Char, TokenKind.Void);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_NoToken_ReturnsFalse()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act
        var result = ((IBufferedTokenSource)queuedSource).IsMatch(0, TokenKind.Int);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_WithLookahead_WorksCorrectly()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test");
        var token2 = CreateTestToken(TokenKind.Int);
        var queuedSource = new QueuedTokenSource(new[] { token1, token2 });

        // Act & Assert
        var bufferedSource = (IBufferedTokenSource)queuedSource;
        Assert.That(bufferedSource.IsMatch(0, TokenKind.Identifier), Is.True);
        Assert.That(bufferedSource.IsMatch(1, TokenKind.Int), Is.True);
        Assert.That(bufferedSource.IsMatch(0, TokenKind.Int), Is.False);
        Assert.That(bufferedSource.IsMatch(1, TokenKind.Identifier), Is.False);
    }

    #endregion

    #region Enumeration Tests

    [Test]
    public void GetEnumerator_EmptySource_NoIterations()
    {
        // Arrange
        var queuedSource = new QueuedTokenSource(Array.Empty<IToken>());

        // Act
        var tokens = new List<IToken>();
        foreach (var token in queuedSource)
        {
            tokens.Add(token);
        }

        // Assert
        Assert.That(tokens, Is.Empty);
    }

    [Test]
    public void GetEnumerator_WithTokens_IteratesCorrectly()
    {
        // Arrange
        var token1 = CreateTestToken(TokenKind.Identifier, "test1");
        var token2 = CreateTestToken(TokenKind.Int);
        var token3 = CreateTestToken(TokenKind.DecimalIntLiteral, "42");
        var queuedSource = new QueuedTokenSource(new[] { token1, token2, token3 });

        // Act
        var tokens = new List<IToken>();
        foreach (var token in queuedSource)
        {
            tokens.Add(token);
        }

        // Assert
        Assert.That(tokens.Count, Is.EqualTo(3));
        Assert.That(tokens[0], Is.EqualTo(token1));
        Assert.That(tokens[1], Is.EqualTo(token2));
        Assert.That(tokens[2], Is.EqualTo(token3));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void RealWorldScenario_ParserLikeUsage()
    {
        // Arrange - Simulate "int x = 42;"
        var queuedSource = new QueuedTokenSource(new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x"),
            CreateTestToken(TokenKind.AssignEqual),
            CreateTestToken(TokenKind.DecimalIntLiteral, "42"),
            CreateTestToken(TokenKind.Semicolon)
        });

        // Act & Assert - Simulate parser behavior
        // Check if it's a type declaration
        var bufferedQueuedSource = (IBufferedTokenSource)queuedSource;
        Assert.That(bufferedQueuedSource.IsMatch(0, TokenKind.Int, TokenKind.Char, TokenKind.Void), Is.True);
        Assert.That(bufferedQueuedSource.IsMatch(1, TokenKind.Identifier), Is.True);

        // Consume type token
        var typeToken = queuedSource.NextToken();
        Assert.That(typeToken?.Kind, Is.EqualTo(TokenKind.Int));

        // Peek at assignment pattern
        Assert.That(bufferedQueuedSource.IsMatch(0, TokenKind.Identifier), Is.True);
        Assert.That(bufferedQueuedSource.IsMatch(1, TokenKind.AssignEqual), Is.True);

        // Consume identifier
        var identifierToken = queuedSource.NextToken();
        Assert.That(identifierToken?.Kind, Is.EqualTo(TokenKind.Identifier));

        // Continue parsing...
        Assert.That(queuedSource.NextToken()?.Kind, Is.EqualTo(TokenKind.AssignEqual));
        Assert.That(queuedSource.NextToken()?.Kind, Is.EqualTo(TokenKind.DecimalIntLiteral));
        Assert.That(queuedSource.NextToken()?.Kind, Is.EqualTo(TokenKind.Semicolon));

        // Should be at end
        Assert.That(queuedSource.NextToken(), Is.Null);
    }

    #endregion
}