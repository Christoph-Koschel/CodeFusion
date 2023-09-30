using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding.Nodes.Expressions;

internal sealed class BoundErrorExpression : BoundExpression
{
    public override BoundNodeType boundType => BoundNodeType.ErrorExpression;
    public override TypeSymbol type => TypeSymbol.error;
}