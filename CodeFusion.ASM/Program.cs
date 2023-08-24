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

        if (Options.INSTANCE.files.Length  < 1) {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine("No files given");
            Environment.Exit(1);
        }

        if (Options.INSTANCE.combine)
        {
            if (Options.INSTANCE.relocatable)
            {
                CombineObjs();
            }
            else
            {
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

    private static ObjectUnit[] CreateObjectUnits()
    {
        List<ObjectUnit> units = new List<ObjectUnit>();

        foreach (string path in Options.INSTANCE.files)
        {
            Compiling.Loader loader = new Compiling.Loader(path);
            Metadata meta = loader.ReadHeader();
            if (meta.magic[0] != '.' || meta.magic[1] != 'C' || meta.magic[2] != 'F')
            {
                Report.PrintReport(path, "File has not the correct file format");
                continue;
            }

            if (meta.version != Metadata.CURRENT_VERSION)
            {
                Report.PrintReport(path, $"Object is not compatible with the VM file expect '{meta.version}' VM has '{Metadata.CURRENT_VERSION}'");
                continue;
            }
            
            if ((meta.flags & Metadata.RELOCATABLE) != Metadata.RELOCATABLE)
            {
                Report.PrintReport(path, "Object is not relocatable");
                continue;
            }

            RelocatableMetadata relocatable = loader.ReadRelocatableHeader();
            ObjectUnit unit = new ObjectUnit();

            for (int i = 0; i < relocatable.symbolCount; i++)
            {
                byte b;
                string name = "";
                do
                {
                    b = loader.reader.ReadByte();
                    if (b != 0) {
                        name += (char)b;
                    }
                } while (b != 0);
                unit.labels.Add(name, BitConverter.ToUInt64(loader.reader.ReadBytes(8)));
            }
            
            for (int i = 0; i < relocatable.missingCount; i++)
            {
                byte b;
                string name = "";
                do
                {
                    b = loader.reader.ReadByte();
                    if (b != 0) {
                        name += (char)b;
                    }
                } while (b != 0);

                unit.unresolved.Add(BitConverter.ToUInt64(loader.reader.ReadBytes(8)), name);
            }

            for (int i = 0; i < relocatable.addressCount; i++)
            {
                unit.addresses.Add(BitConverter.ToUInt64(loader.reader.ReadBytes(8)));
            }

            for (int i = 0; i < meta.poolSize; i++)
            {
                unit.pool.Add(new Word(BitConverter.ToUInt64(loader.reader.ReadBytes(8))), BitConverter.ToUInt16(loader.reader.ReadBytes(2)));
            }

            for (ulong i = 0; i < meta.programSize; i++)
            {
                byte opcode = loader.reader.ReadByte();
                if (!Opcode.HasOperand(opcode))
                {
                    unit.insts.Add(new Inst(opcode));
                    continue;
                }

                byte size = loader.reader.ReadByte();
                if (size == 0)
                {
                    unit.insts.Add(new Inst(opcode));
                    continue;
                }

                byte[] bytes = new byte[8];
                for (int j = 0; j < size; j++)
                {
                    bytes[j] = loader.reader.ReadByte();
                }
                unit.insts.Add(new Inst(opcode, new Word(BitConverter.ToUInt64(bytes))));
            }

            units.Add(unit);
        }

        return units.ToArray();
    }

    private static void CombineObjsToExecutable()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErros) {
            return;
        }

        ObjectUnit baseUnit = units[0];
        
        if (units.Length > 1)
        {
            Combinder combinder = new Combinder();

            for (int i = 1; i < units.Length; i++)
            {
                combinder.Combine(ref baseUnit, units[i]);
            }
        }

        foreach (KeyValuePair<ulong, string> unresolved in baseUnit.unresolved)
        {
            Report.PrintWarning(baseUnit.path, $"WARNING: Unresolved label '{unresolved.Value}'");
        }

        BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
        Compiler compiler = new Compiler(new[] { baseUnit.insts.ToArray() }, baseUnit.pool, 0);
        compiler.WriteHeader(ref writer, (byte)(Metadata.EXECUTABLE | (baseUnit.unresolved.Count == 0 ? 0 : Metadata.CONTAINS_ERRORS)));
        compiler.WritePool(ref writer);
        compiler.WriteProgram(ref writer);

        writer.Close();
    }

    private static void CombineObjs()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErros) {
            return;
        }

        if (units.Length < 2)
        {
            Report.PrintReport(Options.INSTANCE.files[0], "Need at least two object files");
            return;
        }


        ObjectUnit baseUnit = units[0];
        Combinder combinder = new Combinder();

        for (int i = 1; i < units.Length; i++)
        {
            combinder.Combine(ref baseUnit, units[i]);
        }

        foreach (KeyValuePair<ulong, string> unresolved in baseUnit.unresolved)
        {
            Report.PrintWarning(baseUnit.path, $"WARNING: Unresolved label '{unresolved.Value}'");
        }

        BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
        Compiler compiler = new Compiler(new[] { baseUnit.insts.ToArray() }, baseUnit.pool, 0);
        compiler.WriteHeader(ref writer, (byte)(Metadata.RELOCATABLE | (baseUnit.unresolved.Count == 0 ? 0 : Metadata.CONTAINS_ERRORS)));
        compiler.WriteRelocatableHeader(ref writer, (ushort)baseUnit.labels.Count, (ushort)baseUnit.unresolved.Count, (ushort)baseUnit.addresses.Count);
        compiler.WriteSymbols(ref writer, baseUnit.labels);
        compiler.WriteSymbols(ref writer, baseUnit.unresolved);
        compiler.WriteAddresses(ref writer, baseUnit.addresses.ToArray());
        compiler.WritePool(ref writer);
        compiler.WriteProgram(ref writer);

        writer.Close();
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
            compiler.WriteHeader(ref writer, (byte)(Metadata.RELOCATABLE | (Report.sentWarning ? Metadata.CONTAINS_ERRORS : 0)));
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