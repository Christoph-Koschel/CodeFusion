using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Binding.Nodes.Statements;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Emitting;
using IllusionScript.Runtime.Memory;
using IllusionScript.Runtime.Memory.Symbols;
using IllusionScript.Runtime.Parsing;

namespace IllusionScript.Runtime;

public sealed class Compilation
{
    public readonly ImmutableArray<SyntaxTree> syntaxTrees;
    public ImmutableArray<FunctionSymbol> functions => GlobalScope.functions;
    public FunctionSymbol mainFunction => globalScope.mainFunction;
    public ImmutableArray<Diagnostic> diagnostics => globalScope.diagnostics;
    private GlobalScope globalScope;

    private Compilation(params SyntaxTree[] syntaxTrees)
    {
        this.syntaxTrees = syntaxTrees.ToImmutableArray();
    }

    public static Compilation Create(params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(syntaxTrees);
    }

    internal GlobalScope GlobalScope
    {
        get
        {
            if (globalScope == null)
            {
                GlobalScope scope = Binder.BindGlobalScope(syntaxTrees.ToImmutableArray());
                Interlocked.CompareExchange(ref globalScope, scope, null);
            }

            return globalScope;
        }
    }

    private BoundProgram GetProgram()
    {
        return Binder.BindProgram(GlobalScope);
    }

    public ImmutableArray<Diagnostic> Check()
    {
        BoundProgram program = GetProgram();
        return syntaxTrees.SelectMany(syntaxTree => syntaxTree.diagnostics).Concat(program.diagnostics).ToImmutableArray();
    }

    public ImmutableArray<Diagnostic> Emit(string outputPath)
    {
        BoundProgram program = GetProgram();
        return Emitter.Emit(program, outputPath);
    }
}