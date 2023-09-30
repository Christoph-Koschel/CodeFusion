namespace IllusionScript.Runtime.Memory.Symbols;

public sealed class ParameterSymbol : LocalVariableSymbol
{
    public ParameterSymbol(string name, TypeSymbol type) : base(name, true, type)
    {
    }

    public override SymbolType symbolType => SymbolType.Parameter;
}