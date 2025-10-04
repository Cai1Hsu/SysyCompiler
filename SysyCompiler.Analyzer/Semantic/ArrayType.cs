namespace SysyCompiler.Analyzer.Semantic;

public class ArrayType : ISymbolType
{
    public ScalarType ElementType { get; }

    public int ArrayRank { get; }

    public ArrayType(ScalarType elementType, int arrayRank)
    {
        ElementType = elementType;
        ArrayRank = arrayRank;
    }

    public override bool Equals(object? obj)
        => obj is ArrayType other && ElementType.Equals(other.ElementType) && ArrayRank == other.ArrayRank;

    public override int GetHashCode()
        => HashCode.Combine(ElementType, ArrayRank);
}