using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeFusion.VM;

namespace CodeFusion.Test;

public class ProgramMaker
{
    private static VmCodeFusion cf;
    private static Dictionary<string, ulong> labels = new Dictionary<string, ulong>();
    private static Dictionary<ulong, string> unresolvedLabels = new Dictionary<ulong, string>();

    private static Dictionary<ulong, ushort> CreatePool()
    {
        Dictionary<ulong, ushort> dictionary = new Dictionary<ulong, ushort>();
        dictionary[Label("entry").asU64] = 12;
        return dictionary;
    }

    private static void CreateProgram()
    {
        PushLabel("entry");
        PushInst(new Inst(Opcode.MALLOC_POOL, Label("entry")));
        PushInst(new Inst(Opcode.PUSH, new Word(40)));
        PushInst(new Inst(Opcode.PUSH, new Word(4)));
        PushInst(new Inst(Opcode.STORE, Word.zero));
        PushInst(new Inst(Opcode.PUSH, new Word(4)));
        PushInst(new Inst(Opcode.LOAD, Word.zero));
        PushInst(new Inst(Opcode.POP));
        PushInst(new Inst(Opcode.PUSH_PTR, Word.zero));
        PushInst(new Inst(Opcode.DUP, Word.zero));
        PushInst(new Inst(Opcode.PUSH, new Word(30)));
        PushInst(new Inst(Opcode.STORE_PTR, new Word(4)));
        PushInst(new Inst(Opcode.LOAD_PTR, new Word(4)));
        PushInst(new Inst(Opcode.PUSH, new Word(4)));
        PushInst(new Inst(Opcode.LOAD, Word.zero));
        PushInst(new Inst(Opcode.POP));
        PushInst(new Inst(Opcode.POP));
        PushInst(new Inst(Opcode.PUSH, new Word(16)));
        PushInst(new Inst(Opcode.PUSH_ARRAY));
        PushInst(new Inst(Opcode.PUSH, new Word(8)));
        PushInst(new Inst(Opcode.STORE, new Word(4)));
        PushInst(new Inst(Opcode.PUSH, new Word(8)));
        PushInst(new Inst(Opcode.LOAD, new Word(4)));
        PushInst(new Inst(Opcode.PUSH, Word.zero));
        PushInst(new Inst(Opcode.PUSH, new Word(64)));
        PushInst(new Inst(Opcode.STORE_ARRAY, new Word(4)));
        PushInst(new Inst(Opcode.PUSH, new Word(8)));
        PushInst(new Inst(Opcode.LOAD, new Word(4)));
        PushInst(new Inst(Opcode.PUSH, Word.zero));
        PushInst(new Inst(Opcode.LOAD_ARRAY, new Word(4)));
        PushInst(new Inst(Opcode.FREE_POOL));
    }

    private static void PushInst(Inst inst)
    {
        cf.program[cf.programSize++] = inst;
    }

    private static void PushLabel(string name)
    {
        labels[name] = cf.programSize;
    }

    private static Word Label(string name)
    {
        if (labels.TryGetValue(name, out ulong label))
        {
            return new Word(label);
        }

        unresolvedLabels.Add(cf.programSize, name);
        return Word.zero;
    }

    private static void WriteHeader(ref BinaryWriter writer, Metadata metadata)
    {
        writer.Write(Encoding.ASCII.GetBytes(metadata.magic));
        writer.Write(BitConverter.GetBytes(metadata.version));
        writer.Write(BitConverter.GetBytes(metadata.entryPoint));
        writer.Write(BitConverter.GetBytes(metadata.poolSize));
        writer.Write(BitConverter.GetBytes(metadata.programSize));
    }

    private static void WritePool(ref BinaryWriter writer, Dictionary<ulong, ushort> pool)
    {
        foreach (KeyValuePair<ulong, ushort> pair in pool)
        {
            writer.Write(BitConverter.GetBytes(pair.Key));
            writer.Write(BitConverter.GetBytes(pair.Value));
        }
    }

    private static void WriteProgram(ref BinaryWriter writer, ref VmCodeFusion cf)
    {
        for (ulong i = 0; i < cf.programSize; i++)
        {
            writer.Write(cf.program[i].opcode);
            if (!Opcode.HasOperand(cf.program[i].opcode))
            {
                continue;
            }

            byte bytes = 0;
            if (cf.program[i].operand.asU64 == 0)
            {
                writer.Write((byte)0);
                continue;
            }

            bytes++;
            ulong maxValue = 0xFF;

            while (cf.program[i].operand.asU64 > maxValue)
            {
                maxValue = maxValue << 8 | 0xFF;
                bytes++;
            }

            writer.Write(bytes);
            byte[] operand = BitConverter.GetBytes(cf.program[i].operand.asU64);
            for (int j = 0; j < bytes; j++)
            {
                writer.Write(operand[j]);
            }
        }
    }

    public static void Make(string file)
    {
        BinaryWriter writer = new BinaryWriter(new FileStream(file, FileMode.OpenOrCreate));
        cf = new VmCodeFusion();
        labels.Clear();
        unresolvedLabels.Clear();
        CreateProgram();
        Dictionary<ulong, ushort> pool = CreatePool();

        foreach (KeyValuePair<ulong, string> unresolvedLabel in unresolvedLabels)
        {
            cf.program[unresolvedLabel.Key].operand = Label(unresolvedLabel.Value);
        }

        Metadata metadata = new Metadata
        {
            magic = ".CF".ToCharArray(),
            version = Metadata.CURRENT_VERSION,
            entryPoint = Label("entry").asU64,
            poolSize = (ushort)pool.Count,
            programSize = cf.programSize
        };

        Console.WriteLine(metadata.programSize);
        Console.WriteLine(metadata.poolSize);

        WriteHeader(ref writer, metadata);
        WritePool(ref writer, pool);
        WriteProgram(ref writer, ref cf);
        writer.Close();
    }
}