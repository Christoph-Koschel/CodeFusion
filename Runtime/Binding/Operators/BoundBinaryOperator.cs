using IllusionScript.Runtime.Memory.Symbols;
using IllusionScript.Runtime.Parsing;

namespace IllusionScript.Runtime.Binding.Operators;

internal sealed class BoundBinaryOperator
{
    public SyntaxType type;
    public BoundBinaryOperatorType operatorType;
    public TypeSymbol leftType;
    public TypeSymbol rightType;
    public TypeSymbol resultType;

    private BoundBinaryOperator(SyntaxType type, BoundBinaryOperatorType operatorType, TypeSymbol leftType,
        TypeSymbol rightType, TypeSymbol resultType)
    {
        this.type = type;
        this.operatorType = operatorType;
        this.leftType = leftType;
        this.rightType = rightType;
        this.resultType = resultType;
    }

    private BoundBinaryOperator(SyntaxType syntaxType, BoundBinaryOperatorType operatorType, TypeSymbol type)
        : this(syntaxType, operatorType, type, type, type)
    {
    }

    private BoundBinaryOperator(SyntaxType syntaxType, BoundBinaryOperatorType operatorType, TypeSymbol type, TypeSymbol result)
        : this(syntaxType, operatorType, type, type, result)
    {
    }

    private static readonly BoundBinaryOperator[] operators =
    {
        new(SyntaxType.PlusToken, BoundBinaryOperatorType.Addition, TypeSymbol.i64),
        new(SyntaxType.MinusToken, BoundBinaryOperatorType.Subtraction, TypeSymbol.i64),
        new(SyntaxType.StarToken, BoundBinaryOperatorType.Multiplication, TypeSymbol.i64),
        new(SyntaxType.SlashToken, BoundBinaryOperatorType.Division, TypeSymbol.i64),
        new(SyntaxType.PercentToken, BoundBinaryOperatorType.Modulo, TypeSymbol.i64),
        new(SyntaxType.PlusToken, BoundBinaryOperatorType.Addition, TypeSymbol.u64),
        new(SyntaxType.MinusToken, BoundBinaryOperatorType.Subtraction, TypeSymbol.u64),
        new(SyntaxType.StarToken, BoundBinaryOperatorType.Multiplication, TypeSymbol.u64),
        new(SyntaxType.SlashToken, BoundBinaryOperatorType.Division, TypeSymbol.u64),
        new(SyntaxType.PercentToken, BoundBinaryOperatorType.Modulo, TypeSymbol.u64),
        new(SyntaxType.PlusToken, BoundBinaryOperatorType.Addition, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.MinusToken, BoundBinaryOperatorType.Subtraction, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.StarToken, BoundBinaryOperatorType.Multiplication, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.SlashToken, BoundBinaryOperatorType.Division, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.PercentToken, BoundBinaryOperatorType.Modulo, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.PlusToken, BoundBinaryOperatorType.Addition, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.MinusToken, BoundBinaryOperatorType.Subtraction, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.StarToken, BoundBinaryOperatorType.Multiplication, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.SlashToken, BoundBinaryOperatorType.Division, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.PercentToken, BoundBinaryOperatorType.Modulo, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.PlusToken, BoundBinaryOperatorType.Addition, TypeSymbol.f64),
        new(SyntaxType.MinusToken, BoundBinaryOperatorType.Subtraction, TypeSymbol.f64),
        new(SyntaxType.StarToken, BoundBinaryOperatorType.Multiplication, TypeSymbol.f64),
        new(SyntaxType.SlashToken, BoundBinaryOperatorType.Division, TypeSymbol.f64),
        new(SyntaxType.PercentToken, BoundBinaryOperatorType.Modulo, TypeSymbol.f64),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.f64, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.f64, TypeSymbol.boolean),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.boolean),
        new(SyntaxType.DoubleAndToken, BoundBinaryOperatorType.LogicalAnd, TypeSymbol.boolean),
        new(SyntaxType.DoubleSplitToken, BoundBinaryOperatorType.LogicalOr, TypeSymbol.boolean),
        new(SyntaxType.AndToken, BoundBinaryOperatorType.BitwiseAnd, TypeSymbol.i64),
        new(SyntaxType.SplitToken, BoundBinaryOperatorType.BitwiseOr, TypeSymbol.i64),
        new(SyntaxType.HatToken, BoundBinaryOperatorType.BitwiseXor, TypeSymbol.i64),
        new(SyntaxType.DoubleLessToken, BoundBinaryOperatorType.BitwiseShiftLeft, TypeSymbol.i64),
        new(SyntaxType.DoubleGreaterToken, BoundBinaryOperatorType.BitwiseShiftRight, TypeSymbol.i64),
        new(SyntaxType.AndToken, BoundBinaryOperatorType.BitwiseAnd, TypeSymbol.u64),
        new(SyntaxType.SplitToken, BoundBinaryOperatorType.BitwiseOr, TypeSymbol.u64),
        new(SyntaxType.HatToken, BoundBinaryOperatorType.BitwiseXor, TypeSymbol.u64),
        new(SyntaxType.DoubleLessToken, BoundBinaryOperatorType.BitwiseShiftLeft, TypeSymbol.u64),
        new(SyntaxType.DoubleGreaterToken, BoundBinaryOperatorType.BitwiseShiftRight, TypeSymbol.u64),
        new(SyntaxType.AndToken, BoundBinaryOperatorType.BitwiseAnd, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.SplitToken, BoundBinaryOperatorType.BitwiseOr, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.HatToken, BoundBinaryOperatorType.BitwiseXor, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.DoubleLessToken, BoundBinaryOperatorType.BitwiseShiftLeft, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.DoubleGreaterToken, BoundBinaryOperatorType.BitwiseShiftRight, TypeSymbol.u64, TypeSymbol.i64, TypeSymbol.u64),
        new(SyntaxType.AndToken, BoundBinaryOperatorType.BitwiseAnd, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.SplitToken, BoundBinaryOperatorType.BitwiseOr, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.HatToken, BoundBinaryOperatorType.BitwiseXor, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.DoubleLessToken, BoundBinaryOperatorType.BitwiseShiftLeft, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.DoubleGreaterToken, BoundBinaryOperatorType.BitwiseShiftRight, TypeSymbol.i64, TypeSymbol.u64, TypeSymbol.u64),
        new(SyntaxType.LessToken, BoundBinaryOperatorType.Less, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.LessEqualsToken, BoundBinaryOperatorType.LessEquals, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.GreaterToken, BoundBinaryOperatorType.Greater, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.GreaterEqualsToken, BoundBinaryOperatorType.GreaterEquals, TypeSymbol.i64, TypeSymbol.boolean),
        new(SyntaxType.LessToken, BoundBinaryOperatorType.Less, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.LessEqualsToken, BoundBinaryOperatorType.LessEquals, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.GreaterToken, BoundBinaryOperatorType.Greater, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.GreaterEqualsToken, BoundBinaryOperatorType.GreaterEquals, TypeSymbol.u64, TypeSymbol.boolean),
        new(SyntaxType.LessToken, BoundBinaryOperatorType.Less, TypeSymbol.f64, TypeSymbol.boolean),
        new(SyntaxType.LessEqualsToken, BoundBinaryOperatorType.LessEquals, TypeSymbol.f64, TypeSymbol.boolean),
        new(SyntaxType.GreaterToken, BoundBinaryOperatorType.Greater, TypeSymbol.f64, TypeSymbol.boolean),
        new(SyntaxType.GreaterEqualsToken, BoundBinaryOperatorType.GreaterEquals, TypeSymbol.f64, TypeSymbol.boolean),
        new(SyntaxType.AndToken, BoundBinaryOperatorType.BitwiseAnd, TypeSymbol.boolean),
        new(SyntaxType.SplitToken, BoundBinaryOperatorType.BitwiseOr, TypeSymbol.boolean),
        new(SyntaxType.HatToken, BoundBinaryOperatorType.BitwiseXor, TypeSymbol.boolean),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.boolean),
        new(SyntaxType.PlusToken, BoundBinaryOperatorType.Addition, TypeSymbol.@string),
        new(SyntaxType.DoubleEqualsToken, BoundBinaryOperatorType.Equals, TypeSymbol.@string, TypeSymbol.boolean),
        new(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, TypeSymbol.@string, TypeSymbol.boolean),
    };

    public static BoundBinaryOperator Bind(SyntaxType type, TypeSymbol leftType, TypeSymbol rightType)
    {
        foreach (BoundBinaryOperator binaryOperator in operators)
        {
            if (binaryOperator.type == type && binaryOperator.leftType == leftType &&
                binaryOperator.rightType == rightType)
            {
                return binaryOperator;
            }
        }

        return null;
    }
}