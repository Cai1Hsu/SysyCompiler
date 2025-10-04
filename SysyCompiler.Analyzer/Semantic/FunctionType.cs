using System.Collections.Immutable;

namespace SysyCompiler.Analyzer.Semantic;

public class FunctionType : ScalarType
{
    public ISymbolType ReturnType { get; }

    public ImmutableArray<ISymbolType> ParameterTypes { get; }

    public FunctionType(ISymbolType returnType, ImmutableArray<ISymbolType>? parameterTypes = null)
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes ?? ImmutableArray<ISymbolType>.Empty;
    }

    public override bool Equals(object? obj)
        => obj is FunctionType other && other.ReturnType.Equals(ReturnType);

    public override int GetHashCode() => HashCode.Combine(ReturnType);
}