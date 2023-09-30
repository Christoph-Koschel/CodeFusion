using IllusionScript.Runtime.Binding.Nodes.Expressions;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal sealed class BoundForStatement : BoundLoopStatement
{
    public readonly VariableSymbol variable;
    public readonly BoundExpression startExpression;
    public readonly BoundExpression endExpression;
    public readonly BoundStatement body;

    public BoundForStatement(VariableSymbol variable, BoundExpression startExpression,
        BoundExpression endExpression, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        : base(breakLabel, continueLabel)
    {
        this.variable = variable;
        this.startExpression = startExpression;
        this.endExpression = endExpression;
        this.body = body;
    }

    public override BoundNodeType boundType => BoundNodeType.ForStatement;
}