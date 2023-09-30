using System.Collections.Immutable;
using IllusionScript.Runtime.Parsing.Nodes.Members;

namespace IllusionScript.Runtime.Memory.Symbols;

public sealed class FunctionSymbol : Symbol
{
    public readonly ImmutableArray<ParameterSymbol> parameters;
    public readonly TypeSymbol returnType;
    public readonly FunctionDeclarationMember declaration;

    public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType,
        FunctionDeclarationMember declaration = null) :
        base(name)
    {
        this.parameters = parameters;
        this.returnType = returnType;
        this.declaration = declaration;
    }

    public override SymbolType symbolType => SymbolType.Function;
}