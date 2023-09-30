using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public class ContinueStatement : Statement
{
    public readonly Token keyword;

    public ContinueStatement(SyntaxTree syntaxTree, Token keyword) : base(syntaxTree)
    {
        this.keyword = keyword;
    }

    public override SyntaxType type => SyntaxType.ContinueStatement;
    public override SyntaxType endToken => SyntaxType.SemicolonToken;
}