using System.Diagnostics;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer.Semantic;

public class SymbolAnalyzer<T> : StackSyntaxVisitor<T>, ISymbolAnalyzer
{
    public delegate void AnalyzeHandler<TValue>(TValue info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics);

    public SemanticDiagnosticCollector Diagnostics { get; }

    Stack<SymbolScope> ISymbolAnalyzer.Scopes => scopes;

    private readonly Stack<SymbolScope> scopes = new();

    public SymbolAnalyzer(SemanticDiagnosticCollector diagnostics)
    {
        Diagnostics = diagnostics;
    }

    public override T? VisitCompilationUnit(CompilationUnitSyntax node, T? val = default)
    {
        // global scope
        using (this.CreateScope())
        {
            return base.VisitCompilationUnit(node, val);
        }
    }

    public override T? VisitBlock(BlockSyntax node, T? val = default)
    {
        val = node.OpenBraceToken.Accept(this);

        using (this.CreateScope())
        {
            AnalyzeBlock?.Invoke(node, this, Diagnostics);

            val = node.Statements.Accept(this, val);
        }

        return node.CloseBraceToken.Accept(this, val);
    }

    public override T? VisitFunctionDeclaration(FunctionDeclarationSyntax node, T? val = default)
    {
        AnalyzeFunctionDeclaration?.Invoke(node, this, Diagnostics);

        return base.VisitFunctionDeclaration(node, val);
    }

    public override T? VisitVariableDeclaration(VariableDeclarationSyntax node, T? val = default)
    {
        AnalyzeVariableDeclaration?.Invoke(node, this, Diagnostics);

        return base.VisitVariableDeclaration(node, val);
    }

    public override T? VisitVariableDefine(VariableDefineSyntax node, T? val = default)
    {
        var declaration = this.Peek(1) as VariableDeclarationSyntax // single define
            ?? this.Peek(2) as VariableDeclarationSyntax; // list define

        if (declaration is VariableDeclarationSyntax variableDeclaration)
        {
            AnalyzeVariableDefine?.Invoke((node, variableDeclaration), this, Diagnostics);
        }

        return base.VisitVariableDefine(node, val);
    }

    public override T? VisitReferenceExpression(ReferenceExpressionSyntax node, T? val = default)
    {
        AnalyzeReferenceUsage?.Invoke(node, this, Diagnostics);
        return base.VisitReferenceExpression(node, val);
    }

    public override T? VisitFunctionCallExpression(FunctionCallExpressionSyntax node, T? val = default)
    {
        AnalyzeFunctionCall?.Invoke(node, this, Diagnostics);

        return base.VisitFunctionCallExpression(node, val);
    }

    public override T? VisitControlFlowStatement(ControlFlowStatementSyntax node, T? val = default)
    {
        AnalyzeControlFlowStatement?.Invoke(node, this, Diagnostics);

        return base.VisitControlFlowStatement(node, val);
    }

    public override T? VisitLiteralExpression(LiteralExpressionSyntax node, T? val = default)
    {
        AnalyzeLiteralExpression?.Invoke(node, this, Diagnostics);

        return base.VisitLiteralExpression(node, val);
    }

    public override T? VisitBinaryExpression(BinaryExpressionSyntax node, T? val = default)
    {
        AnalyzeBinaryExpression?.Invoke(node, this, Diagnostics);

        return base.VisitBinaryExpression(node, val);
    }

    public override T? VisitReturnStatement(ReturnStatementSyntax node, T? val = default)
    {
        AnalyzeReturnStatement?.Invoke(node, this, Diagnostics);

        return base.VisitReturnStatement(node, val);
    }

    public override T? VisitArrayDimension(ArrayDimensionSyntax node, T? val = default)
    {
        AnalyzeArrayDimension?.Invoke(node, this, Diagnostics);

        return base.VisitArrayDimension(node, val);
    }

    public override T? VisitIfStatement(IfStatementSyntax node, T? val = default)
    {
        AnalyzeIfStatement?.Invoke(node, this, Diagnostics);

        return base.VisitIfStatement(node, val);
    }

    public override T? VisitWhileStatement(WhileStatementSyntax node, T? val = default)
    {
        AnalyzeWhileStatement?.Invoke(node, this, Diagnostics);

        return base.VisitWhileStatement(node, val);
    }

    public override T? VisitExpressionStatement(ExpressionStatementSyntax node, T? val = default)
    {
        AnalyzeExpressionStatement?.Invoke(node, this, Diagnostics);

        return base.VisitExpressionStatement(node, val);
    }

    public event AnalyzeHandler<BlockSyntax>? AnalyzeBlock;

    public event AnalyzeHandler<FunctionDeclarationSyntax>? AnalyzeFunctionDeclaration;

    public event AnalyzeHandler<VariableDeclarationSyntax>? AnalyzeVariableDeclaration;

    public event AnalyzeHandler<(VariableDefineSyntax, VariableDeclarationSyntax)>? AnalyzeVariableDefine;

    public event AnalyzeHandler<ReferenceExpressionSyntax>? AnalyzeReferenceUsage;

    public event AnalyzeHandler<FunctionCallExpressionSyntax>? AnalyzeFunctionCall;

    public event AnalyzeHandler<ControlFlowStatementSyntax>? AnalyzeControlFlowStatement;

    public event AnalyzeHandler<LiteralExpressionSyntax>? AnalyzeLiteralExpression;

    public event AnalyzeHandler<BinaryExpressionSyntax>? AnalyzeBinaryExpression;

    public event AnalyzeHandler<ReturnStatementSyntax>? AnalyzeReturnStatement;

    public event AnalyzeHandler<ArrayDimensionSyntax>? AnalyzeArrayDimension;

    public event AnalyzeHandler<IfStatementSyntax>? AnalyzeIfStatement;

    public event AnalyzeHandler<WhileStatementSyntax>? AnalyzeWhileStatement;

    public event AnalyzeHandler<ExpressionStatementSyntax>? AnalyzeExpressionStatement;
}
