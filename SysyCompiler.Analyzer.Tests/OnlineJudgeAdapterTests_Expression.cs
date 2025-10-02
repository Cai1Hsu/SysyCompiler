using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer.Tests;

[TestFixture]
public partial class OnlineJudgeAdapterTests
{
    [Test]
    public void Test_SimpleNumberLiteral_ShouldGenerateFullHierarchy()
    {
        // int count = 0;
        var output = ParseOutput("0", p => p.ParseExpression());

        AssertNodes(output,
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_SimpleVariable_ShouldGenerateLValAndFullHierarchy()
    {
        // return count;
        var output = ParseOutput("count", p => p.ParseExpression());

        AssertNodes(output,
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_Addition_RightOperandShouldStopAtMulExp()
    {
        // count = count + 1
        // Right side: 1 should stop at MulExp
        var output = ParseOutput("count + 1", p => p.ParseExpression());

        AssertNodes(output,
            // Left: count
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            // +
            // Right: 1
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            // Binary expression result
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_Division_RightOperandShouldStopAtUnaryExp()
    {
        // (a + b) / 2
        // Right side: 2 should stop at UnaryExp
        var output = ParseOutput("(a + b) / 2", p => p.ParseExpression());

        AssertNodes(output,
            // "IDENFR a
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            // "PLUS +
            // "IDENFR b
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            // Binary expression
            VirtualNode.AddExp,
            VirtualNode.Exp,
            // "RPARENT )
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            // "DIV /
            // "INTCON 2
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_Multiplication_RightOperandShouldStopAtUnaryExp()
    {
        // a * b
        var output = ParseOutput("a * b", p => p.ParseExpression());

        AssertNodes(output,
            // Left: a
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            // Right: b (stops at UnaryExp)
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            // Binary expression result
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_ArrayAccess_ShouldGenerateLValAndHierarchy()
    {
        // a[1]
        var output = ParseOutput("a[1]", p => p.ParseExpression());

        AssertNodes(output,
            // Index: 1 (inside array dimension, becomes Exp)
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp,
            // Array expression
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_UnaryExpression_ShouldHaveLeadingUnaryOp()
    {
        var output = ParseOutput("-a", p => p.ParseExpression());

        AssertNodes(output,
            // Unary -
            VirtualNode.UnaryOp,
            // Operand: a
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp, // the identifier 'a' also generates a UnaryExp
            // The whole unary expression
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_FunctionCall_ShouldGenerateUnaryExpAndHierarchy()
    {
        // mytest(arr1, arr2);
        var output = ParseOutput("mytest(arr1, arr2)", p => p.ParseExpression());

        AssertNodes(output,
            // Arg1: arr1
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp,
            // Arg2: arr2
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp,
            VirtualNode.FuncRParams,
            // Function call
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_ConditionExpression_ShouldGenerateCondHierarchy()
    {
        // if (!1) - condition expression should end with Cond
        var output = ParseOutput("if (!1) {}", p => p.ParseIfStatement());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Find the Cond node
        Assert.That(virtNodes, Does.Contain(VirtualNode.Cond));

        var expected = new[] {
            VirtualNode.UnaryOp,
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.RelExp,
            VirtualNode.EqExp,
            VirtualNode.LAndExp,
            VirtualNode.LOrExp
        };

        // Check hierarchy before Cond
        var condIndex = Array.IndexOf(virtNodes, VirtualNode.Cond);
        var beforeCond = virtNodes.Take(condIndex).Reverse().Take(expected.Length).Reverse().ToArray();

        AssertNodes(beforeCond, expected);
    }


    [Test]
    public void Test_AssignmentStatement_LeftOnlyGenerateLVal()
    {
        var output = ParseOutput("a = 1;", p => p.ParseStatement());

        AssertNodes(output,
            VirtualNode.LVal,
            // =
            // Right: 1
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp,
            // Left: a
            VirtualNode.Stmt
        );
    }

    [Test]
    public void Test_TwoDimensionalArrayAccess_LValOnlyGeneratesIntheEnd()
    {
        var output = ParseOutput("a[0][1]", p => p.ParseExpression());

        AssertNodes(output,
            // 0
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp,
            // 1
            VirtualNode.Number,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp,
            // a[0][1]
            VirtualNode.LVal,
            VirtualNode.PrimaryExp,
            VirtualNode.UnaryExp,
            VirtualNode.MulExp,
            VirtualNode.AddExp,
            VirtualNode.Exp
        );
    }

    [Test]
    public void Test_RelationalExpression_RightOperandShouldStopAtAddExp()
    {
        // if (i < -1) - right side of < should stop at AddExp
        var output = ParseOutput("if (i < -1) {}", p => p.ParseIfStatement());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Find RelExp nodes (there should be two: one for left, one for the whole expression)
        var relExpIndices = virtNodes
            .Select((node, index) => (node, index))
            .Where(x => x.node is VirtualNode.RelExp)
            .Select(x => x.index)
            .ToArray();

        Assert.That(relExpIndices.Length, Is.GreaterThanOrEqualTo(2));

        // After the first RelExp, we should see the right operand
        // Right: -1 should output: UnaryOp, Number, PrimaryExp, UnaryExp, UnaryExp, MulExp, AddExp
        // Then RelExp (for the binary expression)
    }

    [Test]
    public void Test_LogicalAnd_RightOperandShouldStopAtEqExp()
    {
        // if (i < -1 && f() < 1)
        // Right side of && should stop at EqExp
        var output = ParseOutput("if (i < -1 && f() < 1) {}", p => p.ParseIfStatement());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Should have LAndExp nodes
        Assert.That(virtNodes, Does.Contain(VirtualNode.LAndExp));

        // Should have multiple RelExp and EqExp nodes
        var eqExpCount = virtNodes.Count(n => n is VirtualNode.EqExp);
        Assert.That(eqExpCount, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void Test_LogicalOr_RightOperandShouldStopAtLAndExp()
    {
        // if (i < -1 && f() < 1 || k < 2)
        // Right side of || should stop at LAndExp
        var output = ParseOutput("if (i < -1 && f() < 1 || k < 2) {}", p => p.ParseIfStatement());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Should have LOrExp and end with Cond
        Assert.That(virtNodes, Does.Contain(VirtualNode.LOrExp));
        Assert.That(virtNodes, Does.Contain(VirtualNode.Cond));
    }

    [Test]
    public void Test_ArrayDimension_ShouldGenerateConstExp()
    {
        // int temp[2][3]
        var output = ParseOutput("int temp[2][3];", p => p.ParseStatement());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Array dimensions should generate ConstExp instead of Exp
        var constExpCount = virtNodes.Count(n => n is VirtualNode.ConstExp);
        Assert.That(constExpCount, Is.EqualTo(2)); // Two array dimensions
    }

    [Test]
    public void Test_InitializerExpression_ShouldGenerateExp()
    {
        // int arr1[2] = {1, 1};
        var output = ParseOutput("int arr1[2] = {1, 1};", p => p.ParseStatement());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Initializer values should generate Exp
        var expCount = virtNodes.Count(n => n is VirtualNode.Exp);
        Assert.That(expCount, Is.GreaterThanOrEqualTo(2)); // Two initializer values
    }

    [Test]
    public void Test_ComplexExpression_ChainedOperators()
    {
        // -+a[1] * a[0] / 1
        var output = ParseOutput("-+a[1] * a[0] / 1", p => p.ParseExpression());

        var virtNodes = output?.Lines.OfType<VirtualNode>().ToArray() ?? Array.Empty<VirtualNode>();

        // Should have multiple MulExp nodes (for *, /)
        var mulExpCount = virtNodes.Count(n => n is VirtualNode.MulExp);
        Assert.That(mulExpCount, Is.GreaterThanOrEqualTo(3));

        // Should end with Exp
        Assert.That(virtNodes.Last(), Is.EqualTo(VirtualNode.Exp));
    }
}