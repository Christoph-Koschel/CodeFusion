using System;
using System.Collections.Generic;
using System.IO;
using CodeFusion.ASM.Compiling;
using CodeFusion.ASM.Lexing;
using CodeFusion.ASM.Parsing;
using CodeFusion.VM;

namespace CodeFusion.ASM;

class Program {
    public static void Main(string[] args) {
        string[] TMP_PATHS = { @"C:\Users\kosch\Desktop\Workbench\CodeFusion\cross.cf", @"C:\Users\kosch\Desktop\Workbench\CodeFusion\test.cf" };
        string ENTRY_POINT = "entry";
        string OUTPUT = @"C:\Users\kosch\Desktop\Workbench\CodeFusion\test.bin";

        ulong addressOffset = 0;
        List<CodeUnit> units = new List<CodeUnit>();
        foreach(string TMP_PATH in TMP_PATHS) {
            Parser parser = new Parser(TMP_PATH);
            CodeUnit unit = parser.ParseUnit(addressOffset);
            addressOffset += Convert.ToUInt32(unit.insts.Count);
            units.Add(unit);
        }

        foreach(CodeUnit unit in units) {
            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved) {
                foreach(CodeUnit item in units) {
                    if (item.labels.TryGetValue(unresolved.Value.text, out ulong value)) {
                        Inst inst = unit.insts[(int)unresolved.Key];
                        inst.operand = new Word(value);
                        unit.insts[(int)unresolved.Key] = inst;
                        unit.unresolved.Remove(unresolved.Key);
                        break;
                    }
                }
            }
        }

        List<Inst[]> insts = new List<Inst[]>();
        Dictionary<Word, ushort> pool = new Dictionary<Word, ushort>();
        ulong entryPoint = 0;
        
        foreach(CodeUnit unit in units) {
            foreach (KeyValuePair<ulong, Token> unresolved in unit.unresolved) {
                Report.PrintReport(unit.source, unresolved.Value, $"Unresolved label '{unresolved.Value.text}'");
            }
            insts.Add(unit.insts.ToArray());
            foreach(KeyValuePair<Word, ushort> item in unit.pool) {
                pool.Add(item.Key, item.Value);
            }

            if (unit.labels.TryGetValue(ENTRY_POINT, out ulong value)) {
                entryPoint = value;
            }
        }

        

        foreach(CodeUnit unit in units) {
            foreach(Inst inst in unit.insts) {
                Console.WriteLine("{0, -10} {1}", Opcode.GetOpcodeName(inst.opcode), inst.operand.asU64);
            }
        }

        if (!Report.sentErros) {
            Compiler compiler = new Compiler(insts.ToArray(), pool, entryPoint);
            BinaryWriter writer = new BinaryWriter(new FileStream(OUTPUT, FileMode.OpenOrCreate));
            compiler.WriteTo(writer);
            writer.Close();
        }
    }
}