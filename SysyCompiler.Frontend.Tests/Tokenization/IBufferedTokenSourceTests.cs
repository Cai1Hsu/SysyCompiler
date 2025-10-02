using SysyCompiler.Frontend.Tokenization;
using SysyCompiler.Frontend.Tokenization.Tokens;

namespace SysyCompiler.Frontend.Tests.Tokenization;

[TestFixture]
public class IBufferedTokenSourceTests
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

    private class MockTokenSource : ITokenSource
    {
        private readonly IToken[] _tokens;
        private int _currentIndex = 0;

        public MockTokenSource(params IToken[] tokens)
        {
            _tokens = tokens;
        }

        public IToken? NextToken()
        {
            if (_currentIndex >= _tokens.Length)
                return null;

            return _tokens[_currentIndex++];
        }

        public IEnumerator<IToken> GetEnumerator()
        {
            var index = 0;
            while (index < _tokens.Length)
            {
                yield return _tokens[index++];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    #region IsMatch Single Kind Implementation Tests

    [Test]
    public void IsMatch_EmptyKindsArray_ReturnsFalse()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act - Test with no kinds (empty params array)
        var result = bufferedSource.IsMatch(0, Array.Empty<TokenKind>());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_NullToken_ReturnsFalse()
    {
        // Arrange
        var mockSource = new MockTokenSource(); // empty source
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Int);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_SingleKind_Match_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Int);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_SingleKind_NoMatch_ReturnsFalse()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Char);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_MultipleKinds_FirstMatches_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Int, TokenKind.Char, TokenKind.Void);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_MultipleKinds_MiddleMatches_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Char, TokenKind.Int, TokenKind.Void);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_MultipleKinds_LastMatches_ReturnsTrue()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Char, TokenKind.Void, TokenKind.Int);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_MultipleKinds_NoneMatch_ReturnsFalse()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Int);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act
        var result = bufferedSource.IsMatch(0, TokenKind.Char, TokenKind.Void, TokenKind.Identifier);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsMatch_LargeKindsArray_WorksCorrectly()
    {
        // Arrange
        var token = CreateTestToken(TokenKind.Semicolon);
        var mockSource = new MockTokenSource(token);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act - Test with many token kinds
        var result = bufferedSource.IsMatch(0,
            TokenKind.Int, TokenKind.Char, TokenKind.Void, TokenKind.Identifier,
            TokenKind.Plus, TokenKind.Minus, TokenKind.Star, TokenKind.Slash,
            TokenKind.LeftParen, TokenKind.RightParen, TokenKind.LeftBrace, TokenKind.RightBrace,
            TokenKind.Semicolon); // This should match

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region IsMatch Sequence Matching Tests

    [Test]
    public void IsMatch_EmptySequence_ReturnsTrue()
    {
        // Arrange
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x")
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act - Empty sequence should always match
        var result = bufferedSource.IsMatch(0, Array.Empty<TokenKind[]>());

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsMatch_SingleTokenSequence_MatchesCorrectly()
    {
        // Arrange
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x")
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0, new[] { TokenKind.Int }), Is.True);
        Assert.That(bufferedSource.IsMatch(0, new[] { TokenKind.Identifier }), Is.False);
        Assert.That(bufferedSource.IsMatch(1, new[] { TokenKind.Identifier }), Is.True);
        Assert.That(bufferedSource.IsMatch(1, new[] { TokenKind.Int }), Is.False);
    }

    [Test]
    public void IsMatch_TwoTokenSequence_MatchesCorrectly()
    {
        // Arrange - "int x"
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x"),
            CreateTestToken(TokenKind.AssignEqual)
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },
            new[] { TokenKind.Identifier }), Is.True);

        Assert.That(bufferedSource.IsMatch(1,
            new[] { TokenKind.Identifier },
            new[] { TokenKind.AssignEqual }), Is.True);

        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Identifier },
            new[] { TokenKind.Int }), Is.False);
    }

    [Test]
    public void IsMatch_ComplexSequence_VariableDeclaration()
    {
        // Arrange - "int x = 42;"
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),           // 0
            CreateTestToken(TokenKind.Identifier, "x"), // 1
            CreateTestToken(TokenKind.AssignEqual),   // 2
            CreateTestToken(TokenKind.DecimalIntLiteral, "42"), // 3
            CreateTestToken(TokenKind.Semicolon)      // 4
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },                    // int
            new[] { TokenKind.Identifier },            // x
            new[] { TokenKind.AssignEqual },           // =
            new[] { TokenKind.DecimalIntLiteral },     // 42
            new[] { TokenKind.Semicolon }              // ;
        ), Is.True);

        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },
            new[] { TokenKind.Identifier }
        ), Is.True);

        Assert.That(bufferedSource.IsMatch(2,
            new[] { TokenKind.AssignEqual },
            new[] { TokenKind.DecimalIntLiteral },
            new[] { TokenKind.Semicolon }
        ), Is.True);
    }

    [Test]
    public void IsMatch_FunctionSignature_MatchesCorrectly()
    {
        // Arrange - "int main ( ) {"
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),           // 0 - return type
            CreateTestToken(TokenKind.Identifier, "main"), // 1 - function name
            CreateTestToken(TokenKind.LeftParen),     // 2 - (
            CreateTestToken(TokenKind.RightParen),    // 3 - )
            CreateTestToken(TokenKind.LeftBrace)      // 4 - {
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },                // return type
            new[] { TokenKind.Identifier },        // function name
            new[] { TokenKind.LeftParen },         // (
            new[] { TokenKind.RightParen },        // )
            new[] { TokenKind.LeftBrace }          // {
        ), Is.True);

        // match signature without body
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },
            new[] { TokenKind.Identifier },
            new[] { TokenKind.LeftParen },
            new[] { TokenKind.RightParen }
        ), Is.True);
    }

    [Test]
    public void IsMatch_AlternativeTokenTypes_MatchesCorrectly()
    {
        // Arrange - "int x" or "char y" or "void z"
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Char),          // 0
            CreateTestToken(TokenKind.Identifier, "y") // 1
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int, TokenKind.Char, TokenKind.Void },
            new[] { TokenKind.Identifier }
        ), Is.True);

        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int, TokenKind.Void },
            new[] { TokenKind.Identifier }
        ), Is.False);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void IsMatch_SequenceLongerThanAvailableTokens_ReturnsFalse()
    {
        // Arrange
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x")
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },
            new[] { TokenKind.Identifier },
            new[] { TokenKind.AssignEqual }
        ), Is.False);
    }

    [Test]
    public void IsMatch_StartingBeyondAvailableTokens_ReturnsFalse()
    {
        // Arrange
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int)
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(5, // Out of range
            new[] { TokenKind.Int }
        ), Is.False);
    }

    [Test]
    public void IsMatch_PartialMatchInMiddle_ReturnsFalse()
    {
        // Arrange - "int x = string"
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x"),
            CreateTestToken(TokenKind.AssignEqual),
            CreateTestToken(TokenKind.StringLiteral, "\"hello\"")
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new[] { TokenKind.Int },
            new[] { TokenKind.Identifier },
            new[] { TokenKind.AssignEqual },
            new[] { TokenKind.DecimalIntLiteral } // Actual to be string literal
        ), Is.False);
    }

    [Test]
    public void IsMatch_EmptyTokenGroups_MatchesAnything()
    {
        // Arrange
        var tokens = new[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.Identifier, "x")
        };
        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.IsMatch(0,
            new TokenKind[] { }, // Empty group should match anything
            new[] { TokenKind.Identifier }
        ), Is.True);
    }

    [Test]
    public void EdgeCase_EndOfFileToken_IsEnded()
    {
        // Arrange
        var tokens = new IToken[]
        {
            CreateTestToken(TokenKind.Int),
            CreateTestToken(TokenKind.EndOfFile)
        };

        var mockSource = new MockTokenSource(tokens);
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.HasToken(0), Is.True); // First token is not EOF
        Assert.That(bufferedSource.HasToken(1), Is.True);  // Second token is EOF
        Assert.That(bufferedSource.HasToken(2), Is.False);  // Beyond EOF
    }

    [Test]
    public void EdgeCase_EmptySourceWithLookahead()
    {
        // Arrange
        var mockSource = new MockTokenSource();
        var bufferedSource = new QueuedTokenSource(mockSource) as IBufferedTokenSource;

        // Act & Assert
        Assert.That(bufferedSource.PeekAt(0), Is.Null);
        Assert.That(bufferedSource.PeekAt(10), Is.Null);
        Assert.That(bufferedSource.HasToken(0), Is.False);
        Assert.That(bufferedSource.HasToken(10), Is.False);
        Assert.That(bufferedSource.IsMatch(0, TokenKind.Int), Is.False);
        Assert.That(bufferedSource.IsMatch(10, TokenKind.Int), Is.False);
    }

    #endregion
}