namespace IllusionScript.Runtime.Diagnostics;

public sealed class TextLine
{
    public readonly SourceText text;
    public readonly int start;
    public readonly int length;
    public readonly int lengthIncludingLineBreak;
    public int end => start + length;
    public TextSpan span => new TextSpan(start, length);
    public TextSpan spanIncludingLineBreak => new TextSpan(start, lengthIncludingLineBreak);

    public TextLine(SourceText text, int start, int length, int lengthIncludingLineBreak)
    {
        this.text = text;
        this.start = start;
        this.length = length;
        this.lengthIncludingLineBreak = lengthIncludingLineBreak;
    }
        
    public override string ToString() => text.ToString();
}