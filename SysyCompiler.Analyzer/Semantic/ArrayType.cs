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
}