using System;
using System.Collections.Generic;
using System.IO;
using CodeFusion.VM;

namespace CodeFusion.ASM.Compiling;

public class Compiler
{
    private readonly Inst[][] insts;
    private readonly Dictionary<Word, ushort> pool;
    private readonly ulong entryPoint;
    private readonly uint symbolSize;

    public Compiler(Inst[][] insts, Dictionary<Word, ushort> pool, ulong entryPoint, uint symbolSize = 0)
    {
        this.insts = insts;
        this.pool = pool;
        this.entryPoint = entryPoint;
        this.symbolSize = symbolSize;
    }

    public void WriteHeader(ref BinaryWriter writer, byte flags)
    {
        ulong size = 0;
        foreach (Inst[] insts in this.insts)
        {
            size += Convert.ToUInt64(insts.Length);
        }
        writer.Write((byte)'.');
        writer.Write((byte)'C');
        writer.Write((byte)'F');
        writer.Write(Metadata.CURRENT_VERSION);
        writer.Write(flags);
        writer.Write(entryPoint);
        writer.Write((ushort)pool.Count);
        writer.Write(size);
        writer.Write(symbolSize);
    }

    public void WriteRelocatableHeader(ref BinaryWriter writer, ushort symbolCount, ushort missingCount, ushort addressCount)
    {
        writer.Write(symbolCount);
        writer.Write(missingCount);
        writer.Write(addressCount);
    }

    public void WritePool(ref BinaryWriter writer)
    {
        foreach (KeyValuePair<Word, ushort> item in pool)
        {
            writer.Write(item.Key.asU64);
            writer.Write(item.Value);
        }
    }

    public void WriteProgram(ref BinaryWriter writer)
    {
        foreach (Inst[] insts in this.insts)
        {
            foreach (Inst inst in insts)
            {
                writer.Write(inst.opcode);
                if (!Opcode.HasOperand(inst.opcode))
                {
                    continue;
                }

                byte bytes = 0;
                if (inst.operand.asU64 == 0)
                {
                    writer.Write((byte)0);
                    continue;
                }

                bytes++;
                ulong maxValue = 0xFF;

                while (inst.operand.asU64 > maxValue)
                {
                    maxValue = maxValue << 8 | 0xFF;
                    bytes++;
                }

                writer.Write(bytes);
                byte[] operand = BitConverter.GetBytes(inst.operand.asU64);
                for (int j = 0; j < bytes; j++)
                {
                    writer.Write(operand[j]);
                }
            }
        }
    }

    public void WriteSymbols(ref BinaryWriter writer, Dictionary<string, ulong> labels)
    {
        foreach (KeyValuePair<string, ulong> label in labels)
        {
            writer.Write((ushort)label.Key.Length);

            foreach (char c in label.Key)
            {
                writer.Write((byte)c);
            }
            writer.Write(label.Value);
        }
    }

    public void WriteSymbols(ref BinaryWriter writer, Dictionary<ulong, string> labels)
    {
        foreach (KeyValuePair<ulong, string> label in labels)
        {
            writer.Write((ushort)label.Value.Length);

            foreach (char c in label.Value)
            {
                writer.Write((byte)c);
            }
            writer.Write(label.Key);
        }
    }

    public void WriteAddresses(ref BinaryWriter writer, ulong[] addresses)
    {
        foreach (ulong address in addresses)
        {
            writer.Write(address);
        }
    }
}