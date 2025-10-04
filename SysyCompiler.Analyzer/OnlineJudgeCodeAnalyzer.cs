using System.Diagnostics;
using System.Collections.Immutable;
using SysyCompiler.Analyzer.Semantic;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Analyzer;

public class OnlineJudgeCodeAnalyzer : SymbolAnalyzer<object>
{
    public OnlineJudgeCodeAnalyzer(SemanticDiagnosticCollector diagnostics)
        : base(diagnostics)
    {
        // rule 'a'
        AnalyzeLiteralExpression += FormatStringAnalysis;

        // rule 'b'
        AnalyzeFunctionDeclaration += (i, a, d) => DeclareSymbol(CreateFunctionSymbol(i), a, d);
        AnalyzeVariableDefine += (i, a, d) => DeclareSymbol(CreateVariableSymbol(i.Item1, i.Item2), a, d);

        // rule 'c'
        AnalyzeReferenceUsage += UndefinedReferenceAnalysis;

        // rule 'd'
        AnalyzeFunctionCall += ArgumentCountAnalysis;

        // rule 'e'
        AnalyzeFunctionCall += ArgumentTypeAnalysis;

        // rule 'f'
        AnalyzeFunctionDeclaration += VoidFunctionReturnValueAnalysis;

        // rule 'g'
        AnalyzeFunctionDeclaration += ValueFunctionMissingReturnAnalysis;

        // rule 'h'
        AnalyzeReferenceUsage += ConstantModificationAnalysis;

        // rule 'i'
        AnalyzeVariableDeclaration += (i, a, d) => MissingSemicolonAnalysis(i.GetTokens().Last(), i.SemicolonToken, a, d);
        AnalyzeControlFlowStatement += (i, a, d) => MissingSemicolonAnalysis(i.ControlFlowToken, i.SemicolonToken, a, d);
        AnalyzeReturnStatement += (i, a, d) => MissingSemicolonAnalysis(i.ReturnKeyword, i.SemicolonToken, a, d);
        AnalyzeExpressionStatement += (i, a, d) => MissingSemicolonAnalysis(i.GetTokens().Last(), i.SemicolonToken, a, d);

        // rule 'j'
        AnalyzeFunctionCall += (i, a, d) => ParenthesisCloseAnalysis(i.GetTokens().Last(), i.CloseParenToken, a, d);
        AnalyzeFunctionDeclaration += (i, a, d) => ParenthesisCloseAnalysis(i.ParameterList?.GetTokens().LastOrDefault()
            ?? i.OpenParenToken, i.CloseParenToken, a, d);
        AnalyzeIfStatement += (i, a, d) => ParenthesisCloseAnalysis(i.Condition.GetTokens().LastOrDefault()
            ?? i.OpenParenToken, i.CloseParenToken, a, d);
        AnalyzeWhileStatement += (i, a, d) => ParenthesisCloseAnalysis(i.Condition.GetTokens().LastOrDefault()
            ?? i.OpenParenToken, i.CloseParenToken, a, d);

        // rule 'k'
        AnalyzeArrayDimension += (i, a, d) => BracketCloseAnalysis(i.GetTokens().Last(), i.CloseBracketToken, a, d);

        // rule 'l'
        AnalyzeFunctionCall += PrintfArgumentAnalysis;

        // rule 'm'
        AnalyzeControlFlowStatement += ControlFlowStatementNotInLoopAnalysis;

        // Register function parameters to local scope
        AnalyzeBlock += RegisterFunctionParameters;
    }

    public static VariableSymbol CreateVariableSymbol(VariableDefineSyntax define, VariableDeclarationSyntax declaration)
    {
        var arrayRank = define.ArrayDimensions?.Count ?? 0;
        var type = ISymbolTypeExtensions.Parse(declaration.Type, arrayRank);

        Debug.Assert(type is not null, "Type should be valid here");

        return new VariableSymbol(define.Identifier, define, type, declaration.Modifier);
    }

    public static FunctionSymbol CreateFunctionSymbol(FunctionDeclarationSyntax declaration)
    {
        var parameterTypes = ImmutableArray.CreateBuilder<ISymbolType>();

        var parameterList = declaration.ParameterList?.Items;
        if (parameterList is not null)
        {
            foreach (var parameter in parameterList)
            {
                var arrayRank = parameter.ArrayDimensions?.Count ?? 0;
                var parameterType = ISymbolTypeExtensions.Parse(parameter.Type, arrayRank);

                Debug.Assert(parameterType is not null, "Type should be valid here");

                parameterTypes.Add(parameterType);
            }
        }

        var type = ISymbolTypeExtensions.Parse(declaration, parameterTypes.ToImmutable());

        Debug.Assert(type is not null, "Type should be valid here");

        return new FunctionSymbol(declaration, type);
    }

    public static void FormatStringAnalysis(LiteralExpressionSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        // <FormatChar> -> %d
        // <NormalChar> -> 32, 33, 40 - 126, 92 | \n
        // <char> -> <FormatChar> | <NormalChar>
        // <FormatString> → '"' {<Char>} '"'

        string? text = info.Token.RawText;

        if (text is null || text.Length < 2) return;

        if (text[0] != '"' || text[^1] != '"') return;

        string content = text[1..^1];

        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '%')
            {
                if (i + 1 < content.Length && content[i + 1] == 'd')
                {
                    // Found a format specifier %d
                    // Handle the format specifier as needed
                    i++; // Skip the next character as it's part of the format specifier
                }
                else
                {
                    diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IllegalFormatStringCharacter, info.Token));
                    return;
                }
            }
            else if (content[i] == '\\')
            {
                if (i + 1 < content.Length && content[i + 1] == 'n')
                {
                    // Found an escape sequence \n
                    // Handle the escape sequence as needed
                    i++; // Skip the next character as it's part of the escape sequence
                }
                else
                {
                    diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IllegalFormatStringCharacter, info.Token));
                    return;
                }
            }
            else if (content[i] < 32 || content[i] > 126)
            {
                diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IllegalFormatStringCharacter, info.Token));
                return;
            }
        }
    }

    public static void DeclareSymbol(INamedSymbol symbol, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        // Required to be local scope
        if (analyzer.CurrentScope?.TryDeclare(symbol) is false)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IdentifierRedefined, symbol.Identifier));
        }
    }

    public static void UndefinedReferenceAnalysis(ReferenceExpressionSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (info.Identifier.Text is "getint" or "printf")
            return;

        if (analyzer.ResolveGlobal(info.Identifier.Text) is null)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IdentifierUndefined, info.Identifier));
        }
    }

    public static void ControlFlowStatementNotInLoopAnalysis(ControlFlowStatementSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (analyzer.GetClosest<WhileStatementSyntax>() is null)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.MisplacedLoopControl, info.ControlFlowToken));
        }
    }

    public void PrintfArgumentAnalysis(FunctionCallExpressionSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (info.Callee is not ReferenceExpressionSyntax directCall)
            return;

        if (directCall.Identifier.Text is not "printf")
            return;

        if (info.ArgumentList.Items.Count < 1)
            return;

        if (info.ArgumentList.Items[0] is not ArgumentItemSyntax { Expression: LiteralExpressionSyntax formatString })
            return;

        string text = formatString.Token.Text;

        // Collect count of arguments

        int formatCount = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '%')
            {
                if (i + 1 < text.Length && text[i + 1] == 'd')
                {
                    // Found a format specifier %d
                    // Handle the format specifier as needed
                    i++; // Skip the next character as it's part of the format specifier
                    formatCount++;
                }
            }
        }

        if (formatCount != info.ArgumentList.Items.Count - 1 /* exclude the format string*/)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.PrintfArgumentCountMismatch, directCall.Identifier));
        }
    }

    public static void ParenthesisCloseAnalysis(SyntaxToken nonTerminal, SyntaxToken? closeParenToken, ISymbolAnalyzer _, SemanticDiagnosticCollector diagnostics)
    {
        if (closeParenToken is null)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.MissingRightParenthesis, nonTerminal));
        }
    }

    public static void BracketCloseAnalysis(SyntaxToken nonTerminal, SyntaxToken? closeBracketToken, ISymbolAnalyzer _, SemanticDiagnosticCollector diagnostics)
    {
        if (closeBracketToken is null)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.MissingRightBracket, nonTerminal));
        }
    }

    public static void MissingSemicolonAnalysis(SyntaxToken nonTerminal, SyntaxToken? semicolonToken, ISymbolAnalyzer _, SemanticDiagnosticCollector diagnostics)
    {
        if (semicolonToken is null)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.MissingSemicolon, nonTerminal));
        }
    }

    public static void VoidFunctionReturnValueAnalysis(FunctionDeclarationSyntax info, ISymbolAnalyzer _, SemanticDiagnosticCollector diagnostics)
    {
        if (info.ReturnType.Token.TokenKind is not TokenKind.Void)
            return;

        var returns = info.Body.GetChildrenSubtree().OfType<ReturnStatementSyntax>();

        if (info.Body.GetChildrenSubtree()
            .OfType<ReturnStatementSyntax>()
            .Any(r => r.Expression is not null))
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.VoidFunctionWithReturnValue, info.Body.CloseBraceToken));
            return;
        }
    }

    public static void ValueFunctionMissingReturnAnalysis(FunctionDeclarationSyntax info, ISymbolAnalyzer _, SemanticDiagnosticCollector diagnostics)
    {
        if (info.ReturnType.Token.TokenKind is TokenKind.Void)
            return;

        var returns = info.Body.GetChildrenSubtree().OfType<ReturnStatementSyntax>();

        // This is a simple check
        if (!returns.Any())
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.ValueFunctionMissingReturn, info.Body.CloseBraceToken));
            return;
        }

        foreach (var returnStatement in returns)
        {
            if (returnStatement.Expression is null)
            {
                diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.ValueFunctionMissingReturn, info.Body.CloseBraceToken));
                return;
            }
        }
    }

    public static void ArgumentCountAnalysis(FunctionCallExpressionSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (info.Callee is not ReferenceExpressionSyntax directCall)
            return;

        if (analyzer.ResolveGlobal(directCall.Identifier.Text) is not FunctionSymbol functionSymbol)
            return;

        if (functionSymbol.Declaration.ParameterList?.Items.Count != info.ArgumentList.Items.Count)
        {
            diagnostics.Report(SemanticErrorKind.FunctionArgumentCountMismatch, info.OpenParenToken);
        }
    }

    public static void ArgumentTypeAnalysis(FunctionCallExpressionSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (info.Callee is not ReferenceExpressionSyntax directCall)
            return;

        if (analyzer.ResolveGlobal(directCall.Identifier.Text) is not FunctionSymbol functionSymbol)
            return;

        var parameters = functionSymbol.Type.ParameterTypes;
        var arguments = info.ArgumentList.Items;

        for (int i = 0; i < Math.Min(parameters.Length, arguments.Count); i++)
        {
            var parameter = parameters[i];
            var argument = arguments[i].Expression;

            if (ResolveExpressionType(argument, analyzer) is not ISymbolType argumentType)
                continue;

            if (!parameter.Equals(argumentType))
            {
                diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.FunctionArgumentTypeMismatch, directCall.Identifier));
            }
        }
    }

    public static ISymbolType? ResolveExpressionType(ExpressionSyntax expression, ISymbolAnalyzer analyzer)
    {
        switch (expression)
        {
            case BinaryExpressionSyntax binaryExpression:
                var leftType = ResolveExpressionType(binaryExpression.Left, analyzer);
                var rightType = ResolveExpressionType(binaryExpression.Right, analyzer);

                if (leftType is null || !leftType.Equals(rightType))
                    goto default; // type mismatch, resolve to null
                else
                    return leftType;

            case LiteralExpressionSyntax literalExpression:
                return ISymbolTypeExtensions.Parse(literalExpression);

            case GroupedExpressionSyntax groupedExpression:
                return ResolveExpressionType(groupedExpression.Expression, analyzer);

            case UnaryExpressionSyntax unaryExpression:
                return ResolveExpressionType(unaryExpression.Operand, analyzer);

            case FunctionCallExpressionSyntax functionCall:
                if (ResolveExpressionType(functionCall.Callee, analyzer) is not FunctionType functionType)
                    goto default; // not a function

                return functionType.ReturnType; // unbox the return type

            case ArrayExpressionSyntax arrayAccess:
                var baseType = ResolveExpressionType(arrayAccess.Base, analyzer);

                if (baseType is not ArrayType arrayType || arrayType.ArrayRank == 0)
                    goto default; // not a array

                if (arrayType.ArrayRank == 1)
                    return baseType; // unbox to element type
                else
                    // flatten the array access
                    return new ArrayType(arrayType.ElementType, arrayType.ArrayRank - 1);

            case ReferenceExpressionSyntax referenceExpression
                when referenceExpression.Identifier.Text is "getint":
                return new FunctionType(new PrimitiveType(PrimitiveTypeKind.Int));

            case ReferenceExpressionSyntax referenceExpression
                when referenceExpression.Identifier.Text is "printf":
                return new FunctionType(new PrimitiveType(PrimitiveTypeKind.Void));

            case ReferenceExpressionSyntax referenceExpression
                when analyzer.ResolveGlobal(referenceExpression.Identifier.Text) is VariableSymbol variableSymbol:
                return variableSymbol.Type;

            default:
                return null;
        }
    }

    public static void ConstantModificationAnalysis(ReferenceExpressionSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (analyzer.GetClosest<BinaryExpressionSyntax>(b => b.Operator.Token.TokenKind is TokenKind.AssignEqual)
            is not BinaryExpressionSyntax assignExpression)
            return;

        if (analyzer.ResolveGlobal(info.Identifier.Text) is not VariableSymbol variableSymbol || variableSymbol.Kind is not SymbolKind.Constant)
            return;

        bool onLeftSide = false;

        // We must exclude cases where this reference is a part of a function arguments or array indices
        foreach (var expr in assignExpression.Left.GetChildrenSubtree())
        {
            if (expr is FunctionCallExpressionSyntax functionCall && functionCall.ArgumentList.GetChildrenSubtree().Contains(info))
                return;

            if (expr is ArrayExpressionSyntax arrayAccess && arrayAccess.Index.GetChildrenSubtree().Contains(info))
                return;

            if (ReferenceEquals(info, expr))
            {
                onLeftSide = true;
                break;
            }
        }

        if (onLeftSide)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.AssignmentToConstant, info.Identifier));
        }
    }

    public static void RegisterFunctionParameters(BlockSyntax info, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        var function = analyzer.GetClosest<FunctionDeclarationSyntax>(searchLimit: 2);

        if (function is null)
            return;

        if (function.Body != info)
            return;

        if (function.ParameterList is null)
            return;

        foreach (var parameter in function.ParameterList.Items)
        {
            var arrayRank = parameter.ArrayDimensions?.Count ?? 0;
            var type = ISymbolTypeExtensions.Parse(parameter.Type, arrayRank);

            Debug.Assert(type is not null, "Type should be valid here");

            var symbol = new VariableSymbol(parameter.Identifier, parameter, type, null);

            if (analyzer.CurrentScope?.TryDeclare(symbol) is false)
            {
                // probably two parameters with the same name
                diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IdentifierRedefined, symbol.Identifier));
            }
        }
    }
}
