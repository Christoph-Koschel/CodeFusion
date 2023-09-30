using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public sealed class LiteralExpression : Expression
{
    public override SyntaxType type => SyntaxType.LiteralExpression;
    public readonly Token literalToken;
    public readonly object value;

    public LiteralExpression(SyntaxTree syntaxTree, Token literalToken, object value)  : base(syntaxTree)
    {
        this.literalToken = literalToken;
        this.value = value;
    }

    public LiteralExpression(SyntaxTree syntaxTree, Token literalToken) 
        : this(syntaxTree, literalToken, literalToken.value)
    {
    }
}