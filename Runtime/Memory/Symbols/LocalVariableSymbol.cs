namespace IllusionScript.Runtime.Memory.Symbols;
public class LocalVariableSymbol : VariableSymbol
{
    internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type)
    {
    }

    public override SymbolType symbolType => SymbolType.LocalVariable;
}