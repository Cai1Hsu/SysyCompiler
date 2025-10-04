using System.Text;

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

    public override string ToString()
    {
        var sb = new StringBuilder(ElementType.ToString());

        for (var i = 0; i < ArrayRank; i++)
            sb.Append("[]");

        return sb.ToString();
    }

    public override bool Equals(object? obj)
        => obj is ArrayType other && ElementType.Equals(other.ElementType) && ArrayRank == other.ArrayRank;

    public override int GetHashCode()
        => HashCode.Combine(ElementType, ArrayRank);
}