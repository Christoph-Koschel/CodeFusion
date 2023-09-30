using System.Collections.Immutable;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding.Nodes.Expressions;

internal sealed class BoundAssignmentExpression : BoundExpression
{
    public readonly VariableSymbol variableSymbol;
    public readonly BoundExpression expression;

    public BoundAssignmentExpression(VariableSymbol variableSymbol, BoundExpression expression)
    {
        this.variableSymbol = variableSymbol;
        this.expression = expression;
    }

    public override BoundNodeType boundType => BoundNodeType.AssignmentExpression;
    public override TypeSymbol type => expression.type;
}

internal sealed class BoundCallExpression : BoundExpression
{
    public readonly FunctionSymbol function;
    public readonly ImmutableArray<BoundExpression> arguments;

    public BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
    {
        this.function = function;
        this.arguments = arguments;
    }

    public override BoundNodeType boundType => BoundNodeType.CallExpression;
    public override TypeSymbol type => function.returnType;
}