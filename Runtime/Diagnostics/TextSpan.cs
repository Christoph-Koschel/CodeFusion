namespace IllusionScript.Runtime.Diagnostics;

public readonly struct TextSpan
{
    public readonly int start;
    public readonly int length;
    public readonly int end => start + length;

    public TextSpan(int start, int length)
    {
        this.start = start;
        this.length = length;
    }


    public static TextSpan FromBounds(int start, int end)
    {
        int length = end - start;
        return new TextSpan(start, length);
    }

    public override string ToString() => $"{start}..{end}";
}