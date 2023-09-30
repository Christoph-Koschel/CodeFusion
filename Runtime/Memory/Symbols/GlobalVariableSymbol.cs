namespace IllusionScript.Runtime.Memory.Symbols;
public class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type)
    {
    }

    public override SymbolType symbolType => SymbolType.GlobalVariable;
}