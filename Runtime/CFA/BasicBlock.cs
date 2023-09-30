using System.Collections.Generic;
using System.IO;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Binding.Nodes.Statements;

namespace IllusionScript.Runtime.CFA;

internal partial class ControlFlowGraph
{
    public sealed class BasicBlock
    {
        public List<BoundStatement> statements;
        public List<ControlFlowGraph.BasicBlockBranch> incoming;
        public List<ControlFlowGraph.BasicBlockBranch> outgoing;
        private readonly bool isStart;
        private readonly bool isEnd;

        public BasicBlock()
        {
            statements = new List<BoundStatement>();
            incoming = new List<ControlFlowGraph.BasicBlockBranch>();
            outgoing = new List<ControlFlowGraph.BasicBlockBranch>();
        }

        public BasicBlock(bool isStart)
        {
            statements = new List<BoundStatement>();
            incoming = new List<ControlFlowGraph.BasicBlockBranch>();
            outgoing = new List<ControlFlowGraph.BasicBlockBranch>();
            this.isStart = isStart;
            isEnd = !isStart;
        }

        public override string ToString()
        {
            if (isStart)
            {
                return "<Start>";
            }

            if (isEnd)
            {
                return "<End>";
            }

            using StringWriter stringWriter = new StringWriter();
            foreach (BoundStatement statement in statements)
            {
                statement.WriteTo(stringWriter);
            }

            return stringWriter.ToString();
        }
    }
}