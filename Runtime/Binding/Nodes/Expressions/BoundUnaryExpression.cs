using IllusionScript.Runtime.Binding.Operators;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding.Nodes.Expressions;

internal sealed class BoundUnaryExpression : BoundExpression
{
    public readonly BoundUnaryOperator unaryOperator;
    public readonly BoundExpression right;

    public BoundUnaryExpression(BoundUnaryOperator unaryOperator, BoundExpression right)
    {
        this.unaryOperator = unaryOperator;
        this.right = right;
    }

    public override TypeSymbol type => unaryOperator.resultType;
    public override BoundNodeType boundType => BoundNodeType.UnaryExpression;
}