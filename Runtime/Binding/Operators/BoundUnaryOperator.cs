using IllusionScript.Runtime.Memory.Symbols;
using IllusionScript.Runtime.Parsing;

namespace IllusionScript.Runtime.Binding.Operators;

internal sealed class BoundUnaryOperator
{
    public SyntaxType type;
    public BoundUnaryOperatorType operatorType;
    public TypeSymbol rightType;
    public TypeSymbol resultType;

    private BoundUnaryOperator(SyntaxType type, BoundUnaryOperatorType operatorType, TypeSymbol rightType,
        TypeSymbol resultType)
    {
        this.type = type;
        this.operatorType = operatorType;
        this.rightType = rightType;
        this.resultType = resultType;
    }

    private BoundUnaryOperator(SyntaxType type, BoundUnaryOperatorType operatorType, TypeSymbol rightType)
        : this(type, operatorType, rightType, rightType)
    {
    }

    private static readonly BoundUnaryOperator[] operators =
    {
        new(SyntaxType.TildeToken, BoundUnaryOperatorType.OnesComplement, TypeSymbol.i64),
        new(SyntaxType.TildeToken, BoundUnaryOperatorType.OnesComplement, TypeSymbol.u64),

        new(SyntaxType.BangToken, BoundUnaryOperatorType.LogicalNegation, TypeSymbol.boolean),

        new(SyntaxType.PlusToken, BoundUnaryOperatorType.Identity, TypeSymbol.i64),
        new(SyntaxType.PlusToken, BoundUnaryOperatorType.Identity, TypeSymbol.u64),
        new(SyntaxType.PlusToken, BoundUnaryOperatorType.Identity, TypeSymbol.f64),

        new(SyntaxType.MinusToken, BoundUnaryOperatorType.Negation, TypeSymbol.i64),
        new(SyntaxType.MinusToken, BoundUnaryOperatorType.Negation, TypeSymbol.u64),
        new(SyntaxType.MinusToken, BoundUnaryOperatorType.Negation, TypeSymbol.f64)
    };

    public static BoundUnaryOperator Bind(SyntaxType type, TypeSymbol rightType)
    {
        foreach (BoundUnaryOperator unaryOperator in operators)
        {
            if (unaryOperator.type == type && unaryOperator.rightType == rightType)
            {
                return unaryOperator;
            }
        }

        return null;
    }
}