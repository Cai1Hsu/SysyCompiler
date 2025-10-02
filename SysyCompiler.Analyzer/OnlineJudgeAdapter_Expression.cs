using System.Diagnostics;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer;

public partial class OnlineJudgeAdapter
{
    public (VirtualExprKind, VirtualExprKind?) GetVirtualExprBaseline(VirtualExprKind expected, VirtualExprKind? upperbound = null)
    {
        var node = (ExpressionSyntax?)Peek();

        Debug.Assert(node is not null, "GetVirtExprBaseline should only be called when the current node is an ExpressionSyntax");

        var parent = Peek(1);

        switch (parent)
        {
            case IfStatementSyntax or WhileStatementSyntax:
                return (VirtualExprKind.Cond, upperbound);

            case ExpressionStatementSyntax when node is BinaryExpressionSyntax potentiallyAssign
                && potentiallyAssign.Operator.Token.TokenKind is TokenKind.AssignEqual:
                return (VirtualExprKind.None, upperbound);

            case ArrayDimensionSyntax:
                var baseLine = expected.Max(VirtualExprKind.Exp);

                // If inside array dimension of a variable declaration, treat as ConstExp
                if (GetClosest<VariableDefineSyntax>() is VariableDefineSyntax varDecl
                    && varDecl.ArrayDimensions?.Any(dim => ReferenceEquals(dim, parent)) is true)
                    baseLine = VirtualExprKind.ConstExp;

                // If inside parameter list, treat as ConstExp
                if (GetClosest<ParameterItemSyntax>() is ParameterItemSyntax parameterItem
                    && parameterItem.ArrayDimensions?.Any(dim => ReferenceEquals(dim, parent)) is true)
                    baseLine = VirtualExprKind.ConstExp;

                return (baseLine, upperbound);

            case ExpressionInitializerSyntax when IsInConstContext():
                return (VirtualExprKind.ConstExp, upperbound);

            case BinaryExpressionSyntax binaryExpr:
                var (baseline, upper) = GetVirtExprBaselineForBinaryParent(node, binaryExpr);

                if (baseline is VirtualExprKind b)
                    expected = b.Max(expected); // expand

                if (upper is VirtualExprKind u)
                    upperbound = u.Min(upperbound ?? u); // restrict

                return (expected, upperbound);

            case UnaryExpressionSyntax:
                // We limit's the operand's baseline to UnaryExp
                // And generate VirtualExpressions below UnaryExp in UnaryExpression
                return (VirtualExprKind.UnaryExp.Max(expected), upperbound);

            case GroupedExpressionSyntax:
                // Exp was generated in VisitGroupedExpression
                return (VirtualExprKind.Exp.Max(expected), upperbound);

            case ExpressionSyntax:
                return (expected, upperbound);

            default:
                return (VirtualExprKind.Exp, upperbound);
        }
    }

    private (VirtualExprKind?, VirtualExprKind?) GetVirtExprBaselineForBinaryParent(ExpressionSyntax node, BinaryExpressionSyntax parent)
    {
        VirtualExprKind? baseline = null;
        VirtualExprKind? upperbound = null;

        var op = parent.Operator.Token.TokenKind;
        bool isLeft = ReferenceEquals(parent.Left, node);

        // Assignment operator - left side should not generate hierarchy
        if (op is TokenKind.AssignEqual)
        {
            if (isLeft)
                (baseline, upperbound) = (VirtualExprKind.LVal, VirtualExprKind.LVal);
            else
                baseline = VirtualExprKind.Exp; // Right side: full expression
        }
        else if (op is TokenKind.Star or TokenKind.Slash or TokenKind.Percent)
        {
            // MulExp → UnaryExp | MulExp ('*' | '/' | '%') UnaryExp
            if (!isLeft)
                baseline = VirtualExprKind.UnaryExp; // Right: stop at UnaryExp
            else
                baseline = VirtualExprKind.MulExp; // Left: expand to MulExp
        }
        else if (op is TokenKind.Plus or TokenKind.Minus)
        {
            // AddExp → MulExp | AddExp ('+' | '−') MulExp
            if (!isLeft)
                baseline = VirtualExprKind.MulExp; // Right: stop at MulExp
            else
                baseline = VirtualExprKind.AddExp; // Left: expand to AddExp
        }
        else if (op is TokenKind.Less or TokenKind.Greater or
                 TokenKind.LessEqual or TokenKind.GreaterEqual)
        {
            // RelExp → AddExp | RelExp ('<' | '>' | '<=' | '>=') AddExp
            if (!isLeft)
                baseline = VirtualExprKind.AddExp; // Right: stop at AddExp
            else
                baseline = VirtualExprKind.RelExp; // Left: expand to RelExp
        }
        else if (op is TokenKind.CompareEqual || op is TokenKind.NotEqual)
        {
            // EqExp → RelExp | EqExp ('==' | '!=') RelExp
            if (!isLeft)
                baseline = VirtualExprKind.RelExp; // Right: stop at RelExp
            else
                baseline = VirtualExprKind.EqExp; // Left: expand to EqExp
        }
        else if (op is TokenKind.LogicAnd)
        {
            // LAndExp → EqExp | LAndExp '&&' EqExp
            if (!isLeft)
                baseline = VirtualExprKind.EqExp; // Right: stop at EqExp
            else
                baseline = VirtualExprKind.LAndExp; // Left: expand to LAndExp
        }
        else if (op is TokenKind.LogicOr)
        {
            // LOrExp → LAndExp | LOrExp '||' LAndExp
            if (!isLeft)
                baseline = VirtualExprKind.LAndExp; // Right: stop at LAndExp
            else
                baseline = VirtualExprKind.LOrExp; // Left: expand to LOrExp
        }

        return (baseline, upperbound);
    }

    // Unary(func call variants) generation
    public override OnlineJudgeOutput? VisitFunctionCallExpression(FunctionCallExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitFunctionCallExpression(node, list);

        if (IsGetintOrPrintf(node))
            return list;

        GenerateVirtualExpr(GetVirtualExprBaseline(VirtualExprKind.UnaryExp, VirtualExprKind.UnaryExp), list);

        return list;
    }

    public override OnlineJudgeOutput? VisitLiteralExpression(LiteralExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitLiteralExpression(node, list);

        // Number
        if (node.Token.TokenKind is TokenKind.DecimalIntLiteral or
            TokenKind.HexIntLiteral or
            TokenKind.OctalIntLiteral or
            TokenKind.BinaryIntLiteral)
        {
            GenerateVirtualExpr(GetVirtualExprBaseline(VirtualExprKind.UnaryExp), list);
        }

        return list;
    }


    // LVal (left value expression, equivalent to array/reference expressions)
    public override OnlineJudgeOutput? VisitReferenceExpression(ReferenceExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitReferenceExpression(node, list);

        var parent = Peek(1);

        if (parent is FunctionCallExpressionSyntax || parent is ArrayExpressionSyntax)
            return list;

        // Check if this is getint/printf identifier
        var name = node.Identifier.Text;
        if (name is "getint" or "printf")
            return list;

        VirtualExprKind baseLine = VirtualExprKind.UnaryExp;

        // Check if on left side of assignment
        if (parent is BinaryExpressionSyntax binary &&
            binary.Operator.Token.TokenKind is TokenKind.AssignEqual &&
            ReferenceEquals(binary.Left, node))
        {
            baseLine = VirtualExprKind.LVal;
        }

        GenerateVirtualExpr(GetVirtualExprBaseline(baseLine, VirtualExprKind.LVal), list);

        return list;
    }

    public override OnlineJudgeOutput? VisitArrayExpression(ArrayExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitArrayExpression(node, list);

        var parent = Peek(1);

        if (parent is FunctionCallExpressionSyntax || parent is ArrayExpressionSyntax)
            return list;

        VirtualExprKind baseLine = VirtualExprKind.UnaryExp;

        // Check if we're on the left side of an assignment
        if (parent is BinaryExpressionSyntax binary &&
            binary.Operator.Token.TokenKind is TokenKind.AssignEqual &&
            ReferenceEquals(binary.Left, node))
        {
            // On left side of assignment, don't output expression hierarchy
            baseLine = VirtualExprKind.LVal;
        }

        // Generate subsequent hierarchy starting from UnaryExp, warpping PrimaryExp
        GenerateVirtualExpr(GetVirtualExprBaseline(baseLine, VirtualExprKind.LVal), list);
        return list;
    }

    public override OnlineJudgeOutput? VisitUnaryExpression(UnaryExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitUnaryExpression(node, list);

        GenerateVirtualExpr(GetVirtualExprBaseline(VirtualExprKind.UnaryExp, VirtualExprKind.UnaryExp), list);

        return list;
    }

    public override OnlineJudgeOutput? VisitGroupedExpression(GroupedExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitGroupedExpression(node, list);

        GenerateVirtualExpr(GetVirtualExprBaseline(VirtualExprKind.PrimaryExp, VirtualExprKind.PrimaryExp), list);

        return list;
    }

    // Binary expressions (MulExp, AddExp, RelExp, EqExp, LAndExp, LOrExp)
    // Assignment is also a binary expression but handled specially
    public override OnlineJudgeOutput? VisitBinaryExpression(BinaryExpressionSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitBinaryExpression(node, list);

        var op = node.Operator.Token.TokenKind;

        VirtualExprKind upperbound = op switch
        {
            TokenKind.Star or TokenKind.Slash or TokenKind.Percent => VirtualExprKind.MulExp,
            TokenKind.Plus or TokenKind.Minus => VirtualExprKind.AddExp,
            TokenKind.Less or TokenKind.Greater or
            TokenKind.LessEqual or TokenKind.GreaterEqual => VirtualExprKind.RelExp,
            TokenKind.CompareEqual or TokenKind.NotEqual => VirtualExprKind.EqExp,
            TokenKind.LogicAnd => VirtualExprKind.LAndExp,
            TokenKind.LogicOr => VirtualExprKind.LOrExp,
            _ => VirtualExprKind.None // Assignment or others
        };

        GenerateVirtualExpr(GetVirtualExprBaseline(upperbound.Max(VirtualExprKind.MulExp), upperbound), list);

        return list;
    }
}
