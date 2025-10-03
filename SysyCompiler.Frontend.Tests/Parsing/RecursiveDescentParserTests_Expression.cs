using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Frontend.Tests.Parsing;

[TestFixture]
public class RecursiveDescentParserTests_Expression : RecursiveDescentParserTests
{
    #region Binary Operator Precedence Tests

    [Test]
    public void ParseExpression_LogicalAndWithComparison_ParsesWithCorrectPrecedence()
    {
        // Test case: a < -1 && f() > 0
        // Expected structure: (a < -1) && (f() > 0)
        // The && operator should be at the root, not <
        const string source = "a < -1 && f() > 0";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be && (LogicAnd)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicAnd),
                "Root operator should be && (LogicAnd), not <");

            // Left side: a < -1
            Assert.That(binary.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)binary.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));

            // Left side left operand: a
            Assert.That(leftBinary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());
            var leftRef = (ReferenceExpressionSyntax)leftBinary.Left;
            Assert.That(leftRef.Identifier.Text, Is.EqualTo("a"));

            // Left side right operand: -1
            Assert.That(leftBinary.Right, Is.InstanceOf<UnaryExpressionSyntax>());
            var unary = (UnaryExpressionSyntax)leftBinary.Right;
            Assert.That(unary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));
            Assert.That(unary.Operand, Is.InstanceOf<LiteralExpressionSyntax>());
            var literal = (LiteralExpressionSyntax)unary.Operand;
            Assert.That(literal.Token.Text, Is.EqualTo("1"));

            // Right side: f() > 0
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Greater));

            // Right side left operand: f()
            Assert.That(rightBinary.Left, Is.InstanceOf<FunctionCallExpressionSyntax>());
            var funcCall = (FunctionCallExpressionSyntax)rightBinary.Left;
            Assert.That(funcCall.Callee, Is.InstanceOf<ReferenceExpressionSyntax>());
            var funcRef = (ReferenceExpressionSyntax)funcCall.Callee;
            Assert.That(funcRef.Identifier.Text, Is.EqualTo("f"));

            // Right side right operand: 0
            Assert.That(rightBinary.Right, Is.InstanceOf<LiteralExpressionSyntax>());
            var rightLiteral = (LiteralExpressionSyntax)rightBinary.Right;
            Assert.That(rightLiteral.Token.Text, Is.EqualTo("0"));
        });
    }

    [Test]
    public void ParseExpression_MultiplicationAndAddition_ParsesWithCorrectPrecedence()
    {
        // Test case: a + b * c
        // Expected structure: a + (b * c)
        // Multiplication has higher precedence than addition
        const string source = "a + b * c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be + (Plus)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus),
                "Root operator should be +");

            // Left side: a
            Assert.That(binary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());
            var leftRef = (ReferenceExpressionSyntax)binary.Left;
            Assert.That(leftRef.Identifier.Text, Is.EqualTo("a"));

            // Right side: b * c
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));

            var b = (ReferenceExpressionSyntax)rightBinary.Left;
            Assert.That(b.Identifier.Text, Is.EqualTo("b"));

            var c = (ReferenceExpressionSyntax)rightBinary.Right;
            Assert.That(c.Identifier.Text, Is.EqualTo("c"));
        });
    }

    [Test]
    public void ParseExpression_LogicalOrAndLogicalAnd_ParsesWithCorrectPrecedence()
    {
        // Test case: a || b && c
        // Expected structure: a || (b && c)
        // && has higher precedence than ||
        const string source = "a || b && c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be || (LogicOr)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicOr));

            // Left side: a
            Assert.That(binary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());

            // Right side: b && c
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicAnd));
        });
    }

    [Test]
    public void ParseExpression_ComparisonAndEquality_ParsesWithCorrectPrecedence()
    {
        // Test case: a < b == c > d
        // Expected structure: (a < b) == (c > d)
        // Comparison (<, >) has higher precedence than equality (==)
        const string source = "a < b == c > d";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be == (CompareEqual)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.CompareEqual));

            // Left side: a < b
            Assert.That(binary.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)binary.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));

            // Right side: c > d
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Greater));
        });
    }

    [Test]
    public void ParseExpression_ComplexMixedOperators_ParsesWithCorrectPrecedence()
    {
        // Test case: a + b * c < d - e && f || g
        // Expected structure: ((a + (b * c)) < (d - e)) && f) || g
        // Priority: *, / > +, - > <, > > ==, != > && > ||
        const string source = "a + b * c < d - e && f || g";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root should be ||
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicOr));

            // Left side should be && expression
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var andExpr = (BinaryExpressionSyntax)root.Left;
            Assert.That(andExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicAnd));

            // Left side of && should be < expression
            Assert.That(andExpr.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var lessExpr = (BinaryExpressionSyntax)andExpr.Left;
            Assert.That(lessExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));

            // Left side of < should be + expression
            Assert.That(lessExpr.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var plusExpr = (BinaryExpressionSyntax)lessExpr.Left;
            Assert.That(plusExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Right side of + should be * expression
            Assert.That(plusExpr.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var mulExpr = (BinaryExpressionSyntax)plusExpr.Right;
            Assert.That(mulExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));
        });
    }

    #endregion

    #region Assignment Expression Tests

    [Test]
    public void ParseExpressionStatement_SimpleAssignment_ParsesAsSingleStatement()
    {
        // Test case: i = i + 1;
        // Should parse as a single expression statement with assignment
        const string source = "i = i + 1;";
        var parser = CreateParser(source);
        var statement = parser.ParseExpressionStatement();

        Assert.Multiple(() =>
        {
            Assert.That(statement, Is.InstanceOf<ExpressionStatementSyntax>());
            Assert.That(statement.Expression, Is.InstanceOf<BinaryExpressionSyntax>());

            var binary = (BinaryExpressionSyntax)statement.Expression;
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual),
                "The operator should be = (AssignEqual)");

            // Left side: i
            Assert.That(binary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());
            var leftRef = (ReferenceExpressionSyntax)binary.Left;
            Assert.That(leftRef.Identifier.Text, Is.EqualTo("i"));

            // Right side: i + 1
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Semicolon should be the actual semicolon, not AssignEqual
            Assert.That(statement.SemicolonToken?.TokenKind, Is.EqualTo(TokenKind.Semicolon),
                "Semicolon token should be Semicolon, not AssignEqual");
        });
    }

    [Test]
    public void ParseExpression_AssignmentWithComplexExpression_ParsesCorrectly()
    {
        // Test case: x = a + b * c
        // Expected structure: x = (a + (b * c))
        // Assignment should be at root, arithmetic on right side
        const string source = "x = a + b * c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be = (AssignEqual)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            // Left side: x
            Assert.That(binary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());
            var leftRef = (ReferenceExpressionSyntax)binary.Left;
            Assert.That(leftRef.Identifier.Text, Is.EqualTo("x"));

            // Right side: a + b * c
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Right side of +: b * c
            Assert.That(rightBinary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var mulExpr = (BinaryExpressionSyntax)rightBinary.Right;
            Assert.That(mulExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));
        });
    }

    [Test]
    public void ParseExpression_AssignmentIsRightAssociative_ParsesCorrectly()
    {
        // Test case: a = b = c
        // Expected structure: a = (b = c)
        // Assignment is right-associative
        const string source = "a = b = c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be = (AssignEqual)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            // Left side: a
            Assert.That(binary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());
            var a = (ReferenceExpressionSyntax)binary.Left;
            Assert.That(a.Identifier.Text, Is.EqualTo("a"));

            // Right side: b = c
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightAssign = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightAssign.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            var b = (ReferenceExpressionSyntax)rightAssign.Left;
            Assert.That(b.Identifier.Text, Is.EqualTo("b"));

            var c = (ReferenceExpressionSyntax)rightAssign.Right;
            Assert.That(c.Identifier.Text, Is.EqualTo("c"));
        });
    }

    [Test]
    public void ParseExpression_ArrayElementAssignment_ParsesCorrectly()
    {
        // Test case: arr[i] = value
        const string source = "arr[i] = value";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be =
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            // Left side: arr[i]
            Assert.That(binary.Left, Is.InstanceOf<ArrayExpressionSyntax>());
            var arrayExpr = (ArrayExpressionSyntax)binary.Left;

            Assert.That(arrayExpr.Base, Is.InstanceOf<ReferenceExpressionSyntax>());
            var arrRef = (ReferenceExpressionSyntax)arrayExpr.Base;
            Assert.That(arrRef.Identifier.Text, Is.EqualTo("arr"));

            // Right side: value
            Assert.That(binary.Right, Is.InstanceOf<ReferenceExpressionSyntax>());
            var valueRef = (ReferenceExpressionSyntax)binary.Right;
            Assert.That(valueRef.Identifier.Text, Is.EqualTo("value"));
        });
    }

    #endregion

    #region Unary Operator Tests

    [Test]
    public void ParseExpression_UnaryMinusWithHighPrecedence_ParsesCorrectly()
    {
        // Test case: -a * b
        // Expected structure: (-a) * b
        // Unary operators have higher precedence than binary operators
        const string source = "-a * b";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be *
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));

            // Left side: -a
            Assert.That(binary.Left, Is.InstanceOf<UnaryExpressionSyntax>());
            var unary = (UnaryExpressionSyntax)binary.Left;
            Assert.That(unary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));

            Assert.That(unary.Operand, Is.InstanceOf<ReferenceExpressionSyntax>());
            var aRef = (ReferenceExpressionSyntax)unary.Operand;
            Assert.That(aRef.Identifier.Text, Is.EqualTo("a"));

            // Right side: b
            Assert.That(binary.Right, Is.InstanceOf<ReferenceExpressionSyntax>());
            var bRef = (ReferenceExpressionSyntax)binary.Right;
            Assert.That(bRef.Identifier.Text, Is.EqualTo("b"));
        });
    }

    [Test]
    public void ParseExpression_LogicalNotWithComparison_ParsesCorrectly()
    {
        // Test case: !a < b
        // Expected structure: (!a) < b
        const string source = "!a < b";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be <
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));

            // Left side: !a
            Assert.That(binary.Left, Is.InstanceOf<UnaryExpressionSyntax>());
            var unary = (UnaryExpressionSyntax)binary.Left;
            Assert.That(unary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Bang));
        });
    }

    [Test]
    public void ParseExpression_DoubleNegation_ParsesCorrectly()
    {
        // Test case: --a
        // Expected structure: -(-a)
        const string source = "--a";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<UnaryExpressionSyntax>());
            var outerUnary = (UnaryExpressionSyntax)expression;
            Assert.That(outerUnary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));

            // Operand should be another unary expression
            Assert.That(outerUnary.Operand, Is.InstanceOf<UnaryExpressionSyntax>());
            var innerUnary = (UnaryExpressionSyntax)outerUnary.Operand;
            Assert.That(innerUnary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));

            // Final operand: a
            Assert.That(innerUnary.Operand, Is.InstanceOf<ReferenceExpressionSyntax>());
            var aRef = (ReferenceExpressionSyntax)innerUnary.Operand;
            Assert.That(aRef.Identifier.Text, Is.EqualTo("a"));
        });
    }

    #endregion

    #region Operator Associativity Tests

    [Test]
    public void ParseExpression_LeftAssociativeAddition_ParsesCorrectly()
    {
        // Test case: a + b + c
        // Expected structure: (a + b) + c
        // Addition is left-associative
        const string source = "a + b + c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost +
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Left side should be: a + b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            var a = (ReferenceExpressionSyntax)leftBinary.Left;
            Assert.That(a.Identifier.Text, Is.EqualTo("a"));

            var b = (ReferenceExpressionSyntax)leftBinary.Right;
            Assert.That(b.Identifier.Text, Is.EqualTo("b"));

            // Right side: c
            Assert.That(root.Right, Is.InstanceOf<ReferenceExpressionSyntax>());
            var c = (ReferenceExpressionSyntax)root.Right;
            Assert.That(c.Identifier.Text, Is.EqualTo("c"));
        });
    }

    [Test]
    public void ParseExpression_LeftAssociativeMultiplication_ParsesCorrectly()
    {
        // Test case: a * b * c
        // Expected structure: (a * b) * c
        // Multiplication is left-associative
        const string source = "a * b * c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost *
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));

            // Left side should be: a * b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));
        });
    }

    [Test]
    public void ParseExpression_LeftAssociativeSubtraction_ParsesCorrectly()
    {
        // Test case: a - b - c
        // Expected structure: (a - b) - c
        // Subtraction is left-associative
        const string source = "a - b - c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost -
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));

            // Left side should be: a - b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));

            var a = (ReferenceExpressionSyntax)leftBinary.Left;
            Assert.That(a.Identifier.Text, Is.EqualTo("a"));

            var b = (ReferenceExpressionSyntax)leftBinary.Right;
            Assert.That(b.Identifier.Text, Is.EqualTo("b"));

            var c = (ReferenceExpressionSyntax)root.Right;
            Assert.That(c.Identifier.Text, Is.EqualTo("c"));
        });
    }

    [Test]
    public void ParseExpression_LeftAssociativeDivision_ParsesCorrectly()
    {
        // Test case: a / b / c
        // Expected structure: (a / b) / c
        // Division is left-associative (important for correctness: a/b/c != a/(b/c))
        const string source = "a / b / c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost /
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Slash));

            // Left side should be: a / b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Slash));
        });
    }

    [Test]
    public void ParseExpression_LeftAssociativeComparison_ParsesCorrectly()
    {
        // Test case: a < b < c
        // Expected structure: (a < b) < c
        // Comparison operators are left-associative
        const string source = "a < b < c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost <
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));

            // Left side should be: a < b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));
        });
    }

    [Test]
    public void ParseExpression_LeftAssociativeLogicalAnd_ParsesCorrectly()
    {
        // Test case: a && b && c
        // Expected structure: (a && b) && c
        // Logical AND is left-associative
        const string source = "a && b && c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost &&
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicAnd));

            // Left side should be: a && b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicAnd));
        });
    }

    [Test]
    public void ParseExpression_LeftAssociativeLogicalOr_ParsesCorrectly()
    {
        // Test case: a || b || c
        // Expected structure: (a || b) || c
        // Logical OR is left-associative
        const string source = "a || b || c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be the rightmost ||
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicOr));

            // Left side should be: a || b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)root.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LogicOr));
        });
    }

    [Test]
    public void ParseExpression_ChainedAssignments_ParsesWithRightAssociativity()
    {
        // Test case: a = b = c = d
        // Expected structure: a = (b = (c = d))
        // Assignment is right-associative
        const string source = "a = b = c = d";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root operator should be = (leftmost assignment)
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            // Left: a
            var a = (ReferenceExpressionSyntax)root.Left;
            Assert.That(a.Identifier.Text, Is.EqualTo("a"));

            // Right side should be: b = c = d
            Assert.That(root.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var level2 = (BinaryExpressionSyntax)root.Right;
            Assert.That(level2.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            var b = (ReferenceExpressionSyntax)level2.Left;
            Assert.That(b.Identifier.Text, Is.EqualTo("b"));

            // Right side of b should be: c = d
            Assert.That(level2.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var level3 = (BinaryExpressionSyntax)level2.Right;
            Assert.That(level3.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            var c = (ReferenceExpressionSyntax)level3.Left;
            Assert.That(c.Identifier.Text, Is.EqualTo("c"));

            var d = (ReferenceExpressionSyntax)level3.Right;
            Assert.That(d.Identifier.Text, Is.EqualTo("d"));
        });
    }

    [Test]
    public void ParseExpression_AssignmentWithArithmeticOnRight_ParsesCorrectly()
    {
        // Test case: x = y = a + b
        // Expected structure: x = (y = (a + b))
        // Assignment has lower precedence than arithmetic
        const string source = "x = y = a + b";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root: x = ...
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));
            var x = (ReferenceExpressionSyntax)root.Left;
            Assert.That(x.Identifier.Text, Is.EqualTo("x"));

            // Right: y = a + b
            Assert.That(root.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var level2 = (BinaryExpressionSyntax)root.Right;
            Assert.That(level2.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            var y = (ReferenceExpressionSyntax)level2.Left;
            Assert.That(y.Identifier.Text, Is.EqualTo("y"));

            // Right of y: a + b
            Assert.That(level2.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var plusExpr = (BinaryExpressionSyntax)level2.Right;
            Assert.That(plusExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));
        });
    }

    [Test]
    public void ParseExpression_MixedAssociativities_ParsesCorrectly()
    {
        // Test case: a = b + c * d
        // Expected structure: a = (b + (c * d))
        // Tests precedence and left-associativity of arithmetic within right-associative assignment
        const string source = "a = b + c * d";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root: a = ...
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            // Right side: b + c * d
            Assert.That(root.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var plusExpr = (BinaryExpressionSyntax)root.Right;
            Assert.That(plusExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Right of +: c * d
            Assert.That(plusExpr.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var mulExpr = (BinaryExpressionSyntax)plusExpr.Right;
            Assert.That(mulExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));
        });
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [Test]
    public void ParseExpression_ParenthesesOverridePrecedence_ParsesCorrectly()
    {
        // Test case: (a + b) * c
        // Expected structure: (a + b) * c
        // Parentheses should force addition before multiplication
        const string source = "(a + b) * c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be *
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));

            // Left side: (a + b)
            Assert.That(binary.Left, Is.InstanceOf<GroupedExpressionSyntax>());
            var grouped = (GroupedExpressionSyntax)binary.Left;

            Assert.That(grouped.Expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var innerBinary = (BinaryExpressionSyntax)grouped.Expression;
            Assert.That(innerBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));
        });
    }

    [Test]
    public void ParseExpression_MixedComparisonOperators_ParsesCorrectly()
    {
        // Test case: a <= b >= c
        // All comparison operators have same precedence, should be left-associative
        const string source = "a <= b >= c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be >= (right-most gets evaluated second)
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.GreaterEqual));

            // Left side should be a <= b
            Assert.That(binary.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var leftBinary = (BinaryExpressionSyntax)binary.Left;
            Assert.That(leftBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.LessEqual));
        });
    }

    [Test]
    public void ParseExpression_FunctionCallInExpression_ParsesCorrectly()
    {
        // Test case: func(a + b) * c
        // Function call should be parsed first, then multiplication
        const string source = "func(a + b) * c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be *
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));

            // Left side: func(a + b)
            Assert.That(binary.Left, Is.InstanceOf<FunctionCallExpressionSyntax>());
            var funcCall = (FunctionCallExpressionSyntax)binary.Left;

            Assert.That(funcCall.ArgumentList.Items.Count, Is.EqualTo(1));
            var arg = funcCall.ArgumentList.Items[0];
            Assert.That(arg.Expression, Is.InstanceOf<BinaryExpressionSyntax>());

            var argExpr = (BinaryExpressionSyntax)arg.Expression;
            Assert.That(argExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));
        });
    }

    [Test]
    public void ParseExpression_AllArithmeticOperators_ParsesWithCorrectPrecedence()
    {
        // Test case: a + b - c * d / e % f
        // Expected: a + b - ((c * d) / e) % f
        // *, /, % are left-associative with same precedence (higher than +, -)
        const string source = "a + b - c * d / e % f";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var root = (BinaryExpressionSyntax)expression;

            // Root should be - (subtraction)
            Assert.That(root.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Minus));

            // Left of - should be: a + b
            Assert.That(root.Left, Is.InstanceOf<BinaryExpressionSyntax>());
            var addExpr = (BinaryExpressionSyntax)root.Left;
            Assert.That(addExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Right of - should contain *, /, %
            Assert.That(root.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightExpr = (BinaryExpressionSyntax)root.Right;
            Assert.That(rightExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Percent));
        });
    }

    [Test]
    public void ParseBlock_AssignmentInBlock_ParsesCorrectly()
    {
        // Test case: { i = i + 1; }
        // Should parse as a block with a single assignment statement
        const string source = "{ i = i + 1; }";
        var parser = CreateParser(source);
        var block = parser.ParseBlock();

        Assert.Multiple(() =>
        {
            Assert.That(block.Statements.Count, Is.EqualTo(1),
                "Block should contain exactly 1 statement");

            var stmt = block.Statements[0];
            Assert.That(stmt, Is.InstanceOf<ExpressionStatementSyntax>());

            var exprStmt = (ExpressionStatementSyntax)stmt;
            Assert.That(exprStmt.Expression, Is.InstanceOf<BinaryExpressionSyntax>());

            var binary = (BinaryExpressionSyntax)exprStmt.Expression;
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.AssignEqual));
        });
    }

    #endregion
}