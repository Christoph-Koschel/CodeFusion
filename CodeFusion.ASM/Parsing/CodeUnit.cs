using System.Collections.Generic;
using CodeFusion.ASM.Lexing;
using CodeFusion.VM;

namespace CodeFusion.ASM.Parsing;

public struct CodeUnit
{
    public readonly SourceFile source;
    public readonly List<Inst> insts;
    public readonly List<byte> memory;
    public readonly Dictionary<string, ulong> labels;
    public readonly Dictionary<string, ulong> memoryLabels;
    public readonly Dictionary<string, ulong> variables;
    public readonly Dictionary<ulong, Token> unresolved;
    public readonly Dictionary<Word, ushort> pool;
    public readonly List<ulong> lookups;
    public readonly List<ulong> memoryLookups;
    public readonly ulong addressOffset;

    public CodeUnit(SourceFile source, ulong addressOffset)
    {
        this.source = source;
        this.insts = new List<Inst>();
        this.memory = new List<byte>();
        this.labels = new Dictionary<string, ulong>();
        this.memoryLabels = new Dictionary<string, ulong>();
        this.variables = new Dictionary<string, ulong>();
        this.unresolved = new Dictionary<ulong, Token>();
        this.pool = new Dictionary<Word, ushort>();
        this.lookups = new List<ulong>();
        this.memoryLookups = new List<ulong>();
        this.addressOffset = addressOffset;
    }
}