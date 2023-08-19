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

    public Compiler(Inst[][] insts, Dictionary<Word, ushort> pool, ulong entryPoint)
    {
        this.insts = insts;
        this.pool = pool;
        this.entryPoint = entryPoint;
    }

    public void WriteTo(BinaryWriter writer)
    {
        WriteHeader(ref writer);
        WritePool(ref writer);
        WriteProgram(ref writer);
    }

    private void WriteHeader(ref BinaryWriter writer)
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
        writer.Write(entryPoint);
        writer.Write((ushort)pool.Count);
        writer.Write(size);
    }

    private void WritePool(ref BinaryWriter writer)
    {
        foreach (KeyValuePair<Word, ushort> item in pool)
        {
            writer.Write(item.Key.asU64);
            writer.Write(item.Value);
        }
    }

    private void WriteProgram(ref BinaryWriter writer)
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
}