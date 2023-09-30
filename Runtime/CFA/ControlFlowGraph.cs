using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Binding.Nodes.Statements;

namespace IllusionScript.Runtime.CFA;

internal partial class ControlFlowGraph
{
    public readonly BasicBlock start;
    public readonly BasicBlock end;
    public readonly List<BasicBlock> blocks;
    public readonly List<BasicBlockBranch> branches;

    private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks,
        List<BasicBlockBranch> branches)
    {
        this.start = start;
        this.end = end;
        this.blocks = blocks;
        this.branches = branches;
    }

    public void WriteTo(TextWriter writer)
    {
        writer.WriteLine("digraph G {");

        Dictionary<BasicBlock, string> blockIds = new Dictionary<BasicBlock, string>();
        for (int i = 0; i < blocks.Count; i++)
        {
            string id = $"N{i}";
            blockIds.Add(blocks[i], id);
        }

        string Quote(string text)
        {
            return "\"" + text.Replace("\"", "\\\"") + "\"";
        }

        foreach (BasicBlock block in blocks)
        {
            string id = blockIds[block];
            string label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
            writer.WriteLine($"\t{id} [label = {label} shape = box]");
        }


        foreach (BasicBlockBranch branch in branches)
        {
            string fromId = blockIds[branch.from];
            string toId = blockIds[branch.to];
            string label = Quote(branch.condition == null ? string.Empty : branch.condition.ToString());
            writer.WriteLine($"\t{fromId} -> {toId} [label = {label}]");
        }

        writer.WriteLine("}");
    }

    public static ControlFlowGraph Create(BoundBlockStatement body)
    {
        BasicBlockBuilder basicBlockBuilder = new BasicBlockBuilder();
        List<BasicBlock> blocks = basicBlockBuilder.Build(body);

        GraphBuilder graphBuilder = new GraphBuilder();
        return graphBuilder.Build(blocks);
    }

    public static bool AllPathsReturn(BoundBlockStatement body)
    {
        ControlFlowGraph graph = Create(body);

        foreach (BasicBlockBranch branch in graph.end.incoming)
        {
            BoundStatement lastStatement = branch.from.statements.LastOrDefault();

            if (lastStatement == null || lastStatement.boundType != BoundNodeType.ReturnStatement)
            {
                return false;
            }
        }

        return true;
    }
}