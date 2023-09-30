using System;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding.Nodes.Expressions;

internal sealed class BoundLiteralExpression : BoundExpression
{
    public readonly object value;

    public BoundLiteralExpression(object value)
    {
        if (value is int)
        {
            type = TypeSymbol.i64;
        }
        else if (value is bool)
        {
            type = TypeSymbol.boolean;
        }
        else if (value is string)
        {
            type = TypeSymbol.@string;
        }
        else
        {
            throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}");
        }
            
        this.value = value;
    }

    public override BoundNodeType boundType => BoundNodeType.LiteralExpression;
    public override TypeSymbol type { get; }
}