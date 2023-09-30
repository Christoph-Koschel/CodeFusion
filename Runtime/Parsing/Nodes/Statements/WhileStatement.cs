using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public sealed class WhileStatement : Statement
{
    public readonly Token keyword;
    public readonly Token lParen;
    public readonly Expression condition;
    public readonly Token rParen;
    public readonly Statement body;

    public WhileStatement(SyntaxTree syntaxTree, Token keyword, Token lParen, Expression condition, Token rParen,
        Statement body) : base(syntaxTree)
    {
        this.keyword = keyword;
        this.lParen = lParen;
        this.condition = condition;
        this.rParen = rParen;
        this.body = body;
    }

    public override SyntaxType type => SyntaxType.WhileStatement;
    public override SyntaxType endToken => SyntaxType.AnyToken;
}