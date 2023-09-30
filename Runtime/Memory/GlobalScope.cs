using System.Collections.Immutable;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Memory;

internal sealed class GlobalScope
{
    public readonly ImmutableArray<Diagnostic> diagnostics;
    public readonly FunctionSymbol mainFunction;
    public readonly ImmutableArray<FunctionSymbol> functions;

    public GlobalScope(ImmutableArray<Diagnostic> diagnostics, FunctionSymbol mainFunction,
        ImmutableArray<FunctionSymbol> functions)
    {
        this.diagnostics = diagnostics;
        this.mainFunction = mainFunction;
        this.functions = functions;
    }
}