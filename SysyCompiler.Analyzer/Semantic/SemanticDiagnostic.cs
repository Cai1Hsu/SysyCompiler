using System.Collections;
using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer.Semantic;

/// <summary>
/// Enumerates all semantic (and related) error kinds that the analyzer can produce.
/// </summary>
public enum SemanticErrorKind
{
    IllegalFormatStringCharacter,
    IdentifierRedefined,
    IdentifierUndefined,
    FunctionArgumentCountMismatch,
    FunctionArgumentTypeMismatch,
    VoidFunctionWithReturnValue,
    ValueFunctionMissingReturn,
    AssignmentToConstant,
    MissingSemicolon,
    MissingRightParenthesis,
    MissingRightBracket,
    PrintfArgumentCountMismatch,
    MisplacedLoopControl,
}

/// <summary>
/// Represents a single diagnostic produced during semantic analysis.
/// </summary>
/// <param name="Kind">The semantic error classification.</param>
/// <param name="Token">The syntax token that triggered the diagnostic.</param>
public readonly record struct SemanticDiagnostic(SemanticErrorKind Kind, SyntaxToken Token);

/// <summary>
/// Provides helpers to translate semantic error kinds to external representations.
/// </summary>
public static class SemanticErrorKindExtensions
{
    /// <summary>
    /// Translates the semantic error kind into the required single-letter category code.
    /// </summary>
    public static char ToCategoryCode(this SemanticErrorKind kind) => kind switch
    {
        SemanticErrorKind.IllegalFormatStringCharacter => 'a',
        SemanticErrorKind.IdentifierRedefined => 'b',
        SemanticErrorKind.IdentifierUndefined => 'c',
        SemanticErrorKind.FunctionArgumentCountMismatch => 'd',
        SemanticErrorKind.FunctionArgumentTypeMismatch => 'e',
        SemanticErrorKind.VoidFunctionWithReturnValue => 'f',
        SemanticErrorKind.ValueFunctionMissingReturn => 'g',
        SemanticErrorKind.AssignmentToConstant => 'h',
        SemanticErrorKind.MissingSemicolon => 'i',
        SemanticErrorKind.MissingRightParenthesis => 'j',
        SemanticErrorKind.MissingRightBracket => 'k',
        SemanticErrorKind.PrintfArgumentCountMismatch => 'l',
        SemanticErrorKind.MisplacedLoopControl => 'm',
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown semantic error kind."),
    };
}

/// <summary>
/// Collects diagnostics during a single pass of semantic analysis.
/// </summary>
public sealed class SemanticDiagnosticCollector : IEnumerable<SemanticDiagnostic>
{
    private readonly List<SemanticDiagnostic> diagnostics = new();

    /// <summary>
    /// Gets the diagnostics accumulated so far.
    /// </summary>
    public IReadOnlyList<SemanticDiagnostic> Diagnostics => diagnostics;

    /// <summary>
    /// Indicates whether any diagnostics have been reported.
    /// </summary>
    public bool Any => diagnostics.Count > 0;

    /// <summary>
    /// Adds a new diagnostic entry.
    /// </summary>
    public void Report(SemanticErrorKind kind, SyntaxToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        diagnostics.Add(new SemanticDiagnostic(kind, token));
    }

    /// <summary>
    /// Adds a pre-created diagnostic entry.
    /// </summary>
    public void Add(SemanticDiagnostic diagnostic)
    {
        diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Adds a list of diagnostics to the collection.
    /// </summary>
    public void AddRange(IEnumerable<SemanticDiagnostic> items)
    {
        diagnostics.AddRange(items);
    }

    public IEnumerator<SemanticDiagnostic> GetEnumerator() => diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
