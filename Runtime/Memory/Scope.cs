using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Memory;

internal sealed class Scope
{
    public readonly Scope parent;
    private Dictionary<string, Symbol> symbols;

    public Scope(Scope parent)
    {
        this.parent = parent;
        symbols = new Dictionary<string, Symbol>();
    }

    public bool TryLookupVariable(string name, out VariableSymbol variable) => TryLookupSymbol(name, out variable);

    public bool TryDeclareVariable(VariableSymbol variable) => TryDeclareSymbol(variable);

    public bool TryLookupFunction(string name, out FunctionSymbol function) => TryLookupSymbol(name, out function);

    public bool TryDeclareFunction(FunctionSymbol function) => TryDeclareSymbol(function);

    private bool TryLookupSymbol<T>(string name, out T symbol)
        where T : Symbol
    {
        symbol = null;
        if (symbols.TryGetValue(name, out Symbol declaredSymbol))
        {
            if (declaredSymbol is T matching)
            {
                symbol = matching;
                return true;
            }

            return false;
        }

        if (parent == null)
        {
            return false;
        }

        return parent.TryLookupSymbol(name, out symbol);
    }

    private bool TryDeclareSymbol<T>(T symbol)
        where T : Symbol
    {
        if (symbols.ContainsKey(symbol.name))
        {
            return false;
        }

        symbols.Add(symbol.name, symbol);
        return true;
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables() => GetDeclaredSymbols<VariableSymbol>();

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();

    private ImmutableArray<T> GetDeclaredSymbols<T>()
        where T : Symbol
    {
        return symbols.Values.OfType<T>().ToImmutableArray();
    }
}