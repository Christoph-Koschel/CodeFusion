using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public sealed class DoWhileStatement : Statement
{
    public readonly Token doKeyword;
    public readonly Statement body;
    public readonly Token whileKeyword;
    public readonly Token lParen;
    public readonly Expression condition;
    public readonly Token rParent;

    public DoWhileStatement(SyntaxTree syntaxTree, Token doKeyword, Statement body, Token whileKeyword,
        Token lParen, Expression condition, Token rParent) : base(syntaxTree)
    {
        this.doKeyword = doKeyword;
        this.body = body;
        this.whileKeyword = whileKeyword;
        this.lParen = lParen;
        this.condition = condition;
        this.rParent = rParent;
    }

    public override SyntaxType type => SyntaxType.DoWhileStatement;

    public override SyntaxType endToken => SyntaxType.SemicolonToken;
}