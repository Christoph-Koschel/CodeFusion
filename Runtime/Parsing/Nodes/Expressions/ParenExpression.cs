using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public class ParenExpression : Expression
{
    public override SyntaxType type => SyntaxType.ParenExpression;
    public readonly Token openToken;
    public readonly Expression expression;
    public readonly Token closeToken;

    public ParenExpression(SyntaxTree syntaxTree, Token openToken, Expression expression, Token closeToken) :
        base(syntaxTree)
    {
        this.openToken = openToken;
        this.expression = expression;
        this.closeToken = closeToken;
    }
}