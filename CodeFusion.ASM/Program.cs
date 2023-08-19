using System;
using System.Collections.Generic;
using CodeFusion.ASM.Lexing;
using CodeFusion.ASM.Parsing;
using CodeFusion.VM;

namespace CodeFusion.ASM;

class Program {
    public static void Main(string[] args) {
        string[] TMP_PATHS = { @"C:\Users\kosch\Desktop\Workbench\CodeFusion\cross.cf", @"C:\Users\kosch\Desktop\Workbench\CodeFusion\test.cf" };
        long addressOffset = 0;
        List<CodeUnit> units = new List<CodeUnit>();
        foreach(string TMP_PATH in TMP_PATHS) {
            Parser parser = new Parser(TMP_PATH);
            CodeUnit unit = parser.ParseUnit(addressOffset);
            addressOffset += unit.insts.Count;
            units.Add(unit);
        }

        foreach(CodeUnit unit in units) {
            foreach (KeyValuePair<long, Token> unresolved in unit.unresolved) {
                foreach(CodeUnit item in units) {
                    if (item.labels.TryGetValue(unresolved.Value.text, out long value)) {
                        Inst inst = unit.insts[(int)unresolved.Key];
                        inst.operand = new Word(value);
                        unit.insts[(int)unresolved.Key] = inst;
                        unit.unresolved.Remove(unresolved.Key);
                        break;
                    }
                }
            }
        }

        foreach(CodeUnit unit in units) {
            foreach (KeyValuePair<long, Token> unresolved in unit.unresolved) {
                Report.PrintReport(unit.source, unresolved.Value, $"Unresolved label '{unresolved.Value.text}'");
            }
        }

        foreach(CodeUnit unit in units) {
            foreach(Inst inst in unit.insts) {
                Console.WriteLine("{0, -10} {1}", Opcode.GetOpcodeName(inst.opcode), inst.operand.asU64);
            }
        }
    }
}