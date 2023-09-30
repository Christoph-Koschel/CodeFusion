using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IllusionScript.Runtime.Binding.Nodes;
using IllusionScript.Runtime.Binding.Nodes.Expressions;
using IllusionScript.Runtime.Binding.Nodes.Statements;
using IllusionScript.Runtime.Binding.Operators;
using IllusionScript.Runtime.CFA;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Lowering;
using IllusionScript.Runtime.Memory;
using IllusionScript.Runtime.Memory.Symbols;
using IllusionScript.Runtime.Parsing;
using IllusionScript.Runtime.Parsing.Nodes;
using IllusionScript.Runtime.Parsing.Nodes.Expressions;
using IllusionScript.Runtime.Parsing.Nodes.Members;
using IllusionScript.Runtime.Parsing.Nodes.Statements;

namespace IllusionScript.Runtime.Binding;

internal sealed class Binder
{
    private readonly FunctionSymbol function;
    private readonly DiagnosticGroup diagnostics;
    private readonly Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> loopStack;
    private int labelCounter;

    private Scope scope;

    public Binder(Scope parent, FunctionSymbol function)
    {
        this.function = function;
        diagnostics = new DiagnosticGroup();
        scope = new Scope(parent);
        loopStack = new Stack<(BoundLabel breakLabel, BoundLabel continueLabel)>();
        labelCounter = 0;

        if (function != null)
        {
            foreach (ParameterSymbol parameter in function.parameters)
            {
                scope.TryDeclareVariable(parameter);
            }
        }
    }

    #region Statements

    private BoundStatement BindErrorStatement()
    {
        return new BoundExpressionStatement(new BoundErrorExpression());
    }

    private BoundStatement BindStatement(Statement syntax)
    {
        BoundStatement result = BindStatementInternal(syntax);

        if (result is BoundExpressionStatement es)
        {
            bool isAllowedExpression = es.expression.boundType
                is BoundNodeType.AssignmentExpression
                or BoundNodeType.ErrorExpression
                or BoundNodeType.CallExpression;

            if (!isAllowedExpression)
            {
                diagnostics.ReportInvalidExpressionStatement(syntax.location);
            }
        }


        return result;
    }

    private BoundStatement BindStatementInternal(Statement syntax)
    {
        switch (syntax.type)
        {
            case SyntaxType.BlockStatement:
                return BindBlockStatement((BlockStatement)syntax);
            case SyntaxType.ExpressionStatement:
                return BindExpressionStatement((ExpressionStatement)syntax);
            case SyntaxType.VariableDeclarationStatement:
                return BindVariableDeclarationStatement((VariableDeclarationStatement)syntax);
            case SyntaxType.IfStatement:
                return BindIfStatement((IfStatement)syntax);
            case SyntaxType.WhileStatement:
                return BindWhileStatement((WhileStatement)syntax);
            case SyntaxType.DoWhileStatement:
                return BindDoWhileStatement((DoWhileStatement)syntax);
            case SyntaxType.ForStatement:
                return BindForStatement((ForStatement)syntax);
            case SyntaxType.ContinueStatement:
                return BindContinueStatement((ContinueStatement)syntax);
            case SyntaxType.BreakStatement:
                return BindBreakStatement((BreakStatement)syntax);
            case SyntaxType.ReturnStatement:
                return BindReturnStatement((ReturnStatement)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.type}");
        }
    }

    private BoundStatement BindReturnStatement(ReturnStatement syntax)
    {
        BoundExpression expression = syntax.expression == null ? null : BindExpression(syntax.expression);

        if (function == null)
        {
            diagnostics.ReportInvalidReturn(syntax.returnKeyword.location);
        }
        else
        {
            if (function.returnType == TypeSymbol.@void)
            {
                if (expression != null)
                {
                    diagnostics.ReportInvalidReturnExpression(syntax.expression.location, function.name);
                }
            }
            else
            {
                if (expression == null)
                {
                    diagnostics.ReportMissingReturnExpression(syntax.returnKeyword.location, function.returnType);
                }
                else
                {
                    expression = BindConversion(syntax.expression.location, expression, function.returnType);
                }
            }
        }

        return new BoundReturnStatement(expression);
    }

    private BoundStatement BindBreakStatement(BreakStatement syntax)
    {
        if (loopStack.Count == 0)
        {
            diagnostics.ReportInvalidBreakOrContinue(syntax.keyword.location, syntax.keyword.text);
            return BindErrorStatement();
        }

        BoundLabel breakLabel = loopStack.Peek().breakLabel;
        return new BoundGotoStatement(breakLabel);
    }

    private BoundStatement BindContinueStatement(ContinueStatement syntax)
    {
        if (loopStack.Count == 0)
        {
            diagnostics.ReportInvalidBreakOrContinue(syntax.keyword.location, syntax.keyword.text);
            return BindErrorStatement();
        }

        BoundLabel continueLabel = loopStack.Peek().continueLabel;
        return new BoundGotoStatement(continueLabel);
    }

    private BoundStatement BindForStatement(ForStatement syntax)
    {
        BoundExpression startExpression = BindExpression(syntax.startExpression, TypeSymbol.i64);
        BoundExpression endExpression = BindExpression(syntax.endExpression, TypeSymbol.i64);

        scope = new Scope(scope);

        VariableSymbol variable = BindVariable(syntax.identifier, true, TypeSymbol.i64);
        BoundStatement body = BindLoopBody(syntax.body, out BoundLabel breakLabel, out BoundLabel continueLabel);

        scope = scope.parent;
        return new BoundForStatement(variable, startExpression, endExpression, body, breakLabel, continueLabel);
    }

    private BoundStatement BindDoWhileStatement(DoWhileStatement syntax)
    {
        BoundStatement body = BindLoopBody(syntax.body, out BoundLabel breakLabel, out BoundLabel continueLabel);
        BoundExpression condition = BindExpression(syntax.condition, TypeSymbol.boolean);
        return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel);
    }

    private BoundStatement BindWhileStatement(WhileStatement syntax)
    {
        BoundExpression condition = BindExpression(syntax.condition, TypeSymbol.boolean);
        BoundStatement body = BindLoopBody(syntax.body, out BoundLabel breakLabel, out BoundLabel continueLabel);

        return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
    }

    private BoundStatement BindLoopBody(Statement body, out BoundLabel breakLabel, out BoundLabel continueLabel)
    {
        breakLabel = new BoundLabel("b" + labelCounter);
        continueLabel = new BoundLabel("c" + labelCounter);
        labelCounter++;

        loopStack.Push((breakLabel, continueLabel));
        BoundStatement boundBody = BindStatement(body);
        loopStack.Pop();
        return boundBody;
    }

    private BoundStatement BindIfStatement(IfStatement syntax)
    {
        BoundExpression condition = BindExpression(syntax.condition, TypeSymbol.boolean);
        BoundStatement statement = BindStatement(syntax.body);
        BoundStatement elseStatement =
            syntax.elseClause == null ? null : BindStatement(syntax.elseClause.body);

        return new BoundIfStatement(condition, statement, elseStatement);
    }

    private BoundStatement BindVariableDeclarationStatement(VariableDeclarationStatement syntax)
    {
        bool isReadOnly = syntax.keyword.type == SyntaxType.ConstKeyword;
        TypeSymbol type = BindTypeClause(syntax.typeClause);
        BoundExpression initializer = BindExpression(syntax.initializer);
        TypeSymbol variableType = type ?? initializer.type;
        VariableSymbol variable = BindVariable(syntax.identifier, isReadOnly, variableType);
        BoundExpression convertedInitializer =
            BindConversion(syntax.initializer.location, initializer, variableType);

        return new BoundVariableDeclarationStatement(variable, convertedInitializer);
    }

    private TypeSymbol BindTypeClause(TypeClause syntax, bool enableVoid = false)
    {
        TypeSymbol type = LookupType(syntax.identifier.text, enableVoid);
        if (type == null && !string.IsNullOrEmpty(syntax.identifier.text))
        {
            diagnostics.ReportUndefinedType(syntax.identifier.location, syntax.identifier.text);
        }

        return type;
    }

    private BoundStatement BindBlockStatement(BlockStatement syntax)
    {
        ImmutableArray<BoundStatement>.Builder statements = ImmutableArray.CreateBuilder<BoundStatement>();
        scope = new Scope(scope);
        foreach (Statement statement in syntax.statements)
        {
            BoundStatement boundStatement = BindStatement(statement);
            statements.Add(boundStatement);
        }

        scope = scope.parent;

        return new BoundBlockStatement(statements.ToImmutable());
    }

    private BoundStatement BindExpressionStatement(ExpressionStatement syntax)
    {
        BoundExpression expression = BindExpression(syntax.expression, true);
        return new BoundExpressionStatement(expression);
    }

    private BoundExpression BindExpression(Expression syntax, TypeSymbol target)
    {
        return BindConversion(syntax, target);
    }

    #endregion

    #region Expression

    private BoundExpression BindExpression(Expression syntax, bool canBeVoid = false)
    {
        BoundExpression result = BindExpressionInternal(syntax);
        if (!canBeVoid && result.type == TypeSymbol.@void)
        {
            diagnostics.ReportExpressionMustHaveValue(syntax.location);
            return new BoundErrorExpression();
        }

        return result;
    }


    private BoundExpression BindExpressionInternal(Expression syntax)
    {
        switch (syntax.type)
        {
            case SyntaxType.ParenExpression:
                return BindParenExpression((ParenExpression)syntax);
            case SyntaxType.LiteralExpression:
                return BindLiteralExpression((LiteralExpression)syntax);
            case SyntaxType.UnaryExpression:
                return BindUnaryExpression((UnaryExpression)syntax);
            case SyntaxType.BinaryExpression:
                return BindBinaryExpression((BinaryExpression)syntax);
            case SyntaxType.NameExpression:
                return BindNameExpression((NameExpression)syntax);
            case SyntaxType.AssignmentExpression:
                return BindAssignmentExpression((AssignmentExpression)syntax);
            case SyntaxType.CallExpression:
                return BindCallExpression((CallExpression)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.type}");
        }
    }

    private BoundExpression BindCallExpression(CallExpression syntax)
    {
        if (syntax.arguments.Length == 1 && LookupType(syntax.identifier.text) is TypeSymbol type)
        {
            return BindConversion(syntax.arguments[0], type, true);
        }

        ImmutableArray<BoundExpression>.Builder arguments = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (Expression argument in syntax.arguments)
        {
            BoundExpression boundArgument = BindExpression(argument);
            arguments.Add(boundArgument);
        }

        if (!scope.TryLookupFunction(syntax.identifier.text, out FunctionSymbol function))
        {
            diagnostics.ReportUndefinedFunction(syntax.identifier.location, syntax.identifier.text);
            return new BoundErrorExpression();
        }

        if (syntax.arguments.Length != function.parameters.Length)
        {
            TextSpan span;
            if (syntax.arguments.Length > function.parameters.Length)
            {
                Node firstExceedingNode;
                if (function.parameters.Length > 0)
                {
                    firstExceedingNode = syntax.arguments.GetSeparator(function.parameters.Length - 1);
                }
                else
                {
                    firstExceedingNode = syntax.arguments[0];
                }

                Expression lastExceedingArgument = syntax.arguments[^1];
                span = TextSpan.FromBounds(firstExceedingNode.span.start, lastExceedingArgument.span.end);
            }
            else
            {
                span = syntax.rParen.span;
            }

            TextLocation location = new TextLocation(syntax.location.text, span);
            diagnostics.ReportWrongArgumentCount(location, syntax.identifier.text,
                function.parameters.Length,
                syntax.arguments.Length);
            return new BoundErrorExpression();
        }

        for (int i = 0; i < syntax.arguments.Length; i++)
        {
            TextLocation argumentLocation = syntax.arguments[i].location;
            BoundExpression argument = arguments[i];
            ParameterSymbol parameter = function.parameters[i];

            BoundExpression convertedArgument = BindConversion(argumentLocation, argument, parameter.type);
            arguments[i] = convertedArgument;
        }

        return new BoundCallExpression(function, arguments.ToImmutable());
    }

    private BoundExpression BindLiteralExpression(LiteralExpression syntax)
    {
        object value = syntax.value ?? 0;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindUnaryExpression(UnaryExpression syntax)
    {
        BoundExpression right = BindExpression(syntax.right);

        if (right.type == TypeSymbol.error)
        {
            return new BoundErrorExpression();
        }

        BoundUnaryOperator unaryOperator = BoundUnaryOperator.Bind(syntax.operatorToken.type, right.type.ToMaxCapacity());
        if (unaryOperator == null)
        {
            diagnostics.ReportUndefinedUnaryOperator(syntax.operatorToken.location, syntax.operatorToken.text, right.type);
            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(unaryOperator, right);
    }


    private BoundExpression BindBinaryExpression(BinaryExpression syntax)
    {
        BoundExpression left = BindExpression(syntax.left);
        BoundExpression right = BindExpression(syntax.right);

        if (left.type == TypeSymbol.error || right.type == TypeSymbol.error)
        {
            return new BoundErrorExpression();
        }

        BoundBinaryOperator binaryOperator =
            BoundBinaryOperator.Bind(syntax.operatorToken.type, left.type.ToMaxCapacity(), right.type.ToMaxCapacity());

        if (binaryOperator == null)
        {
            diagnostics.ReportUndefinedBinaryOperator(syntax.operatorToken.location, syntax.operatorToken.text,
                left.type, right.type);
            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(left, binaryOperator, right);
    }

    private BoundExpression BindParenExpression(ParenExpression syntax)
    {
        return BindExpression(syntax.expression);
    }

    private BoundExpression BindNameExpression(NameExpression syntax)
    {
        string name = syntax.identifier.text;
        if (string.IsNullOrEmpty(name))
        {
            return new BoundErrorExpression();
        }

        if (!scope.TryLookupVariable(name, out VariableSymbol variable))
        {
            diagnostics.ReportUndefinedIdentifier(syntax.identifier.location, name);
            return new BoundErrorExpression();
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpression syntax)
    {
        string name = syntax.identifier.text;
        BoundExpression boundExpression = BindExpression(syntax.expression);

        if (!scope.TryLookupVariable(name, out VariableSymbol variable))
        {
            diagnostics.ReportUndefinedIdentifier(syntax.identifier.location, name);
            return new BoundErrorExpression();
        }

        if (variable.isReadOnly)
        {
            diagnostics.ReportCannotAssign(syntax.identifier.location, name);
        }

        BoundExpression convertedExpression =
            BindConversion(syntax.expression.location, boundExpression, variable.type);
        return new BoundAssignmentExpression(variable, convertedExpression);
    }

    #endregion

    private void BindFunctionDeclarationMember(FunctionDeclarationMember syntax)
    {
        ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        HashSet<string> seenParameterName = new HashSet<string>();
        foreach (Parameter parameter in syntax.parameters)
        {
            string parameterName = parameter.identifier.text;
            TypeSymbol parameterType = BindTypeClause(parameter.typeClause);
            if (!seenParameterName.Add(parameterName))
            {
                diagnostics.ReportParameterAlreadyDeclared(parameter.location, parameterName);
            }
            else
            {
                ParameterSymbol p = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(p);
            }
        }

        TypeSymbol type = BindTypeClause(syntax.typeClause, true) ?? TypeSymbol.@void;

        FunctionSymbol function =
            new FunctionSymbol(syntax.identifier.text, parameters.ToImmutable(), type, syntax);
        if (function.declaration.identifier.text != null && !scope.TryDeclareFunction(function))
        {
            diagnostics.ReportSymbolAlreadyDeclared(syntax.identifier.location, function.name);
        }
    }

    private void BindExternFunctionDeclarationMember(ExternFunctionDeclarationMember syntax)
    {
        ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        HashSet<string> seenParameterName = new HashSet<string>();
        foreach (Parameter parameter in syntax.parameters)
        {
            string parameterName = parameter.identifier.text;
            TypeSymbol parameterType = BindTypeClause(parameter.typeClause);
            if (!seenParameterName.Add(parameterName))
            {
                diagnostics.ReportParameterAlreadyDeclared(parameter.location, parameterName);
            }
            else
            {
                ParameterSymbol p = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(p);
            }
        }

        TypeSymbol type = BindTypeClause(syntax.typeClause, true) ?? TypeSymbol.@void;

        FunctionSymbol function =
            new FunctionSymbol(syntax.identifier.text, parameters.ToImmutable(), type);
        if (function.name != null && !scope.TryDeclareFunction(function))
        {
            diagnostics.ReportSymbolAlreadyDeclared(syntax.identifier.location, function.name);
        }
    }


    public static BoundProgram BindProgram(GlobalScope globalScope)
    {
        Scope parentScope = CreateBaseScope(globalScope);
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder functionBodies =
            ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        DiagnosticGroup diagnostics = new DiagnosticGroup();

        foreach (FunctionSymbol function in globalScope.functions)
        {
            if (function.declaration == null)
            {
                continue;
            }

            Binder binder = new Binder(parentScope, function);
            BoundStatement body = binder.BindStatement(function.declaration.body);
            BoundBlockStatement loweredBody = Lowerer.Lower(body);

            if (function.returnType != TypeSymbol.@void && !ControlFlowGraph.AllPathsReturn(loweredBody))
            {
                binder.diagnostics.ReportAllPathsMustReturn(function.declaration.identifier.location);
            }

            functionBodies.Add(function, loweredBody);

            diagnostics.AddRange(binder.diagnostics);
        }

        return new BoundProgram(globalScope, diagnostics, globalScope.mainFunction, functionBodies.ToImmutable());
    }

    public static GlobalScope BindGlobalScope(ImmutableArray<SyntaxTree> syntaxTrees)
    {
        Scope parentScope = CreateRootScope();
        Binder binder = new Binder(parentScope, null);

        IEnumerable<FunctionDeclarationMember> functionDeclarations =
            syntaxTrees.SelectMany(st => st.root.members).OfType<FunctionDeclarationMember>();

        foreach (FunctionDeclarationMember function in functionDeclarations)
        {
            binder.BindFunctionDeclarationMember(function);
        }

        IEnumerable<ExternFunctionDeclarationMember> externFunctionDeclarations =
            syntaxTrees.SelectMany(st => st.root.members).OfType<ExternFunctionDeclarationMember>();

        foreach (ExternFunctionDeclarationMember function in externFunctionDeclarations)
        {
            binder.BindExternFunctionDeclarationMember(function);
        }

        ImmutableArray<FunctionSymbol> functions = binder.scope.GetDeclaredFunctions();

        FunctionSymbol mainFunction = functions.FirstOrDefault(f => f.name == "main");

        if (mainFunction != null)
        {
            if (mainFunction.returnType != TypeSymbol.@void || mainFunction.parameters.Any())
            {
                binder.diagnostics.ReportMainMustHaveCorrectSignature(mainFunction.declaration.identifier
                    .location);
            }
        }

        ImmutableArray<Diagnostic> diagnostics = binder.diagnostics.ToImmutableArray();
        return new GlobalScope(diagnostics, mainFunction, functions);
    }

    private static Scope CreateRootScope()
    {
        Scope result = new Scope(null);
        foreach (FunctionSymbol symbol in BuiltInFunctions.GetAll())
        {
            result.TryDeclareFunction(symbol);
        }

        return result;
    }

    private static Scope CreateBaseScope(GlobalScope globalScope)
    {
        Scope scope = new Scope(null);
        foreach (FunctionSymbol function in globalScope.functions)
        {
            scope.TryDeclareFunction(function);
        }

        return scope;
    }


    private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
    {
        string name = identifier.text ?? "?";
        bool declare = !identifier.isMissing;
        VariableSymbol variable = function == null
            ? new GlobalVariableSymbol(name, isReadOnly, type)
            : new LocalVariableSymbol(name, isReadOnly, type);

        if (declare && !scope.TryDeclareVariable(variable))
        {
            diagnostics.ReportSymbolAlreadyDeclared(identifier.location, name);
        }

        return variable;
    }

    private BoundExpression BindConversion(Expression syntax, TypeSymbol type, bool allowExplicit = false)
    {
        BoundExpression expression = BindExpression(syntax);
        return BindConversion(syntax.location, expression, type, allowExplicit);
    }

    private BoundExpression BindConversion(TextLocation location, BoundExpression expression, TypeSymbol type,
        bool allowExplicit = false)
    {
        Conversion conversion = Conversion.Classify(expression.type, type);

        if (!conversion.exists)
        {
            if (expression.type != TypeSymbol.error && type != TypeSymbol.error)
            {
                diagnostics.ReportCannotConvert(location, expression.type, type);
            }

            return new BoundErrorExpression();
        }

        if (!allowExplicit && conversion.isExplicit)
        {
            diagnostics.ReportCannotConvertConvertImplicitly(location, expression.type, type);
        }

        if (conversion.isIdentity)
        {
            return expression;
        }

        return new BoundConversionExpression(type, expression);
    }

    private TypeSymbol LookupType(string name, bool enableVoid = false)
    {
        if (enableVoid && name == TypeSymbol.@void.name)
        {
            return TypeSymbol.@void;
        }

        foreach (TypeSymbol symbol in TypeSymbol.primitives)
        {
            if (symbol.name == name)
            {
                return symbol;
            }
        }

        return null;
    }
}