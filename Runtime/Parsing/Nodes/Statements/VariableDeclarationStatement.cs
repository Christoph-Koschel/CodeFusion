using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Statements;

public sealed class VariableDeclarationStatement : Statement
{
    public readonly Token keyword;
    public readonly Token identifier;
    public readonly TypeClause typeClause;
    public readonly Token equalsToken;
    public readonly Expression initializer;

    public VariableDeclarationStatement(SyntaxTree syntaxTree, Token keyword, Token identifier,
        TypeClause typeClause, Token equalsToken, Expression initializer) : base(syntaxTree)
    {
        this.keyword = keyword;
        this.identifier = identifier;
        this.typeClause = typeClause;
        this.equalsToken = equalsToken;
        this.initializer = initializer;
    }

    public override SyntaxType type => SyntaxType.VariableDeclarationStatement;

    public override SyntaxType endToken => SyntaxType.SemicolonToken;
}