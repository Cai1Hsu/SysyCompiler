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
        AnalyzeFunctionDeclaration += (i, a, d) => IdentifierRedefinedAnalysis(i.Identifier, a, d);
        AnalyzeVariableDefine += (i, a, d) => IdentifierRedefinedAnalysis(i.Item1.Identifier, a, d);

        // rule 'c'
        AnalyzeReferenceUsage += UndefinedReferenceAnalysis;

        // rule 'f'
        AnalyzeFunctionDeclaration += VoidFunctionReturnValueAnalysis;

        // rule 'g'
        AnalyzeFunctionDeclaration += ValueFunctionMissingReturnAnalysis;

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
        AnalyzeFunctionDeclaration += (i, a, d) => ParenthesisCloseAnalysis(i.GetTokens().Last(), i.CloseParenToken, a, d);

        // rule 'l'
        AnalyzeFunctionCall += PrintfArgumentAnalysis;

        // rule 'm'
        AnalyzeControlFlowStatement += ControlFlowStatementNotInLoopAnalysis;
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

    public static void IdentifierRedefinedAnalysis(SyntaxToken identifier, ISymbolAnalyzer analyzer, SemanticDiagnosticCollector diagnostics)
    {
        if (analyzer.ResolveLocal(identifier.Text) is not null)
        {
            diagnostics.Add(new SemanticDiagnostic(SemanticErrorKind.IdentifierRedefined, identifier));
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

        string? text = formatString.Token.RawText;

        // Collect count of arguments

        int formatCount = 0;

        for (int i = 0; i < text?.Length; i++)
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
}
