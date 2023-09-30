namespace IllusionScript.Runtime.Parsing.Nodes.Members;

public sealed class StatementMember : Member
{
    public readonly Statement statement;

    public StatementMember(SyntaxTree syntaxTree, Statement statement) : base(syntaxTree)
    {
        this.statement = statement;
    }

    public override SyntaxType type => SyntaxType.StatementMember;
}