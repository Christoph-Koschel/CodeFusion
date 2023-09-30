using System;
using System.Collections.Generic;
using System.Linq;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Binding.Nodes;
using IllusionScript.Runtime.Binding.Nodes.Expressions;
using IllusionScript.Runtime.Binding.Nodes.Statements;
using IllusionScript.Runtime.Binding.Operators;
using IllusionScript.Runtime.Memory.Symbols;
using IllusionScript.Runtime.Parsing;

namespace IllusionScript.Runtime.CFA;

internal partial class ControlFlowGraph
{
    public sealed class GraphBuilder
    {
        private readonly Dictionary<BoundStatement, BasicBlock> blockFromStatement;
        private Dictionary<BoundLabel, BasicBlock> blockFromLabel;
        private List<BasicBlockBranch> branches;
        private BasicBlock start;
        private BasicBlock end;

        public GraphBuilder()
        {
            branches = new List<BasicBlockBranch>();
            blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
        }

        public ControlFlowGraph Build(List<BasicBlock> blocks)
        {
            start = new BasicBlock(true);
            end = new BasicBlock(false);

            if (!blocks.Any())
            {
                Connect(start, end);
            }
            else
            {
                Connect(start, blocks.First());
            }

            foreach (BasicBlock block in blocks)
            {
                foreach (BoundStatement statement in block.statements)
                {
                    blockFromStatement.Add(statement, block);
                    if (statement is BoundLabelStatement labelStatement)
                    {
                        blockFromLabel.Add(labelStatement.BoundLabel, block);
                    }
                }
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                BasicBlock current = blocks[i];
                BasicBlock next = i == blocks.Count - 1 ? end : blocks[i + 1];

                foreach (BoundStatement statement in current.statements)
                {
                    bool isLast = statement == current.statements.Last();
                    switch (statement.boundType)
                    {
                        case BoundNodeType.ExpressionStatement:
                        case BoundNodeType.LabelStatement:
                        case BoundNodeType.VariableDeclarationStatement:
                            if (isLast)
                            {
                                Connect(current, next);
                            }

                            break;
                        case BoundNodeType.GotoStatement:
                        {
                            BoundGotoStatement s = (BoundGotoStatement)statement;
                            BasicBlock toBlock = blockFromLabel[s.BoundLabel];
                            Connect(current, toBlock);
                            break;
                        }
                        case BoundNodeType.ConditionalGotoStatement:
                        {
                            BoundConditionalGotoStatement s = (BoundConditionalGotoStatement)statement;
                            BasicBlock thenBlock = blockFromLabel[s.boundLabel];
                            BasicBlock elseBlock = next;

                            BoundExpression negatedCondition = Negate(s.condition);
                            BoundExpression thenCondition = s.jmpIfTrue ? s.condition : negatedCondition;
                            BoundExpression elseCondition = s.jmpIfTrue ? negatedCondition : s.condition;

                            string thenKeyword = s.jmpIfTrue ? "true" : "false";
                            string elseKeyword = s.jmpIfTrue ? "false" : "true";

                            Connect(current, thenBlock, thenKeyword, thenCondition);
                            Connect(current, elseBlock, elseKeyword, elseCondition);
                            break;
                        }
                        case BoundNodeType.ReturnStatement:
                            Connect(current, end);
                            break;
                        default:
                            throw new Exception($"Unexpected statement: {statement.boundType}");
                    }
                }
            }

            ScanAgain:
            foreach (BasicBlock block in blocks)
            {
                if (!block.incoming.Any())
                {
                    RemoveBlock(blocks, block);
                    goto ScanAgain;
                }
            }

            blocks.Insert(0, start);
            blocks.Add(end);

            return new ControlFlowGraph(start, end, blocks, branches);
        }

        private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
        {
            foreach (BasicBlockBranch branch in block.incoming)
            {
                branch.from.outgoing.Remove(branch);
                branches.Remove(branch);
            }
                
            foreach (BasicBlockBranch branch in block.outgoing)
            {
                branch.to.incoming.Remove(branch);
                branches.Remove(branch);
            }
                
            blocks.Remove(block);
        }

        private BoundExpression Negate(BoundExpression condition)
        {
            if (condition is BoundLiteralExpression l)
            {
                bool value = (bool)l.value;
                return new BoundLiteralExpression(!value);
            }

            BoundUnaryOperator unaryOperator = BoundUnaryOperator.Bind(SyntaxType.BangToken, TypeSymbol.boolean);
            return new BoundUnaryExpression(unaryOperator, condition);
        }

        private void Connect(BasicBlock from, BasicBlock to, string condition = "",
            BoundExpression expression = null)
        {
            if (expression is BoundLiteralExpression l)
            {
                bool value = (bool)l.value;
                if (value)
                {
                    condition = null;
                }
                else
                {
                    return;
                }
            }

            BasicBlockBranch branch = new BasicBlockBranch(from, to, condition);
            from.outgoing.Add(branch);
            to.incoming.Add(branch);
            branches.Add(branch);
        }
    }
}