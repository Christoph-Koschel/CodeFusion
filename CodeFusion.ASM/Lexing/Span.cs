namespace CodeFusion.ASM.Lexing;

public struct Span
{
    public readonly int start;
    public readonly int end;
    public readonly int length => end - start;

    public Span(int start, int end)
    {
        this.start = start;
        this.end = end;
    }
}
