using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding.Nodes.Expressions;

internal sealed class BoundVariableExpression : BoundExpression
{
    public readonly VariableSymbol variableSymbol;

    public BoundVariableExpression(VariableSymbol variableSymbol)
    {
        this.variableSymbol = variableSymbol;
    }

    public override BoundNodeType boundType => BoundNodeType.VariableExpression;
    public override TypeSymbol type => variableSymbol.type;
}