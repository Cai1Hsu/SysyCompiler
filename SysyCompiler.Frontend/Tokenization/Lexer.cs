using System.Collections;
using SysyCompiler.Frontend.Tokenization.Tokens;

namespace SysyCompiler.Frontend.Tokenization;

public class Lexer : ITokenSource
{
    public TokenFactory TokenFactory { get; }
    public string SourceCode { get; }

    public Lexer(string sourceCode)
    {
        SourceCode = sourceCode;
        TokenFactory = new TokenFactory(SourceCode);
    }

    public char? PeekAt(int lookahead = 0)
    {
        var index = TokenFactory.CurrentPosition + lookahead;

        if (index >= SourceCode.Length)
            return null;

        return SourceCode[index];
    }

    public ReadOnlySpan<char> PeekSpanAt(int length, int lookahead = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);

        var index = TokenFactory.CurrentPosition + lookahead;

        if (index + length > SourceCode.Length)
            return "";

        return SourceCode.AsSpan(index, length);
    }

    public IToken? NextToken()
    {
        while (true)
        {
            SkipTrivias();

            TokenFactory.BeginToken();

            if (PeekAt() is not char peek)
                return TokenFactory.InitializeToken(new EndOfFileToken());

            if (peek == '/' && PeekAt(1) is '/' or '*')
            {
                SkipComment();
                continue;
            }

            if (char.IsLetter(peek) || peek == '_')
                return LexIdentifierOrKeyword();

            if (char.IsDigit(peek))
                return LexNumericLiteral();

            if (peek == '\"')
                return LexStringLiteral();

            if (peek == '\'')
                return LexCharLiteral();

            return LexOperatorOrPunctuation();
        }
    }

    public IToken LexIdentifierOrKeyword()
    {
        while (PeekAt() is char peek && (char.IsLetterOrDigit(peek) || peek == '_'))
            TokenFactory.CurrentPosition++;

        if (TokenKindExtensions.ParseKeyword(TokenFactory.TokenString) is TokenKind kind)
            return TokenFactory.InitializeToken(new KeywordToken(kind));

        return TokenFactory.InitializeToken(new IdentifierToken(TokenFactory.TokenString.ToString()));
    }

    private enum NumberLexState
    {
        Undetermined,
        Float,
        HexInteger,
        DecimalInteger,
        OctalInteger, // not supported
        BinaryInteger, // not supported
    }

    private NumberLexState TryDetermineNumberType()
    {
        var peek2 = PeekSpanAt(2);

        if (peek2.Length != 2 || peek2[0] != '0')
            return NumberLexState.Undetermined; // can not determine, maybe decimal or float

        return peek2[1] switch
        {
            'x' or 'X' => NumberLexState.HexInteger,
            'b' or 'B' => NumberLexState.BinaryInteger,
            'o' or 'O' => NumberLexState.OctalInteger,
            _ when char.IsDigit(peek2[1]) => NumberLexState.Undetermined, // can not determine, maybe decimal or float
            _ => NumberLexState.Undetermined, // TODO: may be invalid input
        };

    }

    public NumericLiteralToken LexNumericLiteral()
    {
        var parseState = TryDetermineNumberType();

        if (parseState != NumberLexState.Undetermined)
            TokenFactory.CurrentPosition += 2; // skip 0x, 0b, 0o

        while (PeekAt() is char peek)
        {
            if (parseState is NumberLexState.Undetermined)
            {
                if (peek == '.' || peek == 'e' || peek == 'E')
                {
                    parseState = NumberLexState.Float;
                }
                else if (!char.IsDigit(peek))
                {
                    break;
                }

                TokenFactory.CurrentPosition++;
            }
            else if (parseState is NumberLexState.Float)
            {
                if (char.IsDigit(peek) || peek == 'e' || peek == 'E' || peek == '+' || peek == '-')
                    TokenFactory.CurrentPosition++;
                else
                    break;
            }
            else if (parseState is NumberLexState.HexInteger)
            {
                if (char.IsDigit(peek) ||
                    // hex digits
                    (peek >= 'a' && peek <= 'f') ||
                    (peek >= 'A' && peek <= 'F'))
                    TokenFactory.CurrentPosition++;
                else
                    break;
            }
            else
            {
                // TODO: stricter validation will be done in semantic analysis
                if (char.IsDigit(peek))
                    TokenFactory.CurrentPosition++;
                else
                    break;
            }
        }

        TokenKind literalKind = parseState switch
        {
            NumberLexState.Float => TokenKind.FloatLiteral,
            NumberLexState.HexInteger => TokenKind.HexIntLiteral,
            NumberLexState.OctalInteger => TokenKind.OctalIntLiteral,
            NumberLexState.BinaryInteger => TokenKind.BinaryIntLiteral,
            NumberLexState.DecimalInteger => TokenKind.DecimalIntLiteral,
            NumberLexState.Undetermined => TokenKind.DecimalIntLiteral,
            _ => throw new NotImplementedException($"Invalid internal state: {parseState}"),
        };

        return TokenFactory.InitializeToken(new NumericLiteralToken(TokenFactory.TokenString.ToString(), literalKind));
    }

    public StringLiteralToken LexStringLiteral()
    {
        bool isEscaping = false;

        TokenFactory.CurrentPosition++; // eat '\"'

        while (PeekAt() is char peek)
        {
            TokenFactory.CurrentPosition++;

            // Finish escaping match
            // Current support single character escaping
            if (isEscaping)
            {
                isEscaping = false;
                continue;
            }

            if (peek == '\\')
            {
                isEscaping = true;
                continue;
            }

            if (peek == '\"')
                break;
        }

        return TokenFactory.InitializeToken(new StringLiteralToken(TokenFactory.TokenString.ToString()));
    }

    public CharLiteralToken LexCharLiteral()
    {
        bool isEscaping = false;

        TokenFactory.CurrentPosition++; // eat '\''

        while (PeekAt() is char peek)
        {
            TokenFactory.CurrentPosition++;

            // Finish escaping match
            // Current support single character escaping
            if (isEscaping)
            {
                isEscaping = false;
                continue;
            }

            if (peek == '\\')
            {
                isEscaping = true;
                continue;
            }

            if (!isEscaping && peek == '\'')
                break;
        }

        // We don't check how many char in the literal
        // This is done in semantic analysis
        return TokenFactory.InitializeToken(new CharLiteralToken(TokenFactory.TokenString.ToString()));
    }

    public IToken LexOperatorOrPunctuation()
    {
        const int LongestSymbolLength = 2;

        for (int len = LongestSymbolLength; len >= 1; len--)
        {
            var symbol = PeekSpanAt(len);

            if (symbol.Length != len)
                continue;

            if (TokenKindExtensions.ParseOperator(symbol) is TokenKind op)
            {
                TokenFactory.CurrentPosition += len;
                return TokenFactory.InitializeToken(new OperatorToken(op));
            }

            if (TokenKindExtensions.ParsePunctuation(symbol) is TokenKind punc)
            {
                TokenFactory.CurrentPosition += len;
                return TokenFactory.InitializeToken(new PunctuationToken(punc));
            }
        }

        return LexUnknown();
    }

    public UnknownToken LexUnknown()
    {
        static bool IsMatchSymbol(char c)
            => char.IsLetterOrDigit(c) || c == '_';

        bool isSymbol = IsMatchSymbol(PeekAt() ?? '\0');

        TokenFactory.CurrentPosition++;

        while (PeekAt() is char peek
            // Stop at whitespace
            && !char.IsWhiteSpace(peek)
            // Stop at comment
            && !(peek == '/' && (PeekAt(1) == '/' || PeekAt(1) == '*'))
            // Match the same kind of symbol
            && IsMatchSymbol(peek) == isSymbol)
            TokenFactory.CurrentPosition++;

        return TokenFactory.InitializeToken(new UnknownToken(TokenFactory.TokenString.ToString()));
    }

    public void SkipTrivias()
    {
        while (PeekAt() is char peek && char.IsWhiteSpace(peek))
        {
            TokenFactory.CurrentPosition++;
        }
    }

    public void SkipComment()
    {
        bool isBlockComment = PeekAt(1) == '*';
        TokenFactory.CurrentPosition += 2; // skip comment start

        while (PeekAt() is char peek)
        {
            TokenFactory.CurrentPosition++;

            if (peek == '\n' && !isBlockComment)
                // consumed
                break;

            if (isBlockComment && peek == '*'
                // Already consumed *
                && PeekAt() == '/')
            {
                TokenFactory.CurrentPosition += 2; // skip */
                break;
            }
        }
    }

    public IEnumerator<IToken> GetEnumerator()
        => new TokenSourceEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}