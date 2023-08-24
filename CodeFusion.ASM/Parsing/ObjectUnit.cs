using System.Collections.Generic;
using CodeFusion.VM;

namespace CodeFusion.ASM.Parsing;

public struct ObjectUnit
{
    public string path = string.Empty;
    public readonly List<Inst> insts;
    public readonly Dictionary<string, ulong> labels;
    public readonly Dictionary<ulong, string> unresolved;
    public readonly Dictionary<Word, ushort> pool;
    public readonly List<ulong> addresses;

    public ObjectUnit() {
        this.insts = new List<Inst>();
        this.labels = new Dictionary<string, ulong>();
        this.unresolved = new Dictionary<ulong, string>();
        this.pool = new Dictionary<Word, ushort>();
        this.addresses = new List<ulong>();
    }
}