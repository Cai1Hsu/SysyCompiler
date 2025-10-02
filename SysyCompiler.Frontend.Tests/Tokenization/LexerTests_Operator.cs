using SysyCompiler.Frontend.Tokenization;
using SysyCompiler.Frontend.Tokenization.Tokens;

namespace SysyCompiler.Frontend.Tests.Tokenization;

[TestFixture]
public partial class LexerTests
{
    #region Arithmetic Operators

    [TestCase("+", TokenKind.Plus)]
    [TestCase("-", TokenKind.Minus)]
    [TestCase("*", TokenKind.Star)]
    [TestCase("/", TokenKind.Slash)]
    [TestCase("%", TokenKind.Percent)]
    public void NextToken_ArithmeticOperators_ReturnsCorrectTokenKind(string input, TokenKind expectedKind)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<OperatorToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));
    }

    #endregion

    #region Comparison Operators

    [TestCase("==", TokenKind.CompareEqual)]
    [TestCase("!=", TokenKind.NotEqual)]
    [TestCase("<", TokenKind.Less)]
    [TestCase("<=", TokenKind.LessEqual)]
    [TestCase(">", TokenKind.Greater)]
    [TestCase(">=", TokenKind.GreaterEqual)]
    public void NextToken_ComparisonOperators_ReturnsCorrectTokenKind(string input, TokenKind expectedKind)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<OperatorToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));
    }

    #endregion

    #region Logical Operators

    [TestCase("&&", TokenKind.LogicAnd)]
    [TestCase("||", TokenKind.LogicOr)]
    [TestCase("!", TokenKind.Bang)]
    public void NextToken_LogicalOperators_ReturnsCorrectTokenKind(string input, TokenKind expectedKind)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<OperatorToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));
    }

    #endregion

    #region Assignment Operators

    [TestCase("=", TokenKind.AssignEqual)]
    public void NextToken_AssignmentOperators_ReturnsCorrectTokenKind(string input, TokenKind expectedKind)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<OperatorToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));
    }

    #endregion

    #region Punctuation

    [TestCase("(", TokenKind.LeftParen)]
    [TestCase(")", TokenKind.RightParen)]
    [TestCase("{", TokenKind.LeftBrace)]
    [TestCase("}", TokenKind.RightBrace)]
    [TestCase("[", TokenKind.LeftBracket)]
    [TestCase("]", TokenKind.RightBracket)]
    [TestCase(";", TokenKind.Semicolon)]
    [TestCase(",", TokenKind.Comma)]
    [TestCase(".", TokenKind.Dot)]
    public void NextToken_Punctuation_ReturnsCorrectTokenKind(string input, TokenKind expectedKind)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<PunctuationToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));
    }

    #endregion

    #region Complex Expressions

    [Test]
    public void NextToken_ArithmeticExpression_TokenizesCorrectly()
    {
        // Arrange
        CreateLexer("a + b * c");
        var expectedTokens = new[]
        {
            TokenKind.Identifier,  // a
            TokenKind.Plus,        // +
            TokenKind.Identifier,  // b
            TokenKind.Star,        // *
            TokenKind.Identifier,  // c
            TokenKind.EndOfFile
        };

        // Act
        var tokens = lexer.ToList();

        // Assert
        Assert.That(tokens.Select(t => t.Kind), Is.EqualTo(expectedTokens),
            "Token kinds do not match the expected sequence.");
    }

    [Test]
    public void NextToken_ComparisonExpression_TokenizesCorrectly()
    {
        // Arrange
        CreateLexer("x == 42 && y != 0");
        var expectedTokens = new[]
        {
            TokenKind.Identifier,      // x
            TokenKind.CompareEqual,    // ==
            TokenKind.DecimalIntLiteral, // 42
            TokenKind.LogicAnd,        // &&
            TokenKind.Identifier,      // y
            TokenKind.NotEqual,        // !=
            TokenKind.DecimalIntLiteral,  // 0
            TokenKind.EndOfFile
        };

        // Act        
        var tokens = lexer.ToList();

        // Assert
        Assert.That(tokens.Select(t => t.Kind), Is.EqualTo(expectedTokens),
            "Token kinds do not match the expected sequence.");
    }

    [Test]
    public void NextToken_FunctionCall_TokenizesCorrectly()
    {
        // Arrange
        CreateLexer("func(a, b)");
        var expectedTokens = new[]
        {
            TokenKind.Identifier,    // func
            TokenKind.LeftParen,     // (
            TokenKind.Identifier,    // a
            TokenKind.Comma,         // ,
            TokenKind.Identifier,    // b
            TokenKind.RightParen,    // )
            TokenKind.EndOfFile
        };

        // Act
        var tokens = lexer.ToList();

        // Assert
        Assert.That(tokens.Select(t => t.Kind), Is.EqualTo(expectedTokens),
            "Token kinds do not match the expected sequence.");
    }

    #endregion
}