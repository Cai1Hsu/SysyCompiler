namespace SysyCompiler.Analyzer.Semantic;

public sealed class PrimitiveType : ScalarType
{
    public PrimitiveTypeKind Kind { get; }

    public PrimitiveType(PrimitiveTypeKind kind)
    {
        Kind = kind;
    }

    public override bool Equals(object? obj)
        => obj is PrimitiveType other && other.Kind == Kind;

    public override int GetHashCode() => HashCode.Combine(Kind);
}
