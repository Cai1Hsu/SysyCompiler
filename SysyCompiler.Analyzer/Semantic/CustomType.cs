using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer.Semantic;


public class CustomType(SyntaxToken identifier) : ScalarType
{
    public SyntaxToken Identifier { get; } = identifier;

    public override bool Equals(object? obj)
        => obj is CustomType other && other.Identifier.Text == Identifier.Text;

    public override int GetHashCode() => HashCode.Combine(Identifier.Text);
}
