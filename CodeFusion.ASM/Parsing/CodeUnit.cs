using System.Collections.Generic;
using CodeFusion.ASM.Lexing;
using CodeFusion.VM;

namespace CodeFusion.ASM.Parsing;

public struct CodeUnit
{
    public readonly SourceFile source;
    public readonly List<Inst> insts;
    public readonly Dictionary<string, long> labels;
    public readonly Dictionary<long, Token> unresolved;
    public readonly Dictionary<string, int> pool;
    public readonly long addressOffset;

    public CodeUnit(SourceFile source, long addressOffset)
    {
        this.source = source;
        this.insts = new List<Inst>();
        this.labels = new Dictionary<string, long>();
        this.unresolved = new Dictionary<long, Token>();
        this.pool = new Dictionary<string, int>();
        this.addressOffset = addressOffset;
    }
}
