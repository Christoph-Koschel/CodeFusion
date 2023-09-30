using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public sealed class BinaryExpression : Expression
{
    public BinaryExpression(SyntaxTree syntaxTree, Expression left, Token operatorToken, Expression right) :
        base(syntaxTree)
    {
        this.left = left;
        this.operatorToken = operatorToken;
        this.right = right;
    }

    public override SyntaxType type => SyntaxType.BinaryExpression;
    public readonly Expression left;
    public readonly Token operatorToken;
    public readonly Expression right;
}