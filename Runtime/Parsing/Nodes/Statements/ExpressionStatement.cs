namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public sealed class ExpressionStatement : Statement
{
    public readonly Expression expression;

    public ExpressionStatement(SyntaxTree syntaxTree, Expression expression) : base(syntaxTree)
    {
        this.expression = expression;
    }

    public override SyntaxType type => SyntaxType.ExpressionStatement;
    public override SyntaxType endToken => SyntaxType.SemicolonToken;
}