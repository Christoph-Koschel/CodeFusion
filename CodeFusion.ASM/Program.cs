using System;
using System.Collections.Generic;
using System.IO;
using CodeFusion.ASM.Compiling;
using CodeFusion.ASM.Lexing;
using CodeFusion.ASM.Parsing;
using CodeFusion.VM;

namespace CodeFusion.ASM;

class Program
{
    public static void Main(string[] args)
    {
        List<string> files = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-o")
            {
                Options.INSTANCE.output = args[++i];
            }
            else if (args[i] == "-r")
            {
                Options.INSTANCE.relocatable = true;
            }
            else if (args[i] == "-e")
            {
                Options.INSTANCE.entryPoint = args[++i];
            }
            else if (args[i] == "-c")
            {
                Options.INSTANCE.combine = true;
            }
            else
            {
                files.Add(args[i]);
            }
        }

        Options.INSTANCE.files = files.ToArray();

        if (Options.INSTANCE.combine)
        {
            if (Options.INSTANCE.relocatable) {
                CombineObjs();
            } else {
                CombineObjsToExecutable();
            }
        }
        else
        {
            if (Options.INSTANCE.relocatable)
            {
                CompileASMToObject();
            }
            else
            {
                CompileASMToExecutable();
            }
        }
    }

    private static void CombineObjsToExecutable()
    {
        throw new NotImplementedException();
    }

    private static void CombineObjs()
    {
        throw new NotImplementedException();
    }

    private static CodeUnit[] CreateCodeUnits()
    {
        ulong addressOffset = 0;
        List<CodeUnit> units = new List<CodeUnit>();
        foreach (string path in Options.INSTANCE.files)
        {
            Parser parser = new Parser(path);
            CodeUnit unit = parser.ParseUnit(addressOffset);
            addressOffset += Convert.ToUInt32(unit.insts.Count);
            units.Add(unit);
        }

        foreach (CodeUnit unit in units)
        {
            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved)
            {
                foreach (CodeUnit item in units)
                {
                    if (item.labels.TryGetValue(unresolved.Value.text, out ulong value))
                    {
                        Inst inst = unit.insts[(int)unresolved.Key];
                        inst.operand = new Word(value);
                        unit.insts[(int)unresolved.Key] = inst;
                        unit.unresolved.Remove(unresolved.Key);
                        break;
                    }
                    if (item.variables.TryGetValue(unresolved.Value.text, out value))
                    {
                        Inst inst = unit.insts[(int)unresolved.Key];
                        inst.operand = new Word(value);
                        unit.insts[(int)unresolved.Key] = inst;
                        unit.unresolved.Remove(unresolved.Key);
                        break;
                    }
                }
            }
        }

        return units.ToArray();
    }

    private static void CompileASMToExecutable()
    {
        CodeUnit[] units = CreateCodeUnits();
        List<Inst[]> insts = new List<Inst[]>();
        Dictionary<Word, ushort> pool = new Dictionary<Word, ushort>();
        ulong entryPoint = 0;

        foreach (CodeUnit unit in units)
        {
            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved)
            {
                Report.PrintReport(unit.source, unresolved.Value, $"Unresolved label '{unresolved.Value.text}'");
            }
            insts.Add(unit.insts.ToArray());
            foreach (KeyValuePair<Word, ushort> item in unit.pool)
            {
                pool.Add(item.Key, item.Value);
            }

            if (unit.labels.TryGetValue(Options.INSTANCE.entryPoint, out ulong value))
            {
                entryPoint = value;
            }
        }

        foreach (CodeUnit unit in units)
        {
            foreach (Inst inst in unit.insts)
            {
                Console.WriteLine("{0, -10} {1}", Opcode.GetOpcodeName(inst.opcode), inst.operand.asU64);
            }
        }

        if (!Report.sentErros)
        {
            Compiler compiler = new Compiler(insts.ToArray(), pool, entryPoint);
            BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
            compiler.WriteHeader(ref writer, Metadata.EXECUTABLE);
            compiler.WritePool(ref writer);
            compiler.WriteProgram(ref writer);
            writer.Close();
        }
    }

    private static void CompileASMToObject()
    {
        CodeUnit[] units = CreateCodeUnits();
        List<Inst[]> insts = new List<Inst[]>();
        Dictionary<Word, ushort> pool = new Dictionary<Word, ushort>();
        Dictionary<string, ulong> labels = new Dictionary<string, ulong>();
        Dictionary<string, ulong> missing = new Dictionary<string, ulong>();
        List<ulong> addresses = new List<ulong>();

        foreach (CodeUnit unit in units)
        {
            foreach (KeyValuePair<string, ulong> label in unit.labels)
            {
                labels.Add(label.Key, label.Value);
            }
            foreach (KeyValuePair<string, ulong> variable in unit.variables)
            {
                labels.Add(variable.Key, variable.Value);
            }

            addresses.AddRange(unit.lookups);

            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved)
            {
                missing.Add(unresolved.Value.text, unresolved.Key);
                Report.PrintWarning(unit.source, unresolved.Value, $"WARNING: Unresolved label '{unresolved.Value.text}'");
            }
            insts.Add(unit.insts.ToArray());
            foreach (KeyValuePair<Word, ushort> item in unit.pool)
            {
                pool.Add(item.Key, item.Value);
            }
        }

        if (!Report.sentErros)
        {
            Compiler compiler = new Compiler(insts.ToArray(), pool, 0);
            BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
            compiler.WriteHeader(ref writer, (ushort)(Metadata.RELOCATABLE | (Report.sentWarning ? Metadata.CONTAINS_ERRORS : 0)));
            compiler.WriteRelocatableHeader(ref writer, Convert.ToUInt16(labels.Count), Convert.ToUInt16(missing.Count), Convert.ToUInt16(addresses.Count));
            compiler.WriteSymbols(ref writer, labels);
            compiler.WriteSymbols(ref writer, missing);
            compiler.WriteAddresses(ref writer, addresses.ToArray());
            compiler.WritePool(ref writer);
            compiler.WriteProgram(ref writer);
            writer.Close();
        }
    }
}