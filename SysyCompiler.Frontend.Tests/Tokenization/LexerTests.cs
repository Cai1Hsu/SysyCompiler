using SysyCompiler.Frontend.Tokenization;
using SysyCompiler.Frontend.Tokenization.Tokens;

namespace SysyCompiler.Frontend.Tests.Tokenization;

[TestFixture]
public partial class LexerTests
{
    private Lexer lexer = null!;

    [TearDown]
    public void TearDown()
    {
        lexer = null!;
    }

    private Lexer CreateLexer(string sourceCode)
    {
        return lexer = new Lexer(sourceCode);
    }

    [Test]
    public void NextToken_EmptySource_ReturnsEOF()
    {
        // Arrange
        lexer = CreateLexer("");

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<EndOfFileToken>());
        Assert.That(token.Kind, Is.EqualTo(TokenKind.EndOfFile));
    }

    [Test]
    public void NextToken_WhitespaceOnly_ReturnsEOF()
    {
        // Arrange
        CreateLexer("   \t\n\r  ");

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<EndOfFileToken>());
        Assert.That(token.Kind, Is.EqualTo(TokenKind.EndOfFile));
    }

    #region Identifier Tests

    [TestCase("variable", "variable")]
    [TestCase("_private", "_private")]
    [TestCase("camelCase", "camelCase")]
    [TestCase("snake_case", "snake_case")]
    [TestCase("PascalCase", "PascalCase")]
    [TestCase("var123", "var123")]
    public void NextToken_Identifier_ReturnsIdentifierToken(string input, string expectedValue)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<IdentifierToken>());
        Assert.That(token.Kind, Is.EqualTo(TokenKind.Identifier));

        var identifierToken = (IdentifierToken)token;
        Assert.That(identifierToken.RawText, Is.EqualTo(expectedValue));
    }

    #endregion

    #region Keyword Tests

    [TestCase("if", TokenKind.If)]
    [TestCase("else", TokenKind.Else)]
    [TestCase("while", TokenKind.While)]
    [TestCase("for", TokenKind.For)]
    [TestCase("break", TokenKind.Break)]
    [TestCase("continue", TokenKind.Continue)]
    [TestCase("return", TokenKind.Return)]
    [TestCase("const", TokenKind.Const)]
    [TestCase("int", TokenKind.Int)]
    [TestCase("void", TokenKind.Void)]
    [TestCase("char", TokenKind.Char)]
    public void NextToken_Keyword_ReturnsKeywordToken(string input, TokenKind expectedKind)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<KeywordToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));
    }

    #endregion

    #region Numeric Literal Tests

    [TestCase("123", TokenKind.DecimalIntLiteral, "123")]
    [TestCase("0", TokenKind.DecimalIntLiteral, "0")]
    [TestCase("456789", TokenKind.DecimalIntLiteral, "456789")]
    public void NextToken_DecimalInteger_ReturnsNumericLiteralToken(string input, TokenKind expectedKind, string expectedValue)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<NumericLiteralToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));

        var numericToken = (NumericLiteralToken)token;
        Assert.That(numericToken.RawText, Is.EqualTo(expectedValue));
    }

    [TestCase("0x123", TokenKind.HexIntLiteral, "0x123")]
    [TestCase("0xFF", TokenKind.HexIntLiteral, "0xFF")]
    [TestCase("0xabc", TokenKind.HexIntLiteral, "0xabc")]
    [TestCase("0X456", TokenKind.HexIntLiteral, "0X456")]
    public void NextToken_HexInteger_ReturnsNumericLiteralToken(string input, TokenKind expectedKind, string expectedValue)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<NumericLiteralToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));

        var numericToken = (NumericLiteralToken)token;
        Assert.That(numericToken.RawText, Is.EqualTo(expectedValue));
    }

    [TestCase("3.14", TokenKind.FloatLiteral, "3.14")]
    [TestCase("0.5", TokenKind.FloatLiteral, "0.5")]
    [TestCase("123.456", TokenKind.FloatLiteral, "123.456")]
    [TestCase("1.0e10", TokenKind.FloatLiteral, "1.0e10")]
    [TestCase("2.5E-3", TokenKind.FloatLiteral, "2.5E-3")]
    public void NextToken_FloatLiteral_ReturnsNumericLiteralToken(string input, TokenKind expectedKind, string expectedValue)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<NumericLiteralToken>());
        Assert.That(token.Kind, Is.EqualTo(expectedKind));

        var numericToken = (NumericLiteralToken)token;
        Assert.That(numericToken.RawText, Is.EqualTo(expectedValue));
    }

    #endregion

    #region String Literal Tests

    [TestCase("\"hello\"", "\"hello\"")]
    [TestCase("\"world\"", "\"world\"")]
    [TestCase("\"\"", "\"\"")]
    [TestCase("\"hello world\"", "\"hello world\"")]
    [TestCase("\"line\\nbreak\"", "\"line\\nbreak\"")]
    [TestCase("\"tab\\there\"", "\"tab\\there\"")]
    public void NextToken_StringLiteral_ReturnsStringLiteralToken(string input, string expectedValue)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<StringLiteralToken>());
        Assert.That(token.Kind, Is.EqualTo(TokenKind.StringLiteral));

        var stringToken = (StringLiteralToken)token;
        Assert.That(stringToken.RawText, Is.EqualTo(expectedValue));
    }

    #endregion

    #region Character Literal Tests

    [TestCase("'a'", "'a'")]
    [TestCase("'Z'", "'Z'")]
    [TestCase("'1'", "'1'")]
    [TestCase("'\\n'", "'\\n'")]
    [TestCase("'\\t'", "'\\t'")]
    [TestCase("'\\\\'", "'\\\\'")]
    public void NextToken_CharLiteral_ReturnsCharLiteralToken(string input, string expectedValue)
    {
        // Arrange
        CreateLexer(input);

        // Act
        var token = lexer.NextToken();

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.TypeOf<CharLiteralToken>());
        Assert.That(token.Kind, Is.EqualTo(TokenKind.CharLiteral));

        var charToken = (CharLiteralToken)token;
        Assert.That(charToken.RawText, Is.EqualTo(expectedValue));
    }

    #endregion

    #region Multiple Token Tests

    [Test]
    public void NextToken_MultipleTokens_ReturnsTokensInOrder()
    {
        // Arrange
        CreateLexer("int x = 123;");
        var expectedTokens = new[]
        {
            TokenKind.Int,
            TokenKind.Identifier,
            TokenKind.AssignEqual,
            TokenKind.DecimalIntLiteral,
            TokenKind.Semicolon,
            TokenKind.EndOfFile
        };

        // Act
        var tokens = lexer.ToList();

        // Assert
        Assert.That(tokens.Count, Is.EqualTo(expectedTokens.Length),
            $"Expected {expectedTokens.Length} tokens but got {tokens.Count}");

        Assert.That(tokens.Select(t => t.Kind), Is.EqualTo(expectedTokens),
            "Token kinds do not match the expected sequence.");
    }

    [Test]
    public void NextToken_SimpleFunction_TokenizesCorrectly()
    {
        // Arrange
        var sourceCode = @"
            int main() {
                return 0;
            }";
        CreateLexer(sourceCode);

        // Act
        var tokens = new List<IToken>();
        IToken? token;
        while ((token = lexer.NextToken()) != null && token.Kind != TokenKind.EndOfFile)
        {
            tokens.Add(token);
        }

        // Assert
        Assert.That(tokens.Count, Is.GreaterThan(0));

        // 检查第一个token是 int 关键字
        Assert.That(tokens[0].Kind, Is.EqualTo(TokenKind.Int));

        // 检查第二个token是标识符 "main"
        Assert.That(tokens[1].Kind, Is.EqualTo(TokenKind.Identifier));
        var identifierToken = tokens[1] as IdentifierToken;
        Assert.That(identifierToken?.RawText, Is.EqualTo("main"));
    }

    #endregion

    #region Comment Tests

    [Test]
    public void NextToken_SingleLineComment_SkipsComment()
    {
        // Arrange
        CreateLexer("int // this is a comment\nvar");

        // Act
        var firstToken = lexer.NextToken();
        var secondToken = lexer.NextToken();

        // Assert
        Assert.That(firstToken?.Kind, Is.EqualTo(TokenKind.Int));
        Assert.That(secondToken?.Kind, Is.EqualTo(TokenKind.Identifier));

        var identifierToken = secondToken as IdentifierToken;
        Assert.That(identifierToken?.RawText, Is.EqualTo("var"));
    }

    [Test]
    public void NextToken_MultiLineComment_SkipsComment()
    {
        // Arrange
        CreateLexer("int /* this is a\n multi-line comment */ var");

        // Act
        var firstToken = lexer.NextToken();
        var secondToken = lexer.NextToken();

        // Assert
        Assert.That(firstToken?.Kind, Is.EqualTo(TokenKind.Int));
        Assert.That(secondToken?.Kind, Is.EqualTo(TokenKind.Identifier));

        var identifierToken = secondToken as IdentifierToken;
        Assert.That(identifierToken?.RawText, Is.EqualTo("var"));
    }

    #endregion

    #region Position Tests

    [Test]
    public void NextToken_TokensHaveCorrectPositions()
    {
        // Arrange
        CreateLexer("int x");

        // Act
        var intToken = lexer.NextToken();
        var identifierToken = lexer.NextToken();

        // Assert
        Assert.That(intToken?.Span.Start, Is.EqualTo(0));
        Assert.That(intToken?.Span.End, Is.EqualTo(3));

        Assert.That(identifierToken?.Span.Start, Is.EqualTo(4));
        Assert.That(identifierToken?.Span.End, Is.EqualTo(5));
    }

    #endregion
}