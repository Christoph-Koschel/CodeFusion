namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal sealed class BoundGotoStatement : BoundStatement
{
    public readonly BoundLabel BoundLabel;

    public BoundGotoStatement(BoundLabel boundLabel)
    {
        this.BoundLabel = boundLabel;
    }

    public override BoundNodeType boundType => BoundNodeType.GotoStatement;
}