using IllusionScript.Runtime.Binding.Nodes.Expressions;

namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public readonly BoundLabel boundLabel;
    public readonly BoundExpression condition;
    public readonly bool jmpIfTrue;

    public BoundConditionalGotoStatement(BoundLabel boundLabel, BoundExpression condition, bool jmpIfTrue = true)
    {
        this.boundLabel = boundLabel;
        this.condition = condition;
        this.jmpIfTrue = jmpIfTrue;
    }

    public override BoundNodeType boundType => BoundNodeType.ConditionalGotoStatement;
}