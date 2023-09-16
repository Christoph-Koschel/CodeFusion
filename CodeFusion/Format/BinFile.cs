using System;
using System.Collections.Generic;
using System.Linq;
using CodeFusion.VM;

namespace CodeFusion.Format;

public class BinFile
{
    public char[] magic;
    public ushort version;
    public byte flags;
    public ulong entryPoint;

    public List<Section> sections = new List<Section>();

    public BinFile()
    {
    }
    public BinFile(Metadata meta)
    {
        this.magic = meta.magic;
        this.version = meta.version;
        this.flags = meta.flags;
        this.entryPoint = meta.entryPoint;
    }
    public BinFile(BinFile file)
    {
        this.magic = file.magic;
        this.version = file.version;
        this.flags = file.flags;
        this.entryPoint = file.entryPoint;
    }

    public void Add(Section section) => sections.Add(section);

    public void AddRange(IEnumerable<Section> sections) => this.sections.AddRange(sections);

    public byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(magic.Select(c => (byte)c));
        bytes.AddRange(BitConverter.GetBytes(version));
        bytes.Add(flags);
        bytes.AddRange(BitConverter.GetBytes(entryPoint));
        bytes.Add((byte)sections.Count);
        bytes.AddRange(sections.SelectMany(section => section.ToBytes()));
        return bytes.ToArray();
    }
}