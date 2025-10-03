using System.Text;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;
using static SysyCompiler.Analyzer.OnlineJudgeAdapter;

namespace SysyCompiler.Analyzer;

public partial class OnlineJudgeAdapter
{
    public enum VirtualExprKind
    {
        None,
        LVal,
        PrimaryExp,
        UnaryExp,
        MulExp,
        AddExp,
        Exp,
        ConstExp,
        RelExp,
        EqExp,
        LAndExp,
        LOrExp,
        Cond,
    }

    public void GenerateLVal(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.LVal)
            return;

        list?.AddNode(VirtualNode.LVal);
    }

    public void GeneratePrimaryExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.PrimaryExp)
            return;

        // Numeric Literal
        if (this.Peek() is LiteralExpressionSyntax literalExpr
            && literalExpr.Token.TokenKind is TokenKind.BinaryIntLiteral
                or TokenKind.OctalIntLiteral
                or TokenKind.DecimalIntLiteral
                or TokenKind.HexIntLiteral)
            list?.AddNode(VirtualNode.Number);
        else if (this.Peek() is ArrayExpressionSyntax or ReferenceExpressionSyntax)
            GenerateLVal(list, upperbound);

        list?.AddNode(VirtualNode.PrimaryExp);
    }

    public void GenerateUnaryExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.UnaryExp)
            return;

        if (this.Peek() is not FunctionCallExpressionSyntax)
            GeneratePrimaryExp(list, upperbound);

        list?.AddNode(VirtualNode.UnaryExp);
    }

    public void GenerateMulExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.MulExp)
            return;

        GenerateUnaryExp(list, upperbound);
        list?.AddNode(VirtualNode.MulExp);
    }

    public void GenerateAddExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.AddExp)
            return;

        GenerateMulExp(list, upperbound);
        list?.AddNode(VirtualNode.AddExp);
    }

    public void GenerateExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.Exp)
            return;

        GenerateAddExp(list, upperbound);
        list?.AddNode(VirtualNode.Exp);
    }

    public void GenerateConstExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.ConstExp)
            return;

        GenerateAddExp(list, upperbound);
        list?.AddNode(VirtualNode.ConstExp);
    }

    public void GenerateRelExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.RelExp)
            return;

        GenerateAddExp(list, upperbound);
        list?.AddNode(VirtualNode.RelExp);
    }

    public void GenerateEqExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.EqExp)
            return;
        GenerateRelExp(list, upperbound);
        list?.AddNode(VirtualNode.EqExp);
    }

    public void GenerateLAndExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.LAndExp)
            return;

        GenerateEqExp(list, upperbound);
        list?.AddNode(VirtualNode.LAndExp);
    }

    public void GenerateLOrExp(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.LOrExp)
            return;

        GenerateLAndExp(list, upperbound);
        list?.AddNode(VirtualNode.LOrExp);
    }

    public void GenerateCond(OnlineJudgeOutput? list, VirtualExprKind? upperbound)
    {
        if (upperbound is VirtualExprKind upper
            && upper > VirtualExprKind.Cond)
            return;

        GenerateLOrExp(list, upperbound);
        list?.AddNode(VirtualNode.Cond);
    }

    public void GenerateVirtualExpr((VirtualExprKind, VirtualExprKind?) vExprKind, OnlineJudgeOutput? list)
    {
        var (baseline, upperbound) = vExprKind;

        switch (baseline)
        {
            case VirtualExprKind.None:
                break;
            case VirtualExprKind.LVal:
                GenerateLVal(list, upperbound);
                break;
            case VirtualExprKind.PrimaryExp:
                GeneratePrimaryExp(list, upperbound);
                break;
            case VirtualExprKind.UnaryExp:
                GenerateUnaryExp(list, upperbound);
                break;
            case VirtualExprKind.MulExp:
                GenerateMulExp(list, upperbound);
                break;
            case VirtualExprKind.AddExp:
                GenerateAddExp(list, upperbound);
                break;
            case VirtualExprKind.Exp:
                GenerateExp(list, upperbound);
                break;
            case VirtualExprKind.ConstExp:
                GenerateConstExp(list, upperbound);
                break;
            case VirtualExprKind.RelExp:
                GenerateRelExp(list, upperbound);
                break;
            case VirtualExprKind.EqExp:
                GenerateEqExp(list, upperbound);
                break;
            case VirtualExprKind.LAndExp:
                GenerateLAndExp(list, upperbound);
                break;
            case VirtualExprKind.LOrExp:
                GenerateLOrExp(list, upperbound);
                break;
            case VirtualExprKind.Cond:
                GenerateCond(list, upperbound);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

public static class VirtualExprKindExtensions
{
    public static VirtualExprKind Min(this VirtualExprKind lhs, VirtualExprKind rhs)
        => (VirtualExprKind)Math.Min((int)lhs, (int)rhs);

    public static VirtualExprKind Max(this VirtualExprKind lhs, VirtualExprKind rhs)
        => (VirtualExprKind)Math.Max((int)lhs, (int)rhs);
}

