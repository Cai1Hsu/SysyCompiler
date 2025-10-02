using System.Collections.Frozen;

namespace SysyCompiler.Frontend.Tokenization;

public enum TokenKind
{
    /// <summary>
    /// Identifier, e.g., variable names, function names
    /// </summary>
    Identifier,

    #region Keywords

    /// <summary>
    /// if keyword
    /// </summary>
    If,
    /// <summary>
    /// else keyword
    /// </summary>
    Else,
    /// <summary>
    /// while keyword
    /// </summary>
    While,
    /// <summary>
    /// for keyword
    /// </summary>
    For,
    /// <summary>
    /// break keyword
    /// </summary>
    Break,
    /// <summary>
    /// continue keyword
    /// </summary>
    Continue,
    /// <summary>
    /// return keyword
    /// </summary>
    Return,
    /// <summary>
    /// const keyword
    /// </summary>
    Const,

    #endregion


    #region Primitive Types

    /// <summary>
    /// int keyword
    /// </summary>
    Int,
    /// <summary>
    /// char keyword
    /// </summary>
    Char,
    /// <summary>
    /// void keyword
    /// </summary>
    Void,

    #endregion

    #region Punctuation

    /// <summary>
    /// , comma
    /// </summary>
    Comma,
    /// <summary>
    /// . dot
    /// </summary>
    Dot,
    /// <summary>
    /// ( left parenthesis
    /// </summary>
    LeftParen,
    /// <summary>
    /// ) right parenthesis
    /// </summary>
    RightParen,
    /// <summary>
    /// { left brace
    /// </summary>
    LeftBrace,
    /// <summary>
    /// } right brace
    /// </summary>
    RightBrace,
    /// <summary>
    /// [ left bracket
    /// </summary>
    LeftBracket,
    /// <summary>
    /// ] right bracket
    /// </summary>
    RightBracket,
    /// <summary>
    /// ; semicolon
    /// </summary>
    Semicolon,

    #endregion

    #region Expression Operators

    /// <summary>
    ///  + addition operator
    /// </summary>
    Plus, // +
    /// <summary>
    /// - subtraction operator
    /// </summary>
    Minus, // -
    /// <summary>
    /// * multiplication operator
    /// </summary>
    Star, // *
    /// <summary>
    /// % modulus operator
    /// </summary>
    Percent, // %
    /// <summary>
    /// / division operator or comment start
    /// </summary>
    Slash, // /
    /// <summary>
    /// = assignment operator or equality operator
    /// </summary>
    AssignEqual, // =

    #endregion

    #region Logical Operators

    /// <summary>
    /// && logical AND operator
    /// </summary>
    LogicAnd,
    /// <summary>
    /// || logical OR operator
    /// </summary>
    LogicOr,
    /// <summary>
    /// == equality operator
    /// </summary>
    CompareEqual, // ==
    /// <summary>
    /// != not equal operator
    /// </summary>
    NotEqual, // !=
    /// <summary>
    /// ! negation operator
    /// </summary>
    Bang,
    /// <summary>
    /// < less than operator
    /// </summary>
    Less,
    /// <summary>
    /// <= less than or equal to operator
    /// </summary>
    LessEqual,
    /// <summary>
    /// > greater than operator
    /// </summary>
    Greater,
    /// <summary>
    /// >= greater than or equal to operator
    /// </summary>
    GreaterEqual,

    #endregion

    #region Literals

    /// <summary>
    /// Binary integer literal, e.g., 0b101, 0b0, 0b1111
    /// </summary>
    BinaryIntLiteral,
    /// <summary>
    /// Octal integer literal, e.g., 0o123, 0o0, 0o77
    /// </summary>
    OctalIntLiteral,
    /// <summary>
    /// Decimal integer literal, e.g., 123, 0, -456
    /// </summary>
    DecimalIntLiteral,
    /// <summary>
    /// Hexadecimal integer literal, e.g., 0x123, 0x0, 0xFF
    /// </summary>
    HexIntLiteral,
    /// <summary>
    /// Floating-point literal, e.g., 3.14, -0.5
    /// </summary>
    FloatLiteral,
    /// <summary>
    /// Character literal, e.g., 'a', 'b', '1'
    /// </summary>
    CharLiteral,
    /// <summary>
    /// String literal, e.g., "hello", "world"
    /// </summary>
    StringLiteral,

    #endregion

    #region Trivias

    /// <summary>
    /// Line comment
    /// </summary>
    LineComment,
    /// <summary>
    /// Block comment
    /// </summary>
    BlockComment,

    #endregion

    Unknown,

    EndOfFile,
}

public static class TokenKindExtensions
{
    public static string GetText(this TokenKind kind)
    {
        if (fixedTokenText.TryGetValue(kind, out var text))
        {
            return text;
        }

        throw new ArgumentException($"TokenKind {kind} does not have a fixed text representation.");
    }

    public static TokenKind? ParseKeyword(ReadOnlySpan<char> text)
        => keywordsLookup.TryGetValue(text, out var kind) ? kind : null;

    public static TokenKind? ParseOperator(ReadOnlySpan<char> text)
        => operatorsLookup.TryGetValue(text, out var kind) ? kind : null;

    public static TokenKind? ParsePunctuation(ReadOnlySpan<char> text)
        => punctuationsLookup.TryGetValue(text, out var kind) ? kind : null;

    private static readonly FrozenDictionary<string, TokenKind> keywordsMap;
    private static readonly FrozenDictionary<string, TokenKind>.AlternateLookup<ReadOnlySpan<char>> keywordsLookup;

    private static readonly FrozenDictionary<string, TokenKind> operatorsMap;
    private static readonly FrozenDictionary<string, TokenKind>.AlternateLookup<ReadOnlySpan<char>> operatorsLookup;

    private static readonly FrozenDictionary<string, TokenKind> punctuationsMap;
    private static readonly FrozenDictionary<string, TokenKind>.AlternateLookup<ReadOnlySpan<char>> punctuationsLookup;

    private static readonly FrozenDictionary<TokenKind, string> fixedTokenText;

    static TokenKindExtensions()
    {
        {
            var builder = new Dictionary<TokenKind, string>()
            {
                { TokenKind.If, "if" },
                { TokenKind.Else, "else" },
                { TokenKind.While, "while" },
                { TokenKind.For, "for" },
                { TokenKind.Break, "break" },
                { TokenKind.Continue, "continue" },
                { TokenKind.Return, "return" },
                { TokenKind.Const, "const" },
                { TokenKind.Int, "int" },
                { TokenKind.Char, "char" },
                { TokenKind.Void, "void" },
                { TokenKind.Comma, "," },
                { TokenKind.Dot, "." },
                { TokenKind.LeftParen, "(" },
                { TokenKind.RightParen, ")" },
                { TokenKind.LeftBrace, "{" },
                { TokenKind.RightBrace, "}" },
                { TokenKind.LeftBracket, "[" },
                { TokenKind.RightBracket, "]" },
                { TokenKind.Semicolon, ";" },
                { TokenKind.Plus, "+" },
                { TokenKind.Minus, "-" },
                { TokenKind.Star, "*" },
                { TokenKind.Percent, "%" },
                { TokenKind.Slash, "/" },
                { TokenKind.AssignEqual, "=" },
                { TokenKind.LogicAnd, "&&" },
                { TokenKind.LogicOr, "||" },
                { TokenKind.CompareEqual, "==" },
                { TokenKind.NotEqual, "!=" },
                { TokenKind.Bang, "!" },
                { TokenKind.Less, "<" },
                { TokenKind.LessEqual, "<=" },
                { TokenKind.Greater, ">" },
                { TokenKind.GreaterEqual, ">=" }
            };

            fixedTokenText = builder.ToFrozenDictionary();
        }

        {
            var builder = new Dictionary<string, TokenKind>(StringComparer.InvariantCulture)
            {
                { "if", TokenKind.If },
                { "while", TokenKind.While },
                { "else", TokenKind.Else },
                { "for", TokenKind.For },
                { "break", TokenKind.Break },
                { "continue", TokenKind.Continue },
                { "return", TokenKind.Return },
                { "const", TokenKind.Const },

                { "int", TokenKind.Int },
                { "void", TokenKind.Void },
                { "char", TokenKind.Char },
            };

            keywordsMap = builder.ToFrozenDictionary();
            keywordsLookup = keywordsMap.GetAlternateLookup<ReadOnlySpan<char>>();

            builder = new Dictionary<string, TokenKind>(StringComparer.InvariantCulture)
            {
                { "+", TokenKind.Plus },
                { "-", TokenKind.Minus },
                { "*", TokenKind.Star },
                { "/", TokenKind.Slash },
                { "%", TokenKind.Percent },

                { "!", TokenKind.Bang },

                { "&&", TokenKind.LogicAnd },
                { "||", TokenKind.LogicOr },

                { "=", TokenKind.AssignEqual },

                { "==", TokenKind.CompareEqual },
                { "!=", TokenKind.NotEqual },
                { "<", TokenKind.Less },
                { "<=", TokenKind.LessEqual },
                { ">", TokenKind.Greater },
                { ">=", TokenKind.GreaterEqual },
            };

            operatorsMap = builder.ToFrozenDictionary();
            operatorsLookup = operatorsMap.GetAlternateLookup<ReadOnlySpan<char>>();

            builder = new Dictionary<string, TokenKind>(StringComparer.InvariantCulture)
            {
                { ".", TokenKind.Dot },
                { ";", TokenKind.Semicolon },
                { ",", TokenKind.Comma },
                { "(", TokenKind.LeftParen },
                { ")", TokenKind.RightParen },
                { "{", TokenKind.LeftBrace },
                { "}", TokenKind.RightBrace },
                { "[", TokenKind.LeftBracket },
                { "]", TokenKind.RightBracket },
            };

            punctuationsMap = builder.ToFrozenDictionary();
            punctuationsLookup = punctuationsMap.GetAlternateLookup<ReadOnlySpan<char>>();
        }
    }
}
