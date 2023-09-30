using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Parsing;
using IllusionScript.Runtime.Parsing.Nodes;

namespace IllusionScript.Runtime.Lexing;

public class Token : Node
{
    public readonly int position;
    public override TextSpan span => new (position, text?.Length ?? 0);
    public readonly string text;
    public readonly object value;
    public readonly bool isMissing;

    public Token(SyntaxTree syntaxTree, SyntaxType type, int position, string text, object value) : base(syntaxTree)
    {
        this.type = type;
        this.position = position;
        this.text = text;
        this.value = value;
        isMissing = text == null;
    }

    public override SyntaxType type { get; }
}