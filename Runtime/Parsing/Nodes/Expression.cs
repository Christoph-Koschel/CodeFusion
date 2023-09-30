namespace IllusionScript.Runtime.Parsing.Nodes;

public abstract class Expression : Node
{
    protected Expression(SyntaxTree syntaxTree) : base(syntaxTree)
    {
    }
}