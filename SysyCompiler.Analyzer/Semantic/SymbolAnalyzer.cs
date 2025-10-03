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
        using (this.CreateScope())
        {
            return base.VisitBlock(node, val);
        }
    }

    public override T? VisitFunctionDeclaration(FunctionDeclarationSyntax node, T? val = default)
    {
        AnalyzeFunctionDeclaration?.Invoke(node, this, Diagnostics);

        return base.VisitFunctionDeclaration(node, val);
    }

    public override T? VisitVariableDefine(VariableDefineSyntax node, T? val = default)
    {
        var declaration = this.Peek(1) as VariableDeclarationSyntax // single define
            ?? this.Peek(2) as VariableDeclarationSyntax; // list define

        if (declaration is VariableDeclarationSyntax variableDeclaration)
        {
            AnalyzeVariableDeclaration?.Invoke((node, variableDeclaration), this, Diagnostics);
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
        return base.VisitArrayDimension(node, val);
    }

    public event AnalyzeHandler<FunctionDeclarationSyntax>? AnalyzeFunctionDeclaration;

    public event AnalyzeHandler<(VariableDefineSyntax, VariableDeclarationSyntax)>? AnalyzeVariableDeclaration;

    public event AnalyzeHandler<ReferenceExpressionSyntax>? AnalyzeReferenceUsage;

    public event AnalyzeHandler<FunctionCallExpressionSyntax>? AnalyzeFunctionCall;

    public event AnalyzeHandler<ControlFlowStatementSyntax>? AnalyzeControlFlowStatement;

    public event AnalyzeHandler<LiteralExpressionSyntax>? AnalyzeLiteralExpression;

    public event AnalyzeHandler<BinaryExpressionSyntax>? AnalyzeBinaryExpression;

    public event AnalyzeHandler<ReturnStatementSyntax>? AnalyzeReturnStatement;

    public event AnalyzeHandler<ArrayDimensionSyntax>? AnalyzeArrayDimension;
}
