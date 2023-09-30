using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public class UnaryExpression : Expression
{
    public override SyntaxType type => SyntaxType.UnaryExpression;
    public readonly Token operatorToken;
    public readonly Expression right;

    public UnaryExpression(SyntaxTree syntaxTree, Token operatorToken, Expression right) : base(syntaxTree)
    {
        this.operatorToken = operatorToken;
        this.right = right;
    }
}