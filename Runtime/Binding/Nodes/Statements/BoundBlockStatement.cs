using System.Collections.Immutable;

namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal sealed class BoundBlockStatement : BoundStatement
{
    public readonly ImmutableArray<BoundStatement> statements;

    public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
    {
        this.statements = statements;
    }

    public override BoundNodeType boundType => BoundNodeType.BlockStatement;
}