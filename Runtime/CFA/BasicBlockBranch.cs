namespace IllusionScript.Runtime.CFA;

internal partial class ControlFlowGraph
{
    public sealed class BasicBlockBranch
    {
        public readonly BasicBlock from;
        public readonly BasicBlock to;
        public readonly string condition;

        public BasicBlockBranch(BasicBlock from, BasicBlock to, string condition)
        {
            this.from = from;
            this.to = to;
            this.condition = condition;
        }

        public override string ToString()
        {
            if (condition == null)
            {
                return string.Empty;
            }

            return condition;
        }
    }
}