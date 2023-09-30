using System;
using System.Collections.Generic;
using System.Linq;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Binding.Nodes.Statements;

namespace IllusionScript.Runtime.CFA;

internal partial class ControlFlowGraph
{
    public sealed class BasicBlockBuilder
    {
        private List<BoundStatement> statements;
        private List<BasicBlock> blocks;

        public BasicBlockBuilder()
        {
            blocks = new List<BasicBlock>();
            statements = new List<BoundStatement>();
        }

        public List<BasicBlock> Build(BoundBlockStatement block)
        {
            foreach (BoundStatement statement in block.statements)
            {
                switch (statement.boundType)
                {
                    case BoundNodeType.ExpressionStatement:
                    case BoundNodeType.VariableDeclarationStatement:
                        statements.Add(statement);
                        break;
                    case BoundNodeType.GotoStatement:
                    case BoundNodeType.ConditionalGotoStatement:
                    case BoundNodeType.ReturnStatement:
                        statements.Add(statement);
                        StartBlock();
                        break;
                    case BoundNodeType.LabelStatement:
                        StartBlock();
                        statements.Add(statement);
                        break;
                    default:
                        throw new Exception($"Unexpected statement: {statement.boundType}");
                }
            }

            EndBlock();

            return blocks.ToList();
        }


        private void StartBlock()
        {
            EndBlock();
        }

        private void EndBlock()
        {
            if (statements.Count > 0)
            {
                BasicBlock block = new BasicBlock();
                block.statements.AddRange(statements);
                blocks.Add(block);
                statements.Clear();
            }
        }
    }
}