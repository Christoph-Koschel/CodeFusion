namespace IllusionScript.Runtime.Diagnostics;

public readonly struct TextLocation
{
    public readonly SourceText text;
    public readonly TextSpan span;
    public int startLine => text.GetLineIndex(span.start);
    public int endLine => text.GetLineIndex(span.end);
    public int startCharacter => span.start - text.lines[startLine].start;
    public int endCharacter => span.end - text.lines[endLine].start;

    public TextLocation(SourceText text, TextSpan span)
    {
        this.text = text;
        this.span = span;
    }
}