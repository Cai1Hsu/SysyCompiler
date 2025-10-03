using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Frontend.Tests.Parsing;

[TestFixture]
public class RecursiveDescentParserTests
{
    public static RecursiveDescentParser CreateParser(string source)
        => new(new Lexer(source));

    [Test]
    public void ParseType_DetailedStructure_ParsesCorrectly()
    {
        const string source = "int";
        var parser = CreateParser(source);
        var typeSyntax = parser.ParseType();

        Assert.Multiple(() =>
        {
            Assert.That(typeSyntax.Token.TokenKind, Is.EqualTo(TokenKind.Int));
            Assert.That(typeSyntax.Token.Text, Is.EqualTo("int"));
            Assert.That(typeSyntax.Kind, Is.EqualTo(SyntaxKind.Type));
        });
    }

    [Test]
    public void ParseVariableDeclaration_DetailedStructure_ParsesCorrectly()
    {
        const string source = "const int value = 42;";
        var parser = CreateParser(source);
        var declaration = parser.ParseVariableDeclaration();

        Assert.Multiple(() =>
        {
            // Check modifier
            Assert.That(declaration.Modifier, Is.Not.Null);
            Assert.That(declaration.Modifier!.Token.TokenKind, Is.EqualTo(TokenKind.Const));
            Assert.That(declaration.Modifier.Token.Text, Is.EqualTo("const"));
            Assert.That(declaration.Modifier.Kind, Is.EqualTo(SyntaxKind.Modifier));

            // Check type
            Assert.That(declaration.Type.Token.TokenKind, Is.EqualTo(TokenKind.Int));
            Assert.That(declaration.Type.Token.Text, Is.EqualTo("int"));
            Assert.That(declaration.Type.Kind, Is.EqualTo(SyntaxKind.Type));

            // Check variable define
            var define = declaration.VariableDefine!;
            Assert.That(define.Identifier.TokenKind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(define.Identifier.Text, Is.EqualTo("value"));
            Assert.That(define.AssignToken!.TokenKind, Is.EqualTo(TokenKind.AssignEqual));

            // Check initializer
            Assert.That(define.Initializer, Is.InstanceOf<ExpressionInitializerSyntax>());
            var exprInit = (ExpressionInitializerSyntax)define.Initializer!;
            Assert.That(exprInit.Kind, Is.EqualTo(SyntaxKind.ExpressionInitializer));
            Assert.That(exprInit.Expression, Is.InstanceOf<LiteralExpressionSyntax>());

            var literal = (LiteralExpressionSyntax)exprInit.Expression;
            Assert.That(literal.Token.TokenKind, Is.EqualTo(TokenKind.DecimalIntLiteral));
            Assert.That(literal.Token.Text, Is.EqualTo("42"));
        });
    }

    [Test]
    public void ParseArrayDeclaration_DetailedStructure_ParsesCorrectly()
    {
        const string source = "int arr[10][20];";
        var parser = CreateParser(source);
        var declaration = parser.ParseVariableDeclaration();

        Assert.Multiple(() =>
        {
            var define = declaration.VariableDefine!;
            Assert.That(define.Identifier.Text, Is.EqualTo("arr"));
            Assert.That(define.ArrayDimensions, Is.Not.Null);
            Assert.That(define.ArrayDimensions!.Count, Is.EqualTo(2));

            // Check first dimension [10]
            var firstDim = define.ArrayDimensions?[0];
            Assert.That(firstDim?.LeftBracketToken.TokenKind, Is.EqualTo(TokenKind.LeftBracket));
            Assert.That(firstDim?.RightBracketToken.TokenKind, Is.EqualTo(TokenKind.RightBracket));
            Assert.That(firstDim?.Expression, Is.InstanceOf<LiteralExpressionSyntax>());

            var firstSize = (LiteralExpressionSyntax)firstDim?.Expression!;
            Assert.That(firstSize.Token.Text, Is.EqualTo("10"));

            // Check second dimension [20]
            var secondDim = define.ArrayDimensions?[1];
            Assert.That(secondDim?.Expression, Is.InstanceOf<LiteralExpressionSyntax>());
            var secondSize = (LiteralExpressionSyntax)secondDim?.Expression!;
            Assert.That(secondSize.Token.Text, Is.EqualTo("20"));
        });
    }

    [Test]
    public void ParseFunctionDeclaration_DetailedStructure_ParsesCorrectly()
    {
        const string source = """
int calculate(int x, int y) {
    return x + y;
}
""";
        var parser = CreateParser(source);
        var function = parser.ParseFunctionDeclaration();

        Assert.Multiple(() =>
        {
            Assert.That(function.Kind, Is.EqualTo(SyntaxKind.FunctionDeclaration));
            Assert.That(function.ReturnType.Token.Text, Is.EqualTo("int"));
            Assert.That(function.Identifier.Text, Is.EqualTo("calculate"));
            Assert.That(function.OpenParenToken.TokenKind, Is.EqualTo(TokenKind.LeftParen));
            Assert.That(function.CloseParenToken.TokenKind, Is.EqualTo(TokenKind.RightParen));

            // Check parameters
            var paramList = function.ParameterList!;
            Assert.That(paramList.Items.Count, Is.EqualTo(2));

            // First parameter: int x
            var firstParam = paramList.Items[0];
            Assert.That(firstParam.Type.Token.Text, Is.EqualTo("int"));
            Assert.That(firstParam.Identifier.Text, Is.EqualTo("x"));

            // Second parameter: int y
            var secondParam = paramList.Items[1];
            Assert.That(secondParam.Type.Token.Text, Is.EqualTo("int"));
            Assert.That(secondParam.Identifier.Text, Is.EqualTo("y"));

            // Check body
            Assert.That(function.Body, Is.Not.Null);
            Assert.That(function.Body.Kind, Is.EqualTo(SyntaxKind.Block));
            Assert.That(function.Body.OpenBraceToken.TokenKind, Is.EqualTo(TokenKind.LeftBrace));
            Assert.That(function.Body.CloseBraceToken.TokenKind, Is.EqualTo(TokenKind.RightBrace));
            Assert.That(function.Body.Statements.Count, Is.EqualTo(1));

            // Check return statement
            var returnStmt = function.Body.Statements[0];
            Assert.That(returnStmt, Is.InstanceOf<ReturnStatementSyntax>());
            var ret = (ReturnStatementSyntax)returnStmt;
            Assert.That(ret.ReturnKeyword.Text, Is.EqualTo("return"));
            Assert.That(ret.Expression, Is.InstanceOf<BinaryExpressionSyntax>());
        });
    }

    [Test]
    public void ParseNestedExpression_ParseCorrectly()
    {
        const string source = "a()[1](0) + 1";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(expression.Kind, Is.EqualTo(SyntaxKind.BinaryExpression));
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());

            var binary = (BinaryExpressionSyntax)expression;
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            var left = binary.Left; ;
            Assert.That(left, Is.InstanceOf<FunctionCallExpressionSyntax>());
            var funcCall = (FunctionCallExpressionSyntax)left;
            Assert.That(funcCall.ArgumentList.Items.Count, Is.EqualTo(1));
            var arg = funcCall.ArgumentList.Items[0];
            Assert.That(arg.Expression, Is.InstanceOf<LiteralExpressionSyntax>());
            var argLiteral = (LiteralExpressionSyntax)arg.Expression!;
            Assert.That(argLiteral.Token.Text, Is.EqualTo("0"));

            Assert.That(funcCall.Callee, Is.InstanceOf<ArrayExpressionSyntax>());
            var arrayExpr = (ArrayExpressionSyntax)funcCall.Callee;
            Assert.That(arrayExpr.Index.Expression, Is.InstanceOf<LiteralExpressionSyntax>());
            var indexLiteral = (LiteralExpressionSyntax)arrayExpr.Index.Expression!;
            Assert.That(indexLiteral.Token.Text, Is.EqualTo("1"));

            var arrayBase = arrayExpr.Base;
            Assert.That(arrayBase, Is.InstanceOf<FunctionCallExpressionSyntax>());
            var innerFuncCall = (FunctionCallExpressionSyntax)arrayBase;
            Assert.That(innerFuncCall.ArgumentList.Items.Count, Is.EqualTo(0));
            Assert.That(innerFuncCall.Callee, Is.InstanceOf<ReferenceExpressionSyntax>());
            var innerRef = (ReferenceExpressionSyntax)innerFuncCall.Callee;
            Assert.That(innerRef.Identifier.Text, Is.EqualTo("a"));
        });
    }

    [Test]
    public void ParseBinaryExpression_DetailedStructure_ParsesCorrectly()
    {
        const string source = "a + b * c";
        var parser = CreateParser(source);
        var expression = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            // Due to precedence, this should be: a + (b * c)
            Assert.That(expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binary = (BinaryExpressionSyntax)expression;

            // Root operator should be +
            Assert.That(binary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));

            // Left operand should be 'a'
            Assert.That(binary.Left, Is.InstanceOf<ReferenceExpressionSyntax>());
            var leftRef = (ReferenceExpressionSyntax)binary.Left;
            Assert.That(leftRef.Identifier.Text, Is.EqualTo("a"));

            // Right operand should be 'b * c'
            Assert.That(binary.Right, Is.InstanceOf<BinaryExpressionSyntax>());
            var rightBinary = (BinaryExpressionSyntax)binary.Right;
            Assert.That(rightBinary.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Star));

            // Check 'b' and 'c'
            var bRef = (ReferenceExpressionSyntax)rightBinary.Left;
            Assert.That(bRef.Identifier.Text, Is.EqualTo("b"));

            var cRef = (ReferenceExpressionSyntax)rightBinary.Right;
            Assert.That(cRef.Identifier.Text, Is.EqualTo("c"));
        });
    }

    [Test]
    public void ParseIfStatement_DetailedStructure_ParsesCorrectly()
    {
        const string source = "if (x > 0) { return x; } else { return 0; }";
        var parser = CreateParser(source);
        var statement = parser.ParseIfStatement();

        Assert.Multiple(() =>
        {
            Assert.That(statement.Kind, Is.EqualTo(SyntaxKind.IfStatement));
            Assert.That(statement.IfKeyword.Text, Is.EqualTo("if"));
            Assert.That(statement.OpenParenToken.TokenKind, Is.EqualTo(TokenKind.LeftParen));
            Assert.That(statement.CloseParenToken?.TokenKind, Is.EqualTo(TokenKind.RightParen));

            // Check condition
            Assert.That(statement.Condition, Is.InstanceOf<BinaryExpressionSyntax>());
            var condition = (BinaryExpressionSyntax)statement.Condition;
            Assert.That(condition.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Greater));

            // Check then statement
            Assert.That(statement.ThenStatement, Is.InstanceOf<BlockStatementSyntax>());

            // Check else statement
            Assert.That(statement.ElseStatement, Is.Not.Null);
            Assert.That(statement.ElseKeyword, Is.Not.Null);
            Assert.That(statement.ElseKeyword!.Text, Is.EqualTo("else"));
            Assert.That(statement.ElseStatement, Is.InstanceOf<BlockStatementSyntax>());
        });
    }

    [Test]
    public void ParseArrayInitializer_DetailedStructure_ParsesCorrectly()
    {
        const string source = "{ 1, 2, { 3, 4 } }";
        var parser = CreateParser(source);
        var initializer = parser.ParseArrayInitializer();

        Assert.Multiple(() =>
        {
            Assert.That(initializer.Kind, Is.EqualTo(SyntaxKind.ArrayInitializer));
            Assert.That(initializer.OpenBraceToken.TokenKind, Is.EqualTo(TokenKind.LeftBrace));
            Assert.That(initializer.CloseBraceToken.TokenKind, Is.EqualTo(TokenKind.RightBrace));
            Assert.That(initializer.Items.Count, Is.EqualTo(3));

            // First item: 1
            var firstItem = initializer.Items[0];
            Assert.That(firstItem.Initializer.Kind, Is.EqualTo(SyntaxKind.ExpressionInitializer));

            // Second item: 2
            var secondItem = initializer.Items[1];
            Assert.That(secondItem.Initializer.Kind, Is.EqualTo(SyntaxKind.ExpressionInitializer));

            // Third item: { 3, 4 } (nested array)
            var thirdItem = initializer.Items[2];
            Assert.That(thirdItem.Initializer.Kind, Is.EqualTo(SyntaxKind.ArrayInitializer));

            var nestedArray = (ArrayInitializerSyntax)thirdItem.Initializer;
            Assert.That(nestedArray.Items.Count, Is.EqualTo(2));
        });
    }

    [Test]
    public void ParseFunctionCall_DetailedStructure_ParsesCorrectly()
    {
        const string source = "printf(\"Value: %d\", x + 1)";
        var parser = CreateParser(source);

        // FIXME: If we call ParseFunctionCallExpression here
        // The callee internally calls ParseExpression, resulting the callee to be the whole function call expression
        var parsed = parser.ParseExpression();

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.InstanceOf<FunctionCallExpressionSyntax>());
            var expression = (FunctionCallExpressionSyntax)parsed;

            Assert.That(expression.Kind, Is.EqualTo(SyntaxKind.FunctionCallExpression));
            Assert.That(expression.Callee, Is.InstanceOf<ReferenceExpressionSyntax>());
            var callee = (ReferenceExpressionSyntax)expression.Callee;
            Assert.That(callee.Identifier.Text, Is.EqualTo("printf"));
            Assert.That(expression.OpenParenToken.TokenKind, Is.EqualTo(TokenKind.LeftParen));
            Assert.That(expression.CloseParenToken.TokenKind, Is.EqualTo(TokenKind.RightParen));

            // Check arguments
            var args = expression.ArgumentList.Items;
            Assert.That(args.Count, Is.EqualTo(2));

            // First argument: string literal
            var firstArg = args[0];
            Assert.That(firstArg.Expression, Is.InstanceOf<LiteralExpressionSyntax>());
            var strLiteral = (LiteralExpressionSyntax)firstArg.Expression;
            Assert.That(strLiteral.Token.TokenKind, Is.EqualTo(TokenKind.StringLiteral));

            // Second argument: binary expression x + 1
            var secondArg = args[1];
            Assert.That(secondArg.Expression, Is.InstanceOf<BinaryExpressionSyntax>());
            var binaryExpr = (BinaryExpressionSyntax)secondArg.Expression;
            Assert.That(binaryExpr.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Plus));
        });
    }

    [Test]
    public void ParseWhileStatement_DetailedStructure_ParsesCorrectly()
    {
        const string source = "while (i < count) { i = i + 1; }";
        var parser = CreateParser(source);
        var statement = parser.ParseWhileStatement();

        Assert.Multiple(() =>
        {
            Assert.That(statement.Kind, Is.EqualTo(SyntaxKind.WhileStatement));
            Assert.That(statement.WhileKeyword.Text, Is.EqualTo("while"));
            Assert.That(statement.OpenParenToken.TokenKind, Is.EqualTo(TokenKind.LeftParen));
            Assert.That(statement.CloseParenToken?.TokenKind, Is.EqualTo(TokenKind.RightParen));

            // Check condition
            Assert.That(statement.Condition, Is.InstanceOf<BinaryExpressionSyntax>());
            var condition = (BinaryExpressionSyntax)statement.Condition;
            Assert.That(condition.Operator.Token.TokenKind, Is.EqualTo(TokenKind.Less));

            var leftOperand = (ReferenceExpressionSyntax)condition.Left;
            Assert.That(leftOperand.Identifier.Text, Is.EqualTo("i"));

            var rightOperand = (ReferenceExpressionSyntax)condition.Right;
            Assert.That(rightOperand.Identifier.Text, Is.EqualTo("count"));

            // Check body
            Assert.That(statement.Body, Is.InstanceOf<BlockStatementSyntax>());
            var blockStmt = (BlockStatementSyntax)statement.Body;
            Assert.That(blockStmt.Block.Statements.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void ParseCompilationUnit_DetailedStructure_ParsesCorrectly()
    {
        const string source = """
int globalVar = 100;

int main() {
    int localVar = globalVar + 50;
    return localVar;
}
""";
        var parser = CreateParser(source);
        var unit = parser.ParseCompilationUnit();

        Assert.Multiple(() =>
        {
            Assert.That(unit.Kind, Is.EqualTo(SyntaxKind.CompilationUnit));
            Assert.That(unit.Members.Count, Is.EqualTo(2));

            // Check global variable
            var globalVarDecl = (VariableDeclarationSyntax)unit.Members[0];
            Assert.That(globalVarDecl.Kind, Is.EqualTo(SyntaxKind.VariableDeclaration));
            Assert.That(globalVarDecl.VariableDefine!.Identifier.Text, Is.EqualTo("globalVar"));
            Assert.That(globalVarDecl.VariableDefine.Initializer, Is.Not.Null);

            // Check function
            var mainFunc = (FunctionDeclarationSyntax)unit.Members[1];
            Assert.That(mainFunc.Kind, Is.EqualTo(SyntaxKind.FunctionDeclaration));
            Assert.That(mainFunc.Identifier.Text, Is.EqualTo("main"));
            Assert.That(mainFunc.ReturnType.Token.TokenKind, Is.EqualTo(TokenKind.Int));

            // Check function body has 2 statements
            Assert.That(mainFunc.Body.Statements.Count, Is.EqualTo(2));

            // First statement should be local declaration
            var firstStmt = mainFunc.Body.Statements[0];
            Assert.That(firstStmt, Is.InstanceOf<LocalDeclarationStatementSyntax>(), firstStmt.ToString());

            // Second statement should be return statement
            var secondStmt = mainFunc.Body.Statements[1];
            Assert.That(secondStmt, Is.InstanceOf<ReturnStatementSyntax>());
        });
    }
}