using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeFusion.ASM.Compiling;
using CodeFusion.ASM.Lexing;
using CodeFusion.ASM.Parsing;
using CodeFusion.Format;
using CodeFusion.VM;

namespace CodeFusion.ASM;

public enum OutputType
{
    RELOCATABLE,
    EXECUTABLE,
    LIBRARY
}

static class Program
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
            BinFile file = new BinFile(meta);

            for (uint i = 0; i < meta.sectionCount; i++)
            {
                Section section = Loader.ReadSection(ref reader);
                if (section != null)
                {
                    file.Add(section);
                }
            }

            units.Add(new ObjectUnit(file, path));

            reader.Close();
            reader.Dispose();
        }

        return units.ToArray();
    }

    private static void CombineObjsToExecutable()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErrors)
        {
            return;
        }

        ObjectUnit baseUnit = units[0];

        if (units.Length > 1)
        {
            Combiner combiner = new Combiner();

            for (int i = 1; i < units.Length; i++)
            {
                combiner.Combine(ref baseUnit, units[i]);
            }
        }

        foreach (MissingSection section in baseUnit.file.sections.Where(section => section.type == Section.TYPE_MISSING).Cast<MissingSection>())
        {
            foreach (KeyValuePair<string, ulong> pair in section.pool)
            {
                Report.PrintReport(baseUnit.path, $"ERROR: Unresolved label '{pair.Value}'");
            }
        }

        if (Report.sentErrors)
        {
            return;
        }

        BinFile file = new BinFile(baseUnit.file);
        file.flags = Metadata.EXECUTABLE;

        FinFile lib = new FinFile(file);
        MemoryStream result = lib.GetBytes(baseUnit.file.sections.Where(section => section.type is Section.TYPE_PROGRAM or Section.TYPE_POOL or Section.TYPE_MEMORY));
        FileStream fileStream = new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate);
        result.WriteTo(fileStream);
        result.Close();
        result.Dispose();
        fileStream.Close();
        fileStream.Dispose();
    }

    private static void CombineObjs()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErrors)
        {
            return;
        }

        if (units.Length < 2)
        {
            Report.PrintReport(Options.INSTANCE.files[0], "Need at least two object files");
            return;
        }


        ObjectUnit baseUnit = units[0];
        Combiner combiner = new Combiner();

        for (int i = 1; i < units.Length; i++)
        {
            combiner.Combine(ref baseUnit, units[i]);
        }

        foreach (MissingSection section in baseUnit.file.sections.Where(section => section.type == Section.TYPE_MISSING).Cast<MissingSection>())
        {
            foreach (KeyValuePair<string, ulong> pair in section.pool)
            {
                Report.PrintWarning(baseUnit.path, $"WARNING: Unresolved label '{pair.Value}'");
            }
        }

        BinFile file = new BinFile(baseUnit.file);
        file.flags = (byte)(Metadata.RELOCATABLE | (Report.sentWarning || Report.sentErrors ? 0 : Metadata.CONTAINS_ERRORS));
        file.AddRange(baseUnit.file.sections);

        BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
        writer.Write(file.ToBytes());
        writer.Close();
    }

    private static void CombineObjsToLibrary()
    {
        ObjectUnit[] units = CreateObjectUnits();

        if (Report.sentErrors)
        {
            return;
        }

        ObjectUnit baseUnit = units[0];

        if (units.Length > 1)
        {
            Combiner combiner = new Combiner();

            for (int i = 1; i < units.Length; i++)
            {
                combiner.Combine(ref baseUnit, units[i]);
            }
        }

        foreach (MissingSection section in baseUnit.file.sections.Where(section => section.type == Section.TYPE_MISSING).Cast<MissingSection>())
        {
            foreach (KeyValuePair<string, ulong> pair in section.pool)
            {
                Report.PrintReport(baseUnit.path, $"ERROR: Unresolved label '{pair.Value}'");
            }
        }

        if (Report.sentErrors)
        {
            return;
        }


        BinFile file = new BinFile(baseUnit.file);
        file.flags = Metadata.LIBRARY;
        FinFile lib = new FinFile(file);
        MemoryStream result =
            lib.GetBytes(baseUnit.file.sections.Where(section => section.type is Section.TYPE_PROGRAM or Section.TYPE_POOL or Section.TYPE_SYMBOL or Section.TYPE_MEMORY));
        FileStream fileStream = new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate);
        result.WriteTo(fileStream);
        result.Close();
        result.Dispose();
        fileStream.Close();
        fileStream.Dispose();
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
                        unit.lookups.Add(unit.addressOffset + unresolved.Key);
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
                    if (item.memoryLabels.TryGetValue(unresolved.Value.text, out value))
                    {
                        Inst inst = unit.insts[(int)unresolved.Key];
                        inst.operand = new Word(value);
                        unit.insts[(int)unresolved.Key] = inst;
                        unit.unresolved.Remove(unresolved.Key);
                        unit.memoryLookups.Add(unit.addressOffset + unresolved.Key);
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
        BinFile file = new BinFile();
        file.magic = new[]
        {
            '.', 'C', 'F'
        };
        file.version = Metadata.CURRENT_VERSION;
        file.flags = Metadata.EXECUTABLE;

        foreach (CodeUnit unit in units)
        {
            ProgramSection programSection = new ProgramSection();
            PoolSection poolSection = new PoolSection();
            MemorySection memorySection = new MemorySection();

            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved)
            {
                Report.PrintReport(unit.source, unresolved.Value, $"Unresolved label '{unresolved.Value.text}'");
            }
            programSection.program.AddRange(unit.insts);
            foreach (KeyValuePair<Word, ushort> item in unit.pool)
            {
                poolSection.pool.Add(item.Key, item.Value);
            }

            memorySection.data.AddRange(unit.memory);

            if (unit.labels.TryGetValue(Options.INSTANCE.entryPoint, out ulong value))
            {
                file.entryPoint = value;
            }
            file.Add(programSection);
            file.Add(poolSection);
            file.Add(memorySection);
        }

        if (!Report.sentErrors)
        {
            FinFile lib = new FinFile(file);
            MemoryStream result = lib.GetBytes(file.sections);
            FileStream fileStream = new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate);
            result.WriteTo(fileStream);
            result.Close();
            result.Dispose();
            fileStream.Close();
            fileStream.Dispose();
        }
    }

    private static void CompileASMToObject()
    {
        CodeUnit[] units = CreateCodeUnits();
        MissingSection missingSection = new MissingSection();
        SymbolSection symbolSection = new SymbolSection();
        AddressSection addressSection = new AddressSection();
        MemorySymbolSection memorySymbolSection = new MemorySymbolSection();
        MemoryAddressSection memoryAddressSection = new MemoryAddressSection();

        BinFile file = new BinFile();
        file.magic = new[]
        {
            '.', 'C', 'F'
        };
        file.version = Metadata.CURRENT_VERSION;

        foreach (CodeUnit unit in units)
        {
            ProgramSection programSection = new ProgramSection();
            PoolSection poolSection = new PoolSection();
            MemorySection memorySection = new MemorySection();

            foreach (KeyValuePair<string, ulong> item in unit.labels)
            {
                symbolSection.pool.Add(item.Key, item.Value);
            }
            foreach (KeyValuePair<string, ulong> item in unit.memoryLabels)
            {
                memorySymbolSection.pool.Add(item.Key, item.Value);
            }

            addressSection.addresses.AddRange(unit.lookups);
            memoryAddressSection.addresses.AddRange(unit.lookups);

            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved)
            {
                missingSection.pool.Add(unresolved.Value.text, unresolved.Key);
                Report.PrintWarning(unit.source, unresolved.Value, $"WARNING: Unresolved label '{unresolved.Value.text}'");
            }
            programSection.program.AddRange(unit.insts);
            memorySection.data.AddRange(unit.memory);

            foreach (KeyValuePair<Word, ushort> item in unit.pool)
            {
                poolSection.pool.Add(item.Key, item.Value);
            }

            file.Add(programSection);
            Console.WriteLine(programSection.program.Count);
            file.Add(poolSection);
            file.Add(memorySection);
        }

        file.Add(missingSection);
        file.Add(symbolSection);
        file.Add(addressSection);
        file.Add(memorySymbolSection);
        file.Add(memoryAddressSection);

        if (!Report.sentErrors)
        {
            file.flags = (byte)(Metadata.RELOCATABLE | (Report.sentWarning ? Metadata.CONTAINS_ERRORS : 0));
            BinaryWriter writer = new BinaryWriter(new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate));
            if (symbolSection.pool.TryGetValue(Options.INSTANCE.entryPoint, out ulong value))
            {
                file.entryPoint = value;
            }

            writer.Write(file.ToBytes());
            writer.Close();
        }
    }

    private static void CompileASMToLibrary()
    {
        CodeUnit[] units = CreateCodeUnits();
        BinFile file = new BinFile();
        file.magic = new[]
        {
            '.', 'C', 'F'
        };
        file.version = Metadata.CURRENT_VERSION;
        file.flags = Metadata.LIBRARY;

        SymbolSection symbolSection = new SymbolSection();

        foreach (CodeUnit unit in units)
        {
            ProgramSection programSection = new ProgramSection();
            PoolSection poolSection = new PoolSection();
            MemorySection memorySection = new MemorySection();

            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved)
            {
                Report.PrintReport(unit.source, unresolved.Value, $"Unresolved label '{unresolved.Value.text}'");
            }
            programSection.program.AddRange(unit.insts);
            memorySection.data.AddRange(unit.memory);
            foreach (KeyValuePair<Word, ushort> item in unit.pool)
            {
                poolSection.pool.Add(item.Key, item.Value);
            }

            foreach (KeyValuePair<string, ulong> label in unit.labels)
            {
                symbolSection.pool.Add(label.Key, label.Value);
            }

            file.Add(programSection);
            file.Add(poolSection);
            file.Add(memorySection);
        }

        file.Add(symbolSection);

        if (!Report.sentErrors)
        {
            FinFile lib = new FinFile(file);
            MemoryStream result = lib.GetBytes(file.sections);
            FileStream fileStream = new FileStream(Options.INSTANCE.output, FileMode.OpenOrCreate);
            result.WriteTo(fileStream);
            result.Close();
            result.Dispose();
            fileStream.Close();
            fileStream.Dispose();
        }
    }
}