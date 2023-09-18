using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeFusion.VM;
using CodeFusion.Ext;

namespace CodeFusion.Format;

public class FinFile
{
    public char[] magic;
    public ushort version;
    public byte flags;
    public ulong entryPoint;
    public ulong poolCount;
    public ulong programCount;
    public ulong symbolCount;
    public ulong memoryCount;


    public FinFile(BinFile file)
    {
        this.magic = file.magic;
        this.version = file.version;
        this.flags = file.flags;
        this.entryPoint = file.entryPoint;
    }

    public MemoryStream GetBytes(IEnumerable<Section> sections)
    {
        this.poolCount = 0;
        this.programCount = 0;
        this.symbolCount = 0;
        this.memoryCount = 0;

        MemoryStream programStream = new MemoryStream();
        MemoryStream poolStream = new MemoryStream();
        MemoryStream symbolStream = new MemoryStream();
        MemoryStream memoryStream = new MemoryStream();

        foreach (Section section in sections)
        {
            if (section.type == Section.TYPE_POOL)
            {
                PoolSection poolSection = (PoolSection)section;
                poolCount += (ulong)poolSection.pool.Count;
                foreach ((Word word, ushort size) in poolSection.pool)
                {
                    poolStream.Write(BitConverter.GetBytes(word.asU64));
                    poolStream.Write(BitConverter.GetBytes(size));
                }
            }
            else if (section.type == Section.TYPE_PROGRAM)
            {
                ProgramSection programSection = (ProgramSection)section;
                programCount += (ulong)programSection.program.Count;
                foreach (Inst inst in programSection.program)
                {
                    programStream.Write(inst.opcode);
                    if (!Opcode.HasOperand(inst.opcode))
                    {
                        continue;
                    }

                    byte size = 0;
                    if (inst.operand.asU64 == 0)
                    {
                        programStream.Write(0);
                        continue;
                    }

                    size++;
                    ulong maxValue = 0xFF;

                    while (inst.operand.asU64 > maxValue)
                    {
                        maxValue = maxValue << 8 | 0xFF;
                        size++;
                    }

                    programStream.Write(size);
                    byte[] operand = BitConverter.GetBytes(inst.operand.asU64);
                    for (int j = 0; j < size; j++)
                    {
                        programStream.Write(operand[j]);
                    }
                }
            }
            else if (section.type == Section.TYPE_SYMBOL)
            {
                SymbolSection symbolSection = (SymbolSection)section;
                symbolCount += (ulong)symbolSection.pool.Count;
                foreach ((string name, ulong value) in symbolSection.pool)
                {
                    symbolStream.Write(BitConverter.GetBytes((ushort)name.Length));
                    symbolStream.Write(name.Select(c => (byte)c).ToArray());
                    symbolStream.Write(BitConverter.GetBytes(value));
                }
            }
            else if (section.type == Section.TYPE_MEMORY)
            {
                MemorySection memorySection = (MemorySection)section;
                memoryCount += (ulong)memorySection.data.Count;
                memoryStream.Write(memorySection.data.ToArray());
            }
        }

        MemoryStream result = new MemoryStream();
        result.Write(magic.Select(m => (byte)m).ToArray());
        result.Write(BitConverter.GetBytes(version));
        result.Write(flags);
        result.Write(BitConverter.GetBytes(entryPoint));
        result.Write(BitConverter.GetBytes(poolCount));
        result.Write(BitConverter.GetBytes(programCount));
        result.Write(BitConverter.GetBytes(symbolCount));
        result.Write(BitConverter.GetBytes(memoryCount));
        poolStream.WriteTo(result);
        programStream.WriteTo(result);
        symbolStream.WriteTo(result);
        memoryStream.WriteTo(result);

        poolStream.Close();
        poolStream.Dispose();

        programStream.Close();
        programStream.Dispose();

        symbolStream.Close();
        symbolStream.Dispose();

        memoryStream.Close();
        memoryStream.Dispose();

        return result;
    }
}