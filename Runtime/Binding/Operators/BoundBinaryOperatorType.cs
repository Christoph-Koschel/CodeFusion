namespace IllusionScript.Runtime.Binding.Operators;

internal enum BoundBinaryOperatorType
{
    // Math
    Addition,
    Subtraction, 
    Multiplication,
    Division,
    Modulo,
    Pow,
        
    // Logical
    LogicalAnd,
    LogicalOr,
    NotEquals,
    Equals,
        
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    BitwiseShiftLeft,
    BitwiseShiftRight,
        
    Less,
    LessEquals,
    Greater,
    GreaterEquals,
}