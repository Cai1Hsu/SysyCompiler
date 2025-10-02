namespace SysyCompiler.Frontend.Syntax;

public enum SyntaxKind
{
    SyntaxToken,
    SyntaxList,
    CompilationUnit,

    FunctionDeclaration,
    VariableDefine,
    VariableDeclaration,

    ParameterList,
    Parameter,
    ArgumentItem,
    ArgumentList,


    ArrayInitializer,
    ArrayInitializerItem,
    ExpressionInitializer,
    ArrayDimension,

    Type,

    Block,

    BlockStatement,
    IfStatement,
    WhileStatement,
    ReturnStatement,
    ExpressionStatement,
    LocalDeclarationStatement,
    EmptyStatement,

    BinaryOperator,
    UnaryOperator,

    ArrayExpression,
    BinaryExpression,
    UnaryExpression,
    LiteralExpression,
    ReferenceExpression,
    FunctionCallExpression,
    GroupedExpression,

    Modifier,
    ControlFlowStatement,
}

public static class SyntaxKindExtensions
{
    public static bool IsExpression(this SyntaxKind kind)
        => kind == SyntaxKind.BinaryExpression
        || kind == SyntaxKind.UnaryExpression
        || kind == SyntaxKind.LiteralExpression
        || kind == SyntaxKind.ReferenceExpression
        || kind == SyntaxKind.ArrayExpression
        || kind == SyntaxKind.FunctionCallExpression
        || kind == SyntaxKind.GroupedExpression;

    public static bool IsStatement(this SyntaxKind kind)
        => kind == SyntaxKind.BlockStatement
        || kind == SyntaxKind.IfStatement
        || kind == SyntaxKind.WhileStatement
        || kind == SyntaxKind.ReturnStatement
        || kind == SyntaxKind.ExpressionStatement
        || kind == SyntaxKind.LocalDeclarationStatement;

    public static bool IsMemberDeclaration(this SyntaxKind kind)
        => kind == SyntaxKind.FunctionDeclaration
        || kind == SyntaxKind.VariableDeclaration;

    public static bool IsInitializer(this SyntaxKind kind)
        => kind == SyntaxKind.ArrayInitializer
        || kind == SyntaxKind.ExpressionInitializer;

    public static bool IsOperator(this SyntaxKind kind)
        => kind == SyntaxKind.BinaryOperator
        || kind == SyntaxKind.UnaryOperator;
}