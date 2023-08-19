namespace CodeFusion.ASM.Lexing;

public struct Token
{
    public readonly TokenType type;
    public readonly string text;
    public readonly Span span;

    public Token(TokenType type, string text, Span span)
    {
        this.type = type;
        this.text = text;
        this.span = span;
    }
}