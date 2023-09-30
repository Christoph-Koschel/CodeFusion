using System.Collections.Immutable;
using IllusionScript.Runtime.Binding.Nodes.Statements;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Memory;
using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding;

internal sealed class BoundProgram
{
    public readonly GlobalScope globalScope;
    public readonly DiagnosticGroup diagnostics;
    public readonly FunctionSymbol mainFunction;
    public readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies;

    public BoundProgram(
        GlobalScope globalScope, 
        DiagnosticGroup diagnostics, 
        FunctionSymbol mainFunction,
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies
    )
    {
        this.globalScope = globalScope;
        this.diagnostics = diagnostics;
        this.mainFunction = mainFunction;
        this.functionBodies = functionBodies;
    }
}