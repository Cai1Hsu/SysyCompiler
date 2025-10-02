using System.Text;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer;

public partial class OnlineJudgeAdapter : StackSyntaxVisitor<OnlineJudgeOutput>
{
    public static bool IsMain(FunctionDeclarationSyntax? func)
        => func is FunctionDeclarationSyntax node
        && node.Identifier is SyntaxToken identToken
        && identToken.TokenKind is TokenKind.Identifier
        && identToken.Text is "main";

    public static bool IsGetintOrPrintf(FunctionCallExpressionSyntax? funcCall)
    {
        if (funcCall?.Callee is not ReferenceExpressionSyntax funcRef)
            return false;

        var name = funcRef.Identifier.Text;
        return name is "getint" or "printf";
    }

    // CompUnit
    public override OnlineJudgeOutput? VisitCompilationUnit(CompilationUnitSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitCompilationUnit(node, list);

        list?.AddNode(VirtualNode.CompUnit);
        return list;
    }

    // Tokens
    public override OnlineJudgeOutput? VisitSyntaxToken(SyntaxToken node, OnlineJudgeOutput? list)
    {
        list = base.VisitSyntaxToken(node, list);
        list?.AddToken(node);
        return list;
    }

    // Block
    public override OnlineJudgeOutput? VisitBlock(BlockSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitBlock(node, list);

        list?.AddNode(VirtualNode.Block);
        return list;
    }

    // VarDecl / ConstDecl
    public override OnlineJudgeOutput? VisitVariableDeclaration(VariableDeclarationSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitVariableDeclaration(node, list);

        // Check if it's const or var declaration
        if (node.Modifier?.Token.TokenKind is TokenKind.Const)
            list?.AddNode(VirtualNode.ConstDecl);
        else
            list?.AddNode(VirtualNode.VarDecl);

        return list;
    }

    // VarDef / ConstDef
    public override OnlineJudgeOutput? VisitVariableDefine(VariableDefineSyntax node, OnlineJudgeOutput? list)
    {
        // list = base.VisitVariableDefine(node, list);
        list = node.Identifier.Accept(this, list);
        list = node.ArrayDimensions?.Accept(this, list) ?? list;
        list = node.AssignToken?.Accept(this, list) ?? list;
        list = node.Initializer?.Accept(this, list) ?? list;

        // Check if in const context using GetClosest
        if (IsInConstContext())
            list?.AddNode(VirtualNode.ConstDef);
        else
            list?.AddNode(VirtualNode.VarDef);

        // The OJ requires that VarDef/ConstDef appears before comma token
        list = node.CommaToken?.Accept(this, list) ?? list;

        return list;
    }

    // FuncType / BType (handled based on context)
    public override OnlineJudgeOutput? VisitType(TypeSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitType(node, list);

        // Don't output <BType> for main function's return type
        if (Peek(1) is FunctionDeclarationSyntax func)
        {
            if (!IsMain(func))
                list?.AddNode(VirtualNode.FuncType);
        }

        // Don't output <BType> at all based on requirements
        return list;
    }

    // FuncDef / MainFuncDef
    public override OnlineJudgeOutput? VisitFunctionDeclaration(FunctionDeclarationSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitFunctionDeclaration(node, list);

        if (IsMain(node))
            list?.AddNode(VirtualNode.MainFuncDef);
        else
            list?.AddNode(VirtualNode.FuncDef);

        return list;
    }

    // FuncFParams
    public override OnlineJudgeOutput? VisitParameterList(ParameterListSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitParameterList(node, list);

        if (node.Items.Count > 0)
            list?.AddNode(VirtualNode.FuncFParams);

        return list;
    }

    // FuncFParam
    public override OnlineJudgeOutput? VisitParameterItem(ParameterItemSyntax node, OnlineJudgeOutput? list)
    {
        // The OJ requires that FuncFParam appears before comma token
        // So we need to override this method instead of relying on base traversal

        list = node.Type.Accept(this, list);
        list = node.Identifier.Accept(this, list);

        list = node.ArrayDimensions?.Accept(this, list);

        list?.AddNode(VirtualNode.FuncFParam);

        list = node.CommaToken?.Accept(this, list);

        return list;
    }

    // Stmt
    public override OnlineJudgeOutput? VisitExpressionStatement(ExpressionStatementSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitExpressionStatement(node, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    public override OnlineJudgeOutput? VisitBlockStatement(BlockStatementSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitBlockStatement(node, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    public override OnlineJudgeOutput? VisitIfStatement(IfStatementSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitIfStatement(node, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    public override OnlineJudgeOutput? VisitWhileStatement(WhileStatementSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitWhileStatement(node, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    public override OnlineJudgeOutput? VisitReturnStatement(ReturnStatementSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitReturnStatement(node, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    public override OnlineJudgeOutput? VisitControlFlowStatement(ControlFlowStatementSyntax n, OnlineJudgeOutput? list)
    {
        list = base.VisitControlFlowStatement(n, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    public override OnlineJudgeOutput? VisitEmptyStatement(EmptyStatementSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitEmptyStatement(node, list);

        list?.AddNode(VirtualNode.Stmt);
        return list;
    }

    // UnaryOp
    public override OnlineJudgeOutput? VisitUnaryOperator(UnaryOperatorSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitUnaryOperator(node, list);

        list?.AddNode(VirtualNode.UnaryOp);
        return list;
    }

    private bool IsInConstContext()
    {
        var varDecl = GetClosest<VariableDeclarationSyntax>();
        return varDecl?.Modifier?.Token.TokenKind is TokenKind.Const;
    }

    // InitVal / ConstInitVal
    public override OnlineJudgeOutput? VisitExpressionInitializer(ExpressionInitializerSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitExpressionInitializer(node, list);

        if (IsInConstContext())
            list?.AddNode(VirtualNode.ConstInitVal);
        else
            list?.AddNode(VirtualNode.InitVal);

        return list;
    }

    public override OnlineJudgeOutput? VisitArrayInitializer(ArrayInitializerSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitArrayInitializer(node, list);

        if (IsInConstContext())
            list?.AddNode(VirtualNode.ConstInitVal);
        else
            list?.AddNode(VirtualNode.InitVal);

        return list;
    }

    public override OnlineJudgeOutput? VisitArgumentList(ArgumentListSyntax node, OnlineJudgeOutput? list)
    {
        list = base.VisitArgumentList(node, list);

        if (node.Items.Count > 0
            && GetClosest<FunctionCallExpressionSyntax>()
                is FunctionCallExpressionSyntax funcCallExpr
            && !IsGetintOrPrintf(funcCallExpr))
            list?.AddNode(VirtualNode.FuncRParams);

        return list;
    }
}
