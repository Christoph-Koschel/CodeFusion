namespace IllusionScript.Runtime.Binding.Nodes.Statements;

internal abstract class BoundLoopStatement : BoundStatement
{
    public readonly BoundLabel breakLabel;
    public readonly BoundLabel continueLabel;

    protected BoundLoopStatement(BoundLabel breakLabel, BoundLabel continueLabel)
    {
        this.breakLabel = breakLabel;
        this.continueLabel = continueLabel;
    }
}