using System;
using System.Collections.Generic;
using System.ComponentModel;
using CodeFusion.ASM.Parsing;
using CodeFusion.VM;

namespace CodeFusion.ASM.Compiling;

public class Combinder {
    
    public void Combine(ref ObjectUnit baseUnit, ObjectUnit unit) {
        ulong offset = Convert.ToUInt64(baseUnit.insts.Count);
        UpdateMissings(ref baseUnit, ref unit);
        UpdateAddresses(ref unit, offset);

        foreach(Inst inst in unit.insts) {
            baseUnit.insts.Add(inst);
        }

        foreach(KeyValuePair<Word, ushort> pool in unit.pool) {
            baseUnit.pool.Add(new Word(pool.Key.asU64 + offset), pool.Value);
        }

        foreach(KeyValuePair<ulong, string> unresolved in unit.unresolved) {
            baseUnit.unresolved.Add(unresolved.Key, unresolved.Value);
        }

        foreach(KeyValuePair<string, ulong> label in unit.labels) {
            if (baseUnit.labels.ContainsKey(label.Key)) {
                Report.PrintWarning(baseUnit.path, $"WARNING: Label '{label.Key}' will be overwritten by '{unit.path}'");
            }

            baseUnit.labels.Add(label.Key, label.Value);
        }

        foreach(ulong address in unit.addresses) {
            baseUnit.addresses.Add(address + offset);
        }
    }

    private void UpdateAddresses(ref ObjectUnit unit, ulong offset)
    {
        foreach(ulong i in unit.addresses) {
            Inst inst = unit.insts[(int)i];
            inst.operand = new Word(inst.operand.asU64 + offset);
            unit.insts[(int)i] = inst;
        }
    }

    private void UpdateMissings(ref ObjectUnit baseUnit, ref ObjectUnit unit) {
        foreach(KeyValuePair<ulong, string> unresolved in baseUnit.unresolved) {
            if (unit.labels.TryGetValue(unresolved.Value, out ulong value)) {
                Inst inst = baseUnit.insts[(int)unresolved.Key];
                inst.operand = new Word(value);
                baseUnit.insts[(int)unresolved.Key] = inst;
                baseUnit.unresolved.Remove(unresolved.Key);
            }
        }

        foreach(KeyValuePair<ulong, string> unresolved in unit.unresolved) {
            if (baseUnit.labels.TryGetValue(unresolved.Value, out ulong value)) {
                Inst inst = unit.insts[(int)unresolved.Key];
                inst.operand = new Word(value);
                unit.insts[(int)unresolved.Key] = inst;
                unit.unresolved.Remove(unresolved.Key);
            }
        }
    }
}