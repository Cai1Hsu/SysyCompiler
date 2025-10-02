namespace SysyCompiler.Frontend;

public readonly struct TextSpan
{
    public int Start { get; }
    public int End { get; }

    public int Length => End - Start;

    public bool IsEmpty => Length == 0;

    public TextSpan(int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(end);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, end);

        Start = start;
        End = end;
    }

    public TextLocation? GetLocation(string text)
    {
        if (Start > text.Length || End > text.Length)
            return null;

        int line = 1;
        int column = 1;

        for (int i = Start; i < End; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return new TextLocation(line, column);
    }
}
