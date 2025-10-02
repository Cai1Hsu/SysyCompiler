namespace SysyCompiler.Frontend;

public readonly struct TextLocation
{
    public int Line { get; }
    public int Column { get; }

    public TextLocation(int line, int column)
    {
        Line = line;
        Column = column;
    }
}