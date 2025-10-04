using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Frontend;

public partial class RecursiveDescentParser
{
    public FunctionCallExpressionSyntax ParseFunctionCallExpression(ExpressionSyntax? callee = null)
    {
        callee ??= ParseExpression();

        SyntaxToken openParenToken = ParseToken(TokenKind.LeftParen);

        ArgumentListSyntax argumentList = ParseArgumentList();

        SyntaxToken? closeParenToken = ParseNullableToken(TokenKind.RightParen);

        return new FunctionCallExpressionSyntax(
            callee,
            openParenToken,
            argumentList,
            closeParenToken
        );
    }

    public BinaryOperatorSyntax? ParseBinaryOperatorSyntax()
    {
        SyntaxToken? op = ParseNullableToken(
            TokenKind.Plus, TokenKind.Minus, TokenKind.Star, TokenKind.Slash, TokenKind.Percent,
            TokenKind.AssignEqual,
            TokenKind.LogicAnd, TokenKind.LogicOr,
            TokenKind.CompareEqual, TokenKind.NotEqual,
            TokenKind.Less, TokenKind.LessEqual,
            TokenKind.Greater, TokenKind.GreaterEqual
        );

        return op is SyntaxToken operatorToken ? new BinaryOperatorSyntax(operatorToken) : null;
    }

    public UnaryOperatorSyntax? ParseUnaryOperator()
    {
        SyntaxToken? op = ParseNullableToken(
            TokenKind.Plus, TokenKind.Minus, TokenKind.Bang
        );

        return op is SyntaxToken operatorToken ? new UnaryOperatorSyntax(operatorToken) : null;
    }

    public UnaryExpressionSyntax ParseUnaryExpression()
    {
        UnaryOperatorSyntax op = ParseUnaryOperator()!;
        ExpressionSyntax operand = ParseExpression();

        return new UnaryExpressionSyntax(op, operand);
    }

    public ReferenceExpressionSyntax ParseReferenceExpression()
    {
        SyntaxToken identifier = ParseToken(TokenKind.Identifier);

        return new ReferenceExpressionSyntax(identifier);
    }

    public GroupedExpressionSyntax ParseGroupedExpression()
    {
        SyntaxToken openParenToken = ParseToken(TokenKind.LeftParen);
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken closeParenToken = ParseToken(TokenKind.RightParen);

        return new GroupedExpressionSyntax(
            openParenToken,
            expression,
            closeParenToken
        );
    }

    public LiteralExpressionSyntax? ParseLiteralExpression()
    {
        SyntaxToken? literalToken = ParseNullableToken(
            TokenKind.BinaryIntLiteral,
            TokenKind.OctalIntLiteral,
            TokenKind.DecimalIntLiteral,
            TokenKind.HexIntLiteral,
            TokenKind.CharLiteral,
            TokenKind.StringLiteral
        );

        return literalToken is SyntaxToken token
            ? new LiteralExpressionSyntax(token)
            : null;
    }

    public ArrayExpressionSyntax ParseArrayExpression(ExpressionSyntax? @base)
    {
        if (@base is null)
            @base = ParseReferenceExpression();

        ArrayDimensionSyntax index = ParseArrayDimension();

        return new ArrayExpressionSyntax(@base, index);
    }

    public ExpressionSyntax ParseExpression()
    {
        return ParseBinaryExpression();
    }

    struct BinaryOperatorInfo
    {
        public int Precedence;
        public bool IsRightAssociative;

        public BinaryOperatorInfo(int precedence, bool isRightAssociative = false)
        {
            Precedence = precedence;
            IsRightAssociative = isRightAssociative;
        }
    }

    private static BinaryOperatorInfo? GetBinaryOperatorInfo(IToken? token)
    {
        switch (token?.Kind)
        {
            case TokenKind.Star:
            case TokenKind.Slash:
            case TokenKind.Percent:
                return new BinaryOperatorInfo(6);

            case TokenKind.Plus:
            case TokenKind.Minus:
                return new BinaryOperatorInfo(5);

            case TokenKind.Less:
            case TokenKind.LessEqual:
            case TokenKind.Greater:
            case TokenKind.GreaterEqual:
                return new BinaryOperatorInfo(4);

            case TokenKind.CompareEqual:
            case TokenKind.NotEqual:
                return new BinaryOperatorInfo(3);

            case TokenKind.LogicAnd:
                return new BinaryOperatorInfo(2);

            case TokenKind.LogicOr:
                return new BinaryOperatorInfo(1);

            case TokenKind.AssignEqual:
                return new BinaryOperatorInfo(0, true);

            default:
                return null;
        }
    }

    private static int? GetUnaryOperatorPrecedence(IToken? token) => (token?.Kind) switch
    {
        // Unary operators typically have higher precedence than binary operators
        TokenKind.Plus
        or TokenKind.Minus
        or TokenKind.Bang => 6,
        _ => null,
    };

    public ExpressionSyntax ParseBinaryExpression(int parentPrecedence = -1)
    {
        ExpressionSyntax left;
        {
            var unaryPrecedence = GetUnaryOperatorPrecedence(Source.PeekAt());
            if (unaryPrecedence is int precedence && precedence >= parentPrecedence)
            {
                var operatorToken = ParseUnaryOperator()!;
                var operand = ParseBinaryExpression(precedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
        }

        while (true)
        {
            var binaryInfo = GetBinaryOperatorInfo(Source.Peek);
            if (binaryInfo is not BinaryOperatorInfo info || info.Precedence <= parentPrecedence)
            {
                break;
            }

            // Close grouped expression, or definitely a missing close bracket
            for (int i = 0; i < 2; i++)
            {
                if (Source.IsMatch(i, TokenKind.RightParen, TokenKind.LeftBrace, TokenKind.Semicolon))
                    return left;
            }

            // This usually indicates the end of an expression
            if (ParseBinaryOperatorSyntax() is not BinaryOperatorSyntax operatorToken)
                break;

            if (info.IsRightAssociative)
            {
                info.Precedence -= 1;
            }

            var right = ParseBinaryExpression(info.Precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
        ExpressionSyntax ParseExpressionBase()
        {
            if (Source.IsMatch(0, TokenKind.LeftParen))
                return ParseGroupedExpression();

            if (ParseLiteralExpression() is { } literal)
                return literal;

            return ParseReferenceExpression();
        }

        var @base = ParseExpressionBase();

        while (true)
        {
            if (Source.IsMatch(0, TokenKind.LeftBracket))
            {
                @base = ParseArrayExpression(@base);
                continue;
            }

            if (Source.IsMatch(0, TokenKind.LeftParen))
            {
                @base = ParseFunctionCallExpression(@base);
                continue;
            }

            break;
        }

        return @base;
    }
}
