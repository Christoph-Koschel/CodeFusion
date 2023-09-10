using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeFusion.ASM.Compiling;
using CodeFusion.ASM.Lexing;
using CodeFusion.ASM.Parsing;
using CodeFusion.VM;

namespace CodeFusion.ASM;

public enum OutputType
{
    RELOCATABLE,
    EXECUTABLE,
    LIBRARY
}

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
            else if (args[i] == "-t")
            {
                string value = args[++i];
                if (value == "exe")
                {
                    Options.INSTANCE.outputType = OutputType.EXECUTABLE;
                }
                else if (value == "lib")
                {
                    Options.INSTANCE.outputType = OutputType.LIBRARY;
                }
                else if (value == "obj")
                {
                    Options.INSTANCE.outputType = OutputType.RELOCATABLE;
                }
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

        if (Options.INSTANCE.files.Length < 1)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine("No files given");
            Environment.Exit(1);
        }


        bool checkForCF = files[0].EndsWith(".cf");
        foreach (string file in files)
        {
            if (checkForCF)
            {
                if (!file.EndsWith(".cf"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Error.WriteLine("Can only use object files or code files as input");
                    Environment.Exit(1);
                }
            }
            else
            {
                if (file.EndsWith(".cf"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Error.WriteLine("Can only use object files or code files as input");
                    Environment.Exit(1);
                }
            }
        }

        if (Options.INSTANCE.combine)
        {
            if (Options.INSTANCE.outputType != OutputType.RELOCATABLE)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine("Can only combine object files to relocatable object file");
                Environment.Exit(1);
            }
            if (checkForCF)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine("Can only combine object files");
                Environment.Exit(1);
            }

            CombineObjs();
        }
        else if (Options.INSTANCE.outputType == OutputType.RELOCATABLE)
        {
            if (checkForCF)
            {
                CompileASMToObject();
            }
        }
        else if (Options.INSTANCE.outputType == OutputType.EXECUTABLE)
        {
            if (checkForCF)
            {
                CompileASMToExecutable();
            }
            else
            {
                CombineObjsToExecutable();
            }
        }
        else if (Options.INSTANCE.outputType == OutputType.LIBRARY)
        {
            if (checkForCF)
            {
                CompileASMToLibrary();
            }
            else
            {
                CombineObjsToLibrary();
            }
        }
    }

    private static ObjectUnit[] CreateObjectUnits()
    {
        List<ObjectUnit> units = new List<ObjectUnit>();

        foreach (string path in Options.INSTANCE.files)
        {
            BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
            Metadata meta = Loader.ReadMainHeader(ref reader);
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

            RelocatableMetadata relocatable = Loader.ReadRelocatableHeader(ref reader);
            ObjectUnit unit = new ObjectUnit();

            for (int i = 0; i < relocatable.symbolCount; i++)
            {
                ushort nameLength = reader.ReadUInt16();
                string name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
                unit.labels.Add(name, BitConverter.ToUInt64(reader.ReadBytes(8)));
            }

            for (int i = 0; i < relocatable.missingCount; i++)
            {
                ushort nameLength = reader.ReadUInt16();
                string name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
                unit.unresolved.Add(BitConverter.ToUInt64(reader.ReadBytes(8)), name);
            }

            for (int i = 0; i < relocatable.addressCount; i++)
            {
                unit.addresses.Add(BitConverter.ToUInt64(reader.ReadBytes(8)));
            }

            for (int i = 0; i < meta.poolSize; i++)
            {
                unit.pool.Add(new Word(BitConverter.ToUInt64(reader.ReadBytes(8))), BitConverter.ToUInt16(reader.ReadBytes(2)));
            }

            for (ulong i = 0; i < meta.programSize; i++)
            {
                byte opcode = reader.ReadByte();
                if (!Opcode.HasOperand(opcode))
                {
                    unit.insts.Add(new Inst(opcode));
                    continue;
                }

                byte size = reader.ReadByte();
                if (size == 0)
                {
                    unit.insts.Add(new Inst(opcode));
                    continue;
                }

                byte[] bytes = new byte[8];
                for (int j = 0; j < size; j++)
                {
                    bytes[j] = reader.ReadByte();
                }
                unit.insts.Add(new Inst(opcode, new Word(BitConverter.ToUInt64(bytes))));
            }

            units.Add(unit);
            reader.Close();
            reader.Dispose();
        }

        return units.ToArray();
    }

    private static void CombineObjsToExecutable()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErros)
        {
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
            Report.PrintReport(baseUnit.path, $"ERROR: Unresolved label '{unresolved.Value}'");
        }

        if (Report.sentErros)
        {
            return;
        }

        BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
        Compiler compiler = new Compiler(new[]
        {
            baseUnit.insts.ToArray()
        }, baseUnit.pool, 0);
        compiler.WriteHeader(ref writer, (byte)(Metadata.EXECUTABLE | (baseUnit.unresolved.Count == 0 ? 0 : Metadata.CONTAINS_ERRORS)));
        compiler.WritePool(ref writer);
        compiler.WriteProgram(ref writer);

        writer.Close();
    }

    private static void CombineObjs()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErros)
        {
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
        Compiler compiler = new Compiler(new[]
        {
            baseUnit.insts.ToArray()
        }, baseUnit.pool, 0);
        compiler.WriteHeader(ref writer, (byte)(Metadata.RELOCATABLE | (baseUnit.unresolved.Count == 0 ? 0 : Metadata.CONTAINS_ERRORS)));
        compiler.WriteRelocatableHeader(ref writer, (ushort)baseUnit.labels.Count, (ushort)baseUnit.unresolved.Count, (ushort)baseUnit.addresses.Count);
        compiler.WriteSymbols(ref writer, baseUnit.labels);
        compiler.WriteSymbols(ref writer, baseUnit.unresolved);
        compiler.WriteAddresses(ref writer, baseUnit.addresses.ToArray());
        compiler.WritePool(ref writer);
        compiler.WriteProgram(ref writer);

        writer.Close();
    }

    private static void CombineObjsToLibrary()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErros)
        {
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
            Report.PrintReport(baseUnit.path, $"ERROR: Unresolved label '{unresolved.Value}'");
        }

        if (Report.sentErros)
        {
            return;
        }

        BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
        Compiler compiler = new Compiler(new[]
        {
            baseUnit.insts.ToArray()
        }, baseUnit.pool, 0, (uint)baseUnit.labels.Count);
        compiler.WriteHeader(ref writer, Metadata.LIBRARY);
        compiler.WritePool(ref writer);
        compiler.WriteProgram(ref writer);
        compiler.WriteSymbols(ref writer, baseUnit.labels);
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

    private static void CompileASMToLibrary()
    {
        CodeUnit[] units = CreateCodeUnits();
        List<Inst[]> insts = new List<Inst[]>();
        Dictionary<Word, ushort> pool = new Dictionary<Word, ushort>();
        ulong entryPoint = 0;

        Dictionary<string, ulong> labels = new Dictionary<string, ulong>();

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

            foreach (KeyValuePair<string, ulong> label in unit.labels)
            {
                labels.Add(label.Key, label.Value);
            }
        }

        if (!Report.sentErros)
        {
            Compiler compiler = new Compiler(insts.ToArray(), pool, entryPoint, (uint)labels.Count);
            BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
            compiler.WriteHeader(ref writer, Metadata.LIBRARY);
            compiler.WritePool(ref writer);
            compiler.WriteProgram(ref writer);
            compiler.WriteSymbols(ref writer, labels);
            writer.Close();
        }
    }
}