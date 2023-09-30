using IllusionScript.Runtime.Binding.Nodes.Expressions;

namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal sealed class BoundIfStatement : BoundStatement
{
    public readonly BoundExpression condition;
    public readonly BoundStatement body;
    public readonly BoundStatement elseBody;

    public BoundIfStatement(BoundExpression condition, BoundStatement body, BoundStatement elseBody)
    {
        this.condition = condition;
        this.body = body;
        this.elseBody = elseBody;
    }

    public override BoundNodeType boundType => BoundNodeType.IfStatement;
}