using System.Collections.Generic;
using CodeFusion.ASM.Lexing;
using CodeFusion.VM;

namespace CodeFusion.ASM.Parsing;

public struct CodeUnit
{
    public readonly SourceFile source;
    public readonly List<Inst> insts;
    public readonly Dictionary<string, ulong> labels;
    public readonly Dictionary<ulong, Token> unresolved;
    public readonly Dictionary<Word, ushort> pool;
    public readonly ulong addressOffset;

    public CodeUnit(SourceFile source, ulong addressOffset)
    {
        this.source = source;
        this.insts = new List<Inst>();
        this.labels = new Dictionary<string, ulong>();
        this.unresolved = new Dictionary<ulong, Token>();
        this.pool = new Dictionary<Word, ushort>();
        this.addressOffset = addressOffset;
    }
}
