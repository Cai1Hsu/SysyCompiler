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
        => obj is FunctionType other
        && other.ReturnType.Equals(ReturnType)
        && other.ParameterTypes.SequenceEqual(ParameterTypes);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ReturnType);
        foreach (var paramType in ParameterTypes)
            hash.Add(paramType);
        return hash.ToHashCode();
    }
}