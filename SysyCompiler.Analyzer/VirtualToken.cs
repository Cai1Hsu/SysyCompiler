using System.Text;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer;

public readonly struct VirtualToken
{
    private readonly SyntaxToken token;

    public string Text => token.Text;

    public VirtualTokenKind Kind { get; }

    private VirtualToken(SyntaxToken token, VirtualTokenKind kind)
    {
        this.token = token;
        Kind = kind;
    }

    public override string ToString() => $"{Kind} {Text}";

    public static VirtualToken? Create(SyntaxToken token)
        => DetermineKind(token) is VirtualTokenKind determinedKind
            ? new VirtualToken(token, determinedKind)
            : null;

    public static VirtualTokenKind? DetermineKind(SyntaxToken token)
        => token.TokenKind switch
        {
            TokenKind.Identifier when token.Text is "main" => VirtualTokenKind.MAINTK,
            TokenKind.Identifier when token.Text is "getint" => VirtualTokenKind.GETINTTK,
            TokenKind.Identifier when token.Text is "printf" => VirtualTokenKind.PRINTFTK,
            TokenKind.Identifier => VirtualTokenKind.IDENFR,
            TokenKind.BinaryIntLiteral
            or TokenKind.OctalIntLiteral
            or TokenKind.DecimalIntLiteral
            or TokenKind.HexIntLiteral => VirtualTokenKind.INTCON,
            TokenKind.StringLiteral => VirtualTokenKind.STRCON,
            TokenKind.Const => VirtualTokenKind.CONSTTK,
            TokenKind.Int => VirtualTokenKind.INTTK,
            TokenKind.Break => VirtualTokenKind.BREAKTK,
            TokenKind.Continue => VirtualTokenKind.CONTINUETK,
            TokenKind.If => VirtualTokenKind.IFTK,
            TokenKind.Else => VirtualTokenKind.ELSETK,
            TokenKind.Bang => VirtualTokenKind.NOT,
            TokenKind.LogicAnd => VirtualTokenKind.AND,
            TokenKind.LogicOr => VirtualTokenKind.OR,
            TokenKind.While => VirtualTokenKind.WHILETK,
            TokenKind.Return => VirtualTokenKind.RETURNTK,
            TokenKind.Plus => VirtualTokenKind.PLUS,
            TokenKind.Minus => VirtualTokenKind.MINU,
            TokenKind.Void => VirtualTokenKind.VOIDTK,
            TokenKind.Star => VirtualTokenKind.MULT,
            TokenKind.Slash => VirtualTokenKind.DIV,
            TokenKind.Percent => VirtualTokenKind.MOD,
            TokenKind.Less => VirtualTokenKind.LSS,
            TokenKind.LessEqual => VirtualTokenKind.LEQ,
            TokenKind.Greater => VirtualTokenKind.GRE,
            TokenKind.GreaterEqual => VirtualTokenKind.GEQ,
            TokenKind.CompareEqual => VirtualTokenKind.EQL,
            TokenKind.NotEqual => VirtualTokenKind.NEQ,
            TokenKind.AssignEqual => VirtualTokenKind.ASSIGN,
            TokenKind.Semicolon => VirtualTokenKind.SEMICN,
            TokenKind.Comma => VirtualTokenKind.COMMA,
            TokenKind.LeftParen => VirtualTokenKind.LPARENT,
            TokenKind.RightParen => VirtualTokenKind.RPARENT,
            TokenKind.LeftBracket => VirtualTokenKind.LBRACK,
            TokenKind.RightBracket => VirtualTokenKind.RBRACK,
            TokenKind.LeftBrace => VirtualTokenKind.LBRACE,
            TokenKind.RightBrace => VirtualTokenKind.RBRACE,
            _ => null
        };
}
