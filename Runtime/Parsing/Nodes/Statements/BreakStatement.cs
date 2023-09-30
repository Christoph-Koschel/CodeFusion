using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public class BreakStatement : Statement
{
    public readonly Token keyword;

    public BreakStatement(SyntaxTree syntaxTree, Token keyword) : base(syntaxTree)
    {
        this.keyword = keyword;
    }

    public override SyntaxType type => SyntaxType.BreakStatement;
    public override SyntaxType endToken => SyntaxType.SemicolonToken;
}