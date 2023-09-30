using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public sealed class AssignmentExpression : Expression
{
    public override SyntaxType type => SyntaxType.AssignmentExpression;
    public readonly Token identifier;
    public readonly Node equalsToken;
    public readonly Expression expression;
        
    public AssignmentExpression(SyntaxTree syntaxTree, Token identifier, Node equalsToken, Expression expression)  : base(syntaxTree)
    {
        this.identifier = identifier;
        this.equalsToken = equalsToken;
        this.expression = expression;
    }
}