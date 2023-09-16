using System;
using System.Collections.Generic;
using CodeFusion.VM;

namespace CodeFusion.Format;

public class ProgramSection : Section
{
    public List<Inst> program = new List<Inst>();

    public ProgramSection()
    {
        type = TYPE_PROGRAM;
        lenght = 0;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.Add(type);

        lenght = 0;
        foreach (Inst inst in program)
        {
            lenght++;
            if (!Opcode.HasOperand(inst.opcode))
            {
                continue;
            }

            if (inst.operand.asU64 == 0)
            {
                lenght++;
                continue;
            }

            lenght += 2;
            ulong maxValue = 0xFF;
            while (inst.operand.asU64 > maxValue)
            {
                maxValue = maxValue << 8 | 0xFF;
                lenght++;
            }
        }
        bytes.AddRange(BitConverter.GetBytes(lenght));

        foreach (Inst inst in program)
        {
            bytes.Add(inst.opcode);
            if (!Opcode.HasOperand(inst.opcode))
            {
                continue;
            }

            byte size = 0;
            if (inst.operand.asU64 == 0)
            {
                bytes.Add(0);
                continue;
            }

            size++;
            ulong maxValue = 0xFF;

            while (inst.operand.asU64 > maxValue)
            {
                maxValue = maxValue << 8 | 0xFF;
                size++;
            }

            bytes.Add(size);
            byte[] operand = BitConverter.GetBytes(inst.operand.asU64);
            for (int j = 0; j < size; j++)
            {
                bytes.Add(operand[j]);
            }
        }

        return bytes.ToArray();
    }
}