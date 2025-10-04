namespace SysyCompiler.Analyzer.Semantic;

public class FunctionType : ScalarType
{
    public ISymbolType ReturnType { get; }

    public FunctionType(ISymbolType returnType)
    {
        ReturnType = returnType;
    }

    public override bool Equals(object? obj)
        => obj is FunctionType other && other.ReturnType.Equals(ReturnType);

    public override int GetHashCode() => HashCode.Combine(ReturnType);
}