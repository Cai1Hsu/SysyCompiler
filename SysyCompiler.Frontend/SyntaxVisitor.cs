using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Frontend;

public abstract class SyntaxVisitor<T>
{
    public virtual T? Visit(SyntaxNode node, T? val = default)
    {
        return node switch
        {
            ArgumentItemSyntax n => VisitArgumentItem(n, val),
            ArgumentListSyntax n => VisitArgumentList(n, val),
            ArrayDimensionSyntax n => VisitArrayDimension(n, val),
            ArrayExpressionSyntax n => VisitArrayExpression(n, val),
            ArrayInitializerItemSyntax n => VisitArrayInitializerItem(n, val),
            ArrayInitializerSyntax n => VisitArrayInitializer(n, val),
            BinaryExpressionSyntax n => VisitBinaryExpression(n, val),
            BinaryOperatorSyntax n => VisitBinaryOperator(n, val),
            BlockStatementSyntax n => VisitBlockStatement(n, val),
            BlockSyntax n => VisitBlock(n, val),
            CompilationUnitSyntax n => VisitCompilationUnit(n, val),
            EmptyStatementSyntax n => VisitEmptyStatement(n, val),
            ExpressionInitializerSyntax n => VisitExpressionInitializer(n, val),
            ExpressionStatementSyntax n => VisitExpressionStatement(n, val),
            FunctionCallExpressionSyntax n => VisitFunctionCallExpression(n, val),
            FunctionDeclarationSyntax n => VisitFunctionDeclaration(n, val),
            GroupedExpressionSyntax n => VisitGroupedExpression(n, val),
            IfStatementSyntax n => VisitIfStatement(n, val),
            LiteralExpressionSyntax n => VisitLiteralExpression(n, val),
            LocalDeclarationStatementSyntax n => VisitLocalDeclarationStatement(n, val),
            ModifierSyntax n => VisitModifier(n, val),
            ParameterItemSyntax n => VisitParameterItem(n, val),
            ParameterListSyntax n => VisitParameterList(n, val),
            ReferenceExpressionSyntax n => VisitReferenceExpression(n, val),
            ReturnStatementSyntax n => VisitReturnStatement(n, val),
            ControlFlowStatementSyntax n => VisitControlFlowStatement(n, val),
            SyntaxList n => VisitSyntaxList(n, val),
            SyntaxToken n => VisitSyntaxToken(n, val),
            TypeSyntax n => VisitType(n, val),
            UnaryExpressionSyntax n => VisitUnaryExpression(n, val),
            UnaryOperatorSyntax n => VisitUnaryOperator(n, val),
            VariableDeclarationSyntax n => VisitVariableDeclaration(n, val),
            VariableDefineSyntax n => VisitVariableDefine(n, val),
            WhileStatementSyntax n => VisitWhileStatement(n, val),

            _ => throw new NotImplementedException($"No visit method for syntax node of kind {node.Kind}"),
        };
    }

    private T? TraversalMembers<TNode>(TNode node, T? val = default)
        where TNode : SyntaxNode
    {
        foreach (var member in node.GetMembers())
        {
            if (member.Value is SyntaxNode childNode)
            {
                childNode.Accept(this, val);
            }
            else if (member.Value is IEnumerable<SyntaxNode> nodeList)
            {
                foreach (var item in nodeList)
                {
                    item.Accept(this, val);
                }
            }
        }

        return val;
    }

    public virtual T? VisitArgumentItem(ArgumentItemSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitArgumentList(ArgumentListSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitArrayDimension(ArrayDimensionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitArrayExpression(ArrayExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitArrayInitializerItem(ArrayInitializerItemSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitArrayInitializer(ArrayInitializerSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitBinaryExpression(BinaryExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitBinaryOperator(BinaryOperatorSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitBlockStatement(BlockStatementSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitBlock(BlockSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitCompilationUnit(CompilationUnitSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitEmptyStatement(EmptyStatementSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitExpressionInitializer(ExpressionInitializerSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitExpressionStatement(ExpressionStatementSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitFunctionCallExpression(FunctionCallExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitFunctionDeclaration(FunctionDeclarationSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitGroupedExpression(GroupedExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitIfStatement(IfStatementSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitLiteralExpression(LiteralExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitModifier(ModifierSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitParameterItem(ParameterItemSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitParameterList(ParameterListSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitReferenceExpression(ReferenceExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitReturnStatement(ReturnStatementSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitControlFlowStatement(ControlFlowStatementSyntax n, T? val = default) => TraversalMembers(n, val);

    public virtual T? VisitSyntaxList(SyntaxList node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitSyntaxToken(SyntaxToken node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitType(TypeSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitUnaryExpression(UnaryExpressionSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitUnaryOperator(UnaryOperatorSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitVariableDeclaration(VariableDeclarationSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitVariableDefine(VariableDefineSyntax node, T? val = default) => TraversalMembers(node, val);

    public virtual T? VisitWhileStatement(WhileStatementSyntax node, T? val = default) => TraversalMembers(node, val);

}
