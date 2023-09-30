using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public sealed class NameExpression : Expression
{
    public override SyntaxType type => SyntaxType.NameExpression;
    public readonly Token identifier;

    public NameExpression(SyntaxTree syntaxTree, Token identifier)  : base(syntaxTree)
    {
        this.identifier = identifier;
    }
}