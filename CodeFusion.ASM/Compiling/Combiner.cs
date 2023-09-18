using System;
using System.Collections.Generic;
using System.Linq;
using CodeFusion.ASM.Parsing;
using CodeFusion.Format;
using CodeFusion.VM;

namespace CodeFusion.ASM.Compiling;

public class Combiner
{
    public void Combine(ref ObjectUnit baseUnit, ObjectUnit unit)
    {
        ulong offset = Convert.ToUInt64(baseUnit.programLength);
        ulong memoryOffset = Convert.ToUInt64(baseUnit.memoryLength);
        UpdateMissing(ref baseUnit, ref unit);
        UpdateAddresses(ref unit, offset);
        UpdateMemoryAddress(ref unit, memoryOffset);

        baseUnit.file.sections.AddRange(unit.file.sections.Where(section => section.type is Section.TYPE_PROGRAM or Section.TYPE_MEMORY));

        foreach (PoolSection section in unit.file.sections.Where(section => section.type == Section.TYPE_POOL).Cast<PoolSection>())
        {
            PoolSection poolSection = new PoolSection();

            foreach (KeyValuePair<Word, ushort> pair in section.pool)
            {
                poolSection.pool.Add(new Word(pair.Key.asU64 + offset), pair.Value);
            }
        }

        MissingSection missingSection = baseUnit.file.sections.FirstOrDefault(section => section.type == Section.TYPE_MISSING) as MissingSection ??
                                        new MissingSection();


        foreach (MissingSection otherMissing in unit.file.sections.Where(section => section.type == Section.TYPE_MISSING).Cast<MissingSection>())
        {
            foreach (KeyValuePair<string, ulong> pair in otherMissing.pool)
            {
                missingSection.pool.Add(pair.Key, pair.Value);
            }
        }


        foreach (SymbolSection symbolSection in unit.file.sections.Where(section => section.type == Section.TYPE_SYMBOL).Cast<SymbolSection>())
        {
            SymbolSection copy = new SymbolSection();
            foreach (KeyValuePair<string, ulong> pair in symbolSection.pool)
            {
                copy.pool.Add(pair.Key, pair.Value + offset);
            }
            baseUnit.file.sections.Add(copy);
        }

        foreach (AddressSection addressSection in unit.file.sections.Where(section => section.type == Section.TYPE_ADDRESS).Cast<AddressSection>())
        {
            AddressSection copy = new AddressSection();
            foreach (ulong address in addressSection.addresses)
            {
                copy.addresses.Add(address + offset);
            }
            baseUnit.file.sections.Add(copy);
        }

        foreach (MemorySymbolSection symbolSection in unit.file.sections.Where(section => section.type == Section.TYPE_SYMBOL).Cast<MemorySymbolSection>())
        {
            MemorySymbolSection copy = new MemorySymbolSection();
            foreach (KeyValuePair<string, ulong> pair in symbolSection.pool)
            {
                copy.pool.Add(pair.Key, pair.Value + memoryOffset);
            }
            baseUnit.file.sections.Add(copy);
        }

        foreach (MemoryAddressSection memoryAddressSection in unit.file.sections.Where(section => section.type == Section.TYPE_ADDRESS)
                     .Cast<MemoryAddressSection>())
        {
            MemoryAddressSection copy = new MemoryAddressSection();
            foreach (ulong address in memoryAddressSection.addresses)
            {
                copy.addresses.Add(address + offset);
            }
            baseUnit.file.sections.Add(copy);
        }
    }

    private void UpdateAddresses(ref ObjectUnit unit, ulong offset)
    {
        foreach (AddressSection addressSection in unit.file.sections.Where(section => section.type == Section.TYPE_ADDRESS).Cast<AddressSection>())
        {
            foreach (ulong address in addressSection.addresses)
            {
                ulong addressBlock = 0;
                foreach (ProgramSection programSection in unit.file.sections.Where(section => section.type == Section.TYPE_PROGRAM).Cast<ProgramSection>())
                {
                    if (addressBlock + (ulong)programSection.program.Count > address)
                    {
                        Inst inst = programSection.program[(int)(address - addressBlock)];
                        inst.operand = new Word(inst.operand.asU64 + offset);
                        programSection.program[(int)(address - addressBlock)] = inst;
                        break;
                    }
                    addressBlock += (ulong)programSection.program.Count;
                }
            }
        }
    }

    private void UpdateMemoryAddress(ref ObjectUnit unit, ulong offset)
    {
        foreach (MemoryAddressSection addressSection in unit.file.sections.Where(section => section.type == Section.TYPE_MEMORY_ADDRESS)
                     .Cast<MemoryAddressSection>())
        {
            foreach (ulong address in addressSection.addresses)
            {
                ulong addressBlock = 0;
                foreach (ProgramSection programSection in unit.file.sections.Where(section => section.type == Section.TYPE_PROGRAM).Cast<ProgramSection>())
                {
                    if (addressBlock + (ulong)programSection.program.Count > address)
                    {
                        Inst inst = programSection.program[(int)(address - addressBlock)];
                        inst.operand = new Word(inst.operand.asU64 + offset);
                        programSection.program[(int)(address - addressBlock)] = inst;
                        break;
                    }
                    addressBlock += (ulong)programSection.program.Count;
                }
            }
        }
    }

    private void UpdateMissing(ref ObjectUnit baseUnit, ref ObjectUnit unit)
    {
        foreach (MissingSection missingSection in baseUnit.file.sections.Where(section => section.type == Section.TYPE_MISSING).Cast<MissingSection>())
        {
            foreach (KeyValuePair<string, ulong> pair in missingSection.pool)
            {
                KeyValuePair<string, ulong> pair2 = unit.file.sections.Where(section => section.type == Section.TYPE_SYMBOL)
                    .Cast<SymbolSection>().SelectMany(section => section.pool).FirstOrDefault(pair2 => pair2.Key == pair.Key);
                if (pair2.Key == null)
                {
                    pair2 = unit.file.sections.Where(section => section.type == Section.TYPE_MEMORY_SYMBOL).Cast<MemorySymbolSection>()
                        .SelectMany(section => section.pool).FirstOrDefault(pair2 => pair2.Key == pair.Key);
                }
                if (pair2.Key != null)
                {
                    ulong addressBlock = 0;
                    foreach (ProgramSection programSection in baseUnit.file.sections.Where(section => section.type == Section.TYPE_PROGRAM)
                                 .Cast<ProgramSection>())
                    {
                        if (addressBlock + (ulong)programSection.program.Count > pair.Value)
                        {
                            Inst inst = programSection.program[(int)(pair.Value - addressBlock)];
                            inst.operand = new Word(pair2.Value);
                            programSection.program[(int)(pair.Value - addressBlock)] = inst;
                            break;
                        }
                        addressBlock += (ulong)programSection.program.Count;
                    }
                }
            }
        }

        foreach (MissingSection missingSection in unit.file.sections.Where(section => section.type == Section.TYPE_MISSING).Cast<MissingSection>())
        {
            foreach (KeyValuePair<string, ulong> pair in missingSection.pool)
            {
                KeyValuePair<string, ulong> pair2 = baseUnit.file.sections.Where(section => section.type == Section.TYPE_SYMBOL)
                    .Cast<SymbolSection>().SelectMany(section => section.pool).FirstOrDefault(pair2 => pair2.Key == pair.Key);
                if (pair2.Key == null)
                {
                    pair2 = baseUnit.file.sections.Where(section => section.type == Section.TYPE_MEMORY_SYMBOL).Cast<MemorySymbolSection>()
                        .SelectMany(section => section.pool).FirstOrDefault(pair2 => pair2.Key == pair.Key);
                }
                if (pair2.Key != null)
                {
                    ulong addressBlock = 0;
                    foreach (ProgramSection programSection in unit.file.sections.Where(section => section.type == Section.TYPE_PROGRAM)
                                 .Cast<ProgramSection>())
                    {
                        if (addressBlock + (ulong)programSection.program.Count > pair.Value)
                        {
                            Inst inst = programSection.program[(int)(pair.Value - addressBlock)];
                            inst.operand = new Word(pair2.Value);
                            programSection.program[(int)(pair.Value - addressBlock)] = inst;
                            break;
                        }
                        addressBlock += (ulong)programSection.program.Count;
                    }
                }
            }
        }
    }
}