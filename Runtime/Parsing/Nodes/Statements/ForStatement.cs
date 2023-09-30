using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public sealed class ForStatement : Statement
{
    public readonly Token keyword;
    public readonly Token lParen;
    public readonly Token identifier;
    public readonly Token equalsToken;
    public readonly Expression startExpression;
    public readonly Token toKeyword;
    public readonly Expression endExpression;
    public readonly Token rParen;
    public readonly Statement body;

    public ForStatement(SyntaxTree syntaxTree, Token keyword, Token lParen, Token identifier, Token equalsToken,
        Expression startExpression, Token toKeyword, Expression endExpression, Token rParen, Statement body) : base(
        syntaxTree)
    {
        this.keyword = keyword;
        this.lParen = lParen;
        this.identifier = identifier;
        this.equalsToken = equalsToken;
        this.startExpression = startExpression;
        this.toKeyword = toKeyword;
        this.endExpression = endExpression;
        this.rParen = rParen;
        this.body = body;
    }

    public override SyntaxType type => SyntaxType.ForStatement;
    public override SyntaxType endToken => SyntaxType.AnyToken;
}