namespace IllusionScript.Runtime.Binding;

public enum BoundNodeType
{
    // Expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    CallExpression,
    ConversionExpression,
    ErrorExpression,
        
    // Statements
    BlockStatement,
    ExpressionStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
    GotoStatement,
    LabelStatement,
    ConditionalGotoStatement,
    ReturnStatement
}