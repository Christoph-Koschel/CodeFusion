using System;
using System.Collections.Immutable;
using IllusionScript.Runtime.Binding.Nodes.Expressions;
using IllusionScript.Runtime.Binding.Nodes.Statements;

namespace IllusionScript.Runtime.Binding;

internal abstract class BoundTreeRewriter
{
    public virtual BoundStatement RewriteStatement(BoundStatement node)
    {
        switch (node.boundType)
        {
            case BoundNodeType.BlockStatement:
                return RewriteBlockStatement((BoundBlockStatement)node);
            case BoundNodeType.ExpressionStatement:
                return RewriteExpressionStatement((BoundExpressionStatement)node);
            case BoundNodeType.VariableDeclarationStatement:
                return RewriteVariableDeclarationStatement((BoundVariableDeclarationStatement)node);
            case BoundNodeType.IfStatement:
                return RewriteIfStatement((BoundIfStatement)node);
            case BoundNodeType.WhileStatement:
                return RewriteWhileStatement((BoundWhileStatement)node);
            case BoundNodeType.DoWhileStatement:
                return RewriteDoWhileStatement((BoundDoWhileStatement)node);
            case BoundNodeType.ForStatement:
                return RewriteForStatement((BoundForStatement)node);
            case BoundNodeType.GotoStatement:
                return RewriteGotoStatement((BoundGotoStatement)node);
            case BoundNodeType.ConditionalGotoStatement:
                return RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node);
            case BoundNodeType.LabelStatement:
                return RewriteLabelStatement((BoundLabelStatement)node);
            case BoundNodeType.ReturnStatement:
                return RewriteReturnStatement((BoundReturnStatement)node);

            default:
                throw new Exception($"Unexpected node: {node.boundType}");
        }
    }

    private BoundStatement RewriteReturnStatement(BoundReturnStatement node)
    {
        BoundExpression expression = node.expression == null ? null : RewriteExpression(node.expression);
        if (expression == node.expression)
        {
            return node;
        }

        return new BoundReturnStatement(expression);
    }

    protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node)
    {
        return node;
    }

    protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
    {
        BoundExpression condition = RewriteExpression(node.condition);
        if (condition == node.condition)
        {
            return node;
        }

        return new BoundConditionalGotoStatement(node.boundLabel, condition, node.jmpIfTrue);
    }

    protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node)
    {
        return node;
    }

    protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
    {
        BoundExpression startExpression = RewriteExpression(node.startExpression);
        BoundExpression endExpression = RewriteExpression(node.endExpression);
        BoundStatement body = RewriteStatement(node.body);

        if (startExpression == node.startExpression && endExpression == node.endExpression && body == node.body)
        {
            return node;
        }

        return new BoundForStatement(node.variable, startExpression, endExpression, body,
            node.breakLabel, node.continueLabel);
    }

    protected virtual BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
    {
        BoundStatement body = RewriteStatement(node.body);
        BoundExpression condition = RewriteExpression(node.condition);

        if (body == node.body && condition == node.condition)
        {
            return node;
        }

        return new BoundDoWhileStatement(body, condition, node.breakLabel, node.continueLabel);
    }

    protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        BoundExpression condition = RewriteExpression(node.condition);
        BoundStatement body = RewriteStatement(node.body);

        if (condition == node.condition && body == node.body)
        {
            return node;
        }

        return new BoundWhileStatement(condition, body, node.breakLabel, node.continueLabel);
    }

    protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        BoundExpression condition = RewriteExpression(node.condition);
        BoundStatement body = RewriteStatement(node.body);

        BoundStatement elseStatement = node.elseBody == null ? null : RewriteStatement(node.elseBody);
        if (condition == node.condition && body == node.body && elseStatement == node.elseBody)
        {
            return node;
        }

        return new BoundIfStatement(condition, body, elseStatement);
    }

    protected virtual BoundStatement RewriteVariableDeclarationStatement(BoundVariableDeclarationStatement node)
    {
        BoundExpression initializer = RewriteExpression(node.initializer);
        if (initializer == node.initializer)
        {
            return node;
        }

        return new BoundVariableDeclarationStatement(node.variable, node.initializer);
    }

    protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
    {
        return node;
    }

    protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
    {
        ImmutableArray<BoundStatement>.Builder builder = null;

        for (int i = 0; i < node.statements.Length; i++)
        {
            BoundStatement statement = RewriteStatement(node.statements[i]);
            if (statement != node.statements[i])
            {
                if (builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<BoundStatement>(node.statements.Length);
                    for (int j = 0; j < i; j++)
                    {
                        builder.Add(node.statements[j]);
                    }
                }
            }

            if (builder != null)
            {
                builder.Add(statement);
            }
        }

        if (builder == null)
        {
            return node;
        }

        return new BoundBlockStatement(builder.ToImmutable());
    }

    protected virtual BoundExpression RewriteExpression(BoundExpression node)
    {
        switch (node.boundType)
        {
            case BoundNodeType.BinaryExpression:
                return RewriteBinaryExpression((BoundBinaryExpression)node);
            case BoundNodeType.UnaryExpression:
                return RewriteUnaryExpression((BoundUnaryExpression)node);
            case BoundNodeType.AssignmentExpression:
                return RewriteAssignmentExpression((BoundAssignmentExpression)node);
            case BoundNodeType.LiteralExpression:
                return RewriteLiteralExpression((BoundLiteralExpression)node);
            case BoundNodeType.VariableExpression:
                return RewriteVariableExpression((BoundVariableExpression)node);
            case BoundNodeType.CallExpression:
                return RewriteCallExpression((BoundCallExpression)node);
            case BoundNodeType.ConversionExpression:
                return RewriteConversionExpression((BoundConversionExpression)node);
            case BoundNodeType.ErrorExpression:
                return RewriteErrorExpression((BoundErrorExpression)node);
            default:
                throw new Exception($"Unexpected node: {node.boundType}");
        }
    }

    protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
    {
        BoundExpression expression = RewriteExpression(node.expression);
        if (expression == node.expression)
        {
            return node;
        }

        return new BoundConversionExpression(node.type, expression);
    }

    protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
    {
        ImmutableArray<BoundExpression>.Builder builder = null;

        for (int i = 0; i < node.arguments.Length; i++)
        {
            BoundExpression statement = RewriteExpression(node.arguments[i]);
            if (statement != node.arguments[i])
            {
                if (builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<BoundExpression>(node.arguments.Length);
                    for (int j = 0; j < i; j++)
                    {
                        builder.Add(node.arguments[j]);
                    }
                }
            }

            if (builder != null)
            {
                builder.Add(statement);
            }
        }

        if (builder == null)
        {
            return node;
        }

        return new BoundCallExpression(node.function, builder.ToImmutable());
    }

    protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
    {
        BoundExpression expressionRight = RewriteExpression(node.right);
        BoundExpression expressionLeft = RewriteExpression(node.left);
        if (expressionLeft == node.left && expressionRight == node.right)
        {
            return node;
        }

        return new BoundBinaryExpression(expressionLeft, node.binaryOperator, expressionRight);
    }

    protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
    {
        BoundExpression expression = RewriteExpression(node.right);
        if (expression == node.right)
        {
            return node;
        }

        return new BoundUnaryExpression(node.unaryOperator, expression);
    }

    protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
    {
        BoundExpression expression = RewriteExpression(node.expression);
        if (expression == node.expression)
        {
            return node;
        }

        return new BoundAssignmentExpression(node.variableSymbol, expression);
    }

    protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
    {
        return node;
    }

    protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node)
    {
        return node;
    }
}