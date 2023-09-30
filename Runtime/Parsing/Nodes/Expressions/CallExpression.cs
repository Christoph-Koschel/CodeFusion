using IllusionScript.Runtime.Extension;
using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes.Expressions;

public sealed class CallExpression : Expression
{
    public readonly Token identifier;
    public readonly Token lParen;
    public readonly SeparatedSyntaxList<Expression> arguments;
    public readonly Token rParen;

    public CallExpression(SyntaxTree syntaxTree, Token identifier, Token lParen, SeparatedSyntaxList<Expression> arguments, Token rParen)  : base(syntaxTree)
    {
        this.identifier = identifier;
        this.lParen = lParen;
        this.arguments = arguments;
        this.rParen = rParen;
    }
        
    public override SyntaxType type => SyntaxType.CallExpression;
}