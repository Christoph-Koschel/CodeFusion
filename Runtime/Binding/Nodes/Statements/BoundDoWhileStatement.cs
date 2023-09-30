using IllusionScript.Runtime.Binding.Nodes.Expressions;

namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal sealed class BoundDoWhileStatement : BoundLoopStatement
{
    public readonly BoundStatement body;
    public readonly BoundExpression condition;

    public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, BoundLabel breakLabel,
        BoundLabel continueLabel) : base(breakLabel, continueLabel)
    {
        this.body = body;
        this.condition = condition;
    }

    public override BoundNodeType boundType => BoundNodeType.DoWhileStatement;
}