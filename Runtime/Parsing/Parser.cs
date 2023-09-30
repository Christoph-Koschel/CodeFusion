using System.Collections.Generic;
using System.Collections.Immutable;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Extension;
using IllusionScript.Runtime.Lexing;
using IllusionScript.Runtime.Parsing.Nodes;
using IllusionScript.Runtime.Parsing.Nodes.Expressions;
using IllusionScript.Runtime.Parsing.Nodes.Members;
using IllusionScript.Runtime.Parsing.Nodes.Statements;

namespace IllusionScript.Runtime.Parsing;

public sealed class Parser : SyntaxFacts
{
    private readonly SyntaxTree syntaxTree;
    private readonly Token[] tokens;
    private readonly DiagnosticGroup diagnostics;
    private int position;
    private Token current => Peek(0);

    internal Parser(SyntaxTree syntaxTree)
    {
        diagnostics = new DiagnosticGroup();
        position = 0;

        Lexer lexer = new Lexer(syntaxTree);
        List<Token> tokens = new List<Token>();
        Token token;
        do
        {
            token = lexer.Lex();

            if (token.type != SyntaxType.WhiteSpaceToken && token.type != SyntaxType.BadToken)
            {
                tokens.Add(token);
            }
        } while (token.type != SyntaxType.EOFToken);

        this.syntaxTree = syntaxTree;
        this.tokens = tokens.ToArray();
        diagnostics.AddRange(lexer.Diagnostics());
    }

    internal DiagnosticGroup Diagnostics => diagnostics;

    private Token NextToken()
    {
        Token token = current;
        position++;
        return token;
    }

    private Token Peek(int offset)
    {
        int index = position + offset;
        return index < tokens.Length ? tokens[index] : tokens[^1];
    }

    private Token Match(SyntaxType type)
    {
        if (current.type == type)
        {
            return NextToken();
        }

        diagnostics.ReportUnexpectedToken(current.location, current.type, type);
        return new Token(syntaxTree, type, current.position, null, null);
    }

    public CompilationUnit ParseCompilationUnit()
    {
        ImmutableArray<Member> members = ParseMembers();
        Token end = Match(SyntaxType.EOFToken);
        return new CompilationUnit(syntaxTree, members, end);
    }

    private ImmutableArray<Member> ParseMembers()
    {
        ImmutableArray<Member>.Builder members = ImmutableArray.CreateBuilder<Member>();

        while (current.type != SyntaxType.EOFToken)
        {
            Token startToken = current;

            Member member = ParseMember();
            if (member != null)
            {
                members.Add(member);
            }

            if (current == startToken)
            {
                NextToken();
            }
        }

        return members.ToImmutable();
    }

    private Member ParseMember()
    {
        if (current.type == SyntaxType.FunctionKeyword)
        {
            return ParseFunctionDeclaration();
        }
        else if (current.type == SyntaxType.ExternKeyword)
        {
            return PareExternFunctionDeclaration();
        }

        diagnostics.ReportUnexpectedToken(current.location, current.type, SyntaxType.FunctionKeyword);
        return null;
    }


    private Member ParseFunctionDeclaration()
    {
        Token functionKeyword = Match(SyntaxType.FunctionKeyword);
        Token identifier = Match(SyntaxType.IdentifierToken);
        Token lParen = Match(SyntaxType.LParenToken);
        SeparatedSyntaxList<Parameter> parameters = ParseParameters();
        Token rParen = Match(SyntaxType.RParenToken);
        TypeClause type = ParseTypeClause();
        BlockStatement body = (BlockStatement)ParseBlockStatement();
        return new FunctionDeclarationMember(syntaxTree, functionKeyword, identifier, lParen, parameters, rParen,
            type, body);
    }

    private Member PareExternFunctionDeclaration()
    {
        Token externKeyword = Match(SyntaxType.ExternKeyword);
        Token functionKeyword = Match(SyntaxType.FunctionKeyword);
        Token identifier = Match(SyntaxType.IdentifierToken);
        Token lParen = Match(SyntaxType.LParenToken);
        SeparatedSyntaxList<Parameter> parameters = ParseParameters();
        Token rParen = Match(SyntaxType.RParenToken);
        TypeClause type = ParseTypeClause();
        Token semicolonToken = Match(SyntaxType.SemicolonToken);

        return new ExternFunctionDeclarationMember(syntaxTree, externKeyword, functionKeyword, identifier, lParen, parameters, rParen, type, semicolonToken);
    }

    private SeparatedSyntaxList<Parameter> ParseParameters()
    {
        ImmutableArray<Node>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();

        while (current.type != SyntaxType.RParenToken &&
               current.type != SyntaxType.EOFToken)
        {
            Parameter expression = ParseParameter();
            nodesAndSeparators.Add(expression);

            if (current.type != SyntaxType.RParenToken)
            {
                Token comma = Match(SyntaxType.CommaToken);
                nodesAndSeparators.Add(comma);
            }
        }

        return new SeparatedSyntaxList<Parameter>(nodesAndSeparators.ToImmutable());
    }

    private Parameter ParseParameter()
    {
        Token identifier = Match(SyntaxType.IdentifierToken);
        TypeClause type = ParseTypeClause();
        return new Parameter(syntaxTree, identifier, type);
    }

    private Statement ParseStatement()
    {
        Statement statement;
        switch (current.type)
        {
            case SyntaxType.LBraceToken:
                statement = ParseBlockStatement();
                break;
            case SyntaxType.LetKeyword or SyntaxType.ConstKeyword:
                statement = ParseVariableDeclaration();
                break;
            case SyntaxType.IfKeyword:
                statement = ParseIfStatement();
                break;
            case SyntaxType.WhileKeyword:
                statement = ParseWhileStatement();
                break;
            case SyntaxType.DoKeyword:
                statement = ParseDoWhileStatement();
                break;
            case SyntaxType.ForKeyword:
                statement = ParseForStatement();
                break;
            case SyntaxType.BreakKeyword:
                statement = ParseBreakStatement();
                break;
            case SyntaxType.ContinueKeyword:
                statement = ParseContinueStatement();
                break;
            case SyntaxType.ReturnKeyword:
                statement = ParseReturnStatement();
                break;
            default:
                statement = ParseExpressionStatement();
                break;
        }

        if (statement.endToken != SyntaxType.AnyToken)
        {
            Match(statement.endToken);
        }

        return statement;
    }

    private Statement ParseReturnStatement()
    {
        Token keyword = Match(SyntaxType.ReturnKeyword);
        Expression expression = null;
        if (current.type != SyntaxType.SemicolonToken)
        {
            expression = ParseExpression();
        }

        return new ReturnStatement(syntaxTree, keyword, expression);
    }

    private Statement ParseContinueStatement()
    {
        Token token = Match(SyntaxType.ContinueKeyword);
        return new ContinueStatement(syntaxTree, token);
    }

    private Statement ParseBreakStatement()
    {
        Token token = Match(SyntaxType.BreakKeyword);
        return new BreakStatement(syntaxTree, token);
    }

    private Statement ParseForStatement()
    {
        Token keyword = Match(SyntaxType.ForKeyword);
        Token lParen = Match(SyntaxType.LParenToken);
        Token identifier = Match(SyntaxType.IdentifierToken);
        Token equalsToken = Match(SyntaxType.EqualsToken);
        Expression startExpression = ParseExpression();
        Token toKeyword = Match(SyntaxType.ToKeyword);
        Expression endExpression = ParseExpression();
        Token rParen = Match(SyntaxType.RParenToken);
        Statement body = ParseStatement();

        return new ForStatement(syntaxTree, keyword, lParen, identifier, equalsToken, startExpression, toKeyword,
            endExpression,
            rParen, body);
    }

    private Statement ParseDoWhileStatement()
    {
        Token doKeyword = Match(SyntaxType.DoKeyword);
        Statement body = ParseStatement();
        Token whileKeyword = Match(SyntaxType.WhileKeyword);
        Token lParen = Match(SyntaxType.LParenToken);
        Expression condition = ParseExpression();
        Token rParent = Match(SyntaxType.RParenToken);
        return new DoWhileStatement(syntaxTree, doKeyword, body, whileKeyword, lParen, condition, rParent);
    }

    private Statement ParseWhileStatement()
    {
        Token keyword = Match(SyntaxType.WhileKeyword);
        Token lParen = Match(SyntaxType.LParenToken);
        Expression condition = ParseExpression();
        Token rParen = Match(SyntaxType.RParenToken);
        Statement body = ParseStatement();

        return new WhileStatement(syntaxTree, keyword, lParen, condition, rParen, body);
    }

    private Statement ParseIfStatement()
    {
        Token keyword = Match(SyntaxType.IfKeyword);
        Token lParen = Match(SyntaxType.LParenToken);
        Expression condition = ParseExpression();
        Token rParen = Match(SyntaxType.RParenToken);
        Statement statement = ParseStatement();
        ElseClause elseClause = ParseElseClause();

        return new IfStatement(syntaxTree, keyword, lParen, condition, rParen, statement, elseClause);
    }

    private ElseClause ParseElseClause()
    {
        if (current.type != SyntaxType.ElseKeyword)
        {
            return null;
        }

        Token keyword = NextToken();
        Statement statement = ParseStatement();
        return new ElseClause(syntaxTree, keyword, statement);
    }

    private Statement ParseVariableDeclaration()
    {
        SyntaxType expected =
            current.type == SyntaxType.LetKeyword ? SyntaxType.LetKeyword : SyntaxType.ConstKeyword;
        Token keyword = Match(expected);
        Token identifier = Match(SyntaxType.IdentifierToken);
        TypeClause typeClause = ParseTypeClause();
        Token equals = Match(SyntaxType.EqualsToken);
        Expression initializer = ParseExpression();
        return new VariableDeclarationStatement(syntaxTree, keyword, identifier, typeClause, equals, initializer);
    }

    private TypeClause ParseTypeClause()
    {
        Token colon = Match(SyntaxType.ColonToken);
        Token identifier = Match(SyntaxType.IdentifierToken);
        return new TypeClause(syntaxTree, colon, identifier);
    }

    private Statement ParseBlockStatement()
    {
        ImmutableArray<Statement>.Builder statements = ImmutableArray.CreateBuilder<Statement>();

        Token lBrace = Match(SyntaxType.LBraceToken);
        while (current.type != SyntaxType.EOFToken && current.type != SyntaxType.RBraceToken)
        {
            Token startToken = current;

            Statement statement = ParseStatement();
            statements.Add(statement);

            if (current == startToken)
            {
                NextToken();
            }
        }

        Token rBrace = Match(SyntaxType.RBraceToken);

        return new BlockStatement(syntaxTree, lBrace, statements.ToImmutable(), rBrace);
    }

    private Statement ParseExpressionStatement()
    {
        Expression expression = ParseExpression();
        return new ExpressionStatement(syntaxTree, expression);
    }

    private Expression ParseExpression()
    {
        return ParseAssignmentExpression();
    }

    private Expression ParseAssignmentExpression()
    {
        if (current.type == SyntaxType.IdentifierToken)
        {
            switch (Peek(1).type)
            {
                case SyntaxType.PlusEqualsToken:
                case SyntaxType.MinusEqualsToken:
                case SyntaxType.StarEqualsToken:
                case SyntaxType.DoubleStarEqualsToken:
                case SyntaxType.SlashEqualsToken:
                case SyntaxType.AndEqualsToken:
                case SyntaxType.SplitEqualsToken:
                case SyntaxType.HatEqualsToken:
                case SyntaxType.PercentEqualsToken:
                case SyntaxType.EqualsToken:
                    Token identifierToken = NextToken();
                    Token operatorToken = NextToken();
                    Expression right = ParseAssignmentExpression();
                    return new AssignmentExpression(syntaxTree, identifierToken, operatorToken, right);
            }
        }

        return ParseBinaryExpression();
    }


    private Expression ParseBinaryExpression(int parentPrecedence = 0)
    {
        Expression left;
        int unaryIndex = GetUnaryOperatorPrecedence(current.type);
        if (unaryIndex != 0 && unaryIndex >= parentPrecedence)
        {
            Token operatorToken = NextToken();
            Expression right = ParseBinaryExpression(unaryIndex);
            left = new UnaryExpression(syntaxTree, operatorToken, right);
        }
        else
        {
            left = ParseBaseExpression();
        }

        while (true)
        {
            int index = GetBinaryOperatorPrecedence(current.type);
            if (index == 0 || index <= parentPrecedence)
            {
                break;
            }

            Token operatorToken = NextToken();
            Expression right = ParseBinaryExpression(index);
            left = new BinaryExpression(syntaxTree, left, operatorToken, right);
        }

        return left;
    }


    private Expression ParseBaseExpression()
    {
        switch (current.type)
        {
            case SyntaxType.LParenToken:
                return ParseParenExpression();
            case SyntaxType.FalseKeyword:
            case SyntaxType.TrueKeyword:
                return ParseBooleanLiteral();
            case SyntaxType.NumberToken:
                return ParseNumberLiteral();
            case SyntaxType.StringToken:
                return ParseStringLiteral();
            case SyntaxType.IdentifierToken:
            default:
                return ParseNameOrCallExpression();
        }
    }

    private Expression ParseNameOrCallExpression()
    {
        if (Peek(0).type == SyntaxType.IdentifierToken && Peek(1).type == SyntaxType.LParenToken)
        {
            return ParseCallExpression();
        }
        else
        {
            return ParseNameExpression();
        }
    }

    private Expression ParseCallExpression()
    {
        Token identifier = Match(SyntaxType.IdentifierToken);
        Token lParen = Match(SyntaxType.LParenToken);
        SeparatedSyntaxList<Expression> arguments = ParseArguments();
        Token rParen = Match(SyntaxType.RParenToken);
        return new CallExpression(syntaxTree, identifier, lParen, arguments, rParen);
    }

    private SeparatedSyntaxList<Expression> ParseArguments()
    {
        ImmutableArray<Node>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();

        while (current.type != SyntaxType.RParenToken &&
               current.type != SyntaxType.EOFToken)
        {
            Expression expression = ParseExpression();
            nodesAndSeparators.Add(expression);

            if (current.type != SyntaxType.RParenToken)
            {
                Token comma = Match(SyntaxType.CommaToken);
                nodesAndSeparators.Add(comma);
            }
        }

        return new SeparatedSyntaxList<Expression>(nodesAndSeparators.ToImmutable());
    }

    private Expression ParseNameExpression()
    {
        Token identifier = Match(SyntaxType.IdentifierToken);
        return new NameExpression(syntaxTree, identifier);
    }

    private Expression ParseNumberLiteral()
    {
        Token numberToken = Match(SyntaxType.NumberToken);
        return new LiteralExpression(syntaxTree, numberToken, numberToken.value);
    }

    private Expression ParseStringLiteral()
    {
        Token stringToken = Match(SyntaxType.StringToken);
        return new LiteralExpression(syntaxTree, stringToken, stringToken.value);
    }

    private Expression ParseBooleanLiteral()
    {
        bool isTrue = current.type == SyntaxType.TrueKeyword;
        Token token = isTrue ? Match(SyntaxType.TrueKeyword) : Match(SyntaxType.FalseKeyword);
        return new LiteralExpression(syntaxTree, token, isTrue);
    }

    private Expression ParseParenExpression()
    {
        Token left = Match(SyntaxType.LParenToken);
        Expression expression = ParseExpression();
        Token right = Match(SyntaxType.RParenToken);
        return new ParenExpression(syntaxTree, left, expression, right);
    }
}