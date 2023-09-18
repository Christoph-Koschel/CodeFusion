using System;
using System.IO;
using CodeFusion.Format;

namespace CodeFusion.VM;

public static class Loader
{
    public static Metadata ReadMainHeader(ref BinaryReader reader)
    {
        byte[] metadata = reader.ReadBytes(Metadata.METADATA_SIZE);
        Metadata meta = new Metadata
        {
            magic = new[]
            {
                (char)metadata[0], (char)metadata[1], (char)metadata[2]
            },
            version = BitConverter.ToUInt16(metadata, Metadata.VERSION_OFFSET),
            flags = metadata[Metadata.FLAGS_OFFSET],
            entryPoint = BitConverter.ToUInt64(metadata, Metadata.ENTRYPOINT_OFFSET),
            sectionCount = metadata[Metadata.SECTION_COUNT_OFFSET]
        };

        if (meta.magic[0] != '.' || meta.magic[1] != 'C' || meta.magic[2] != 'F')
        {
            Console.Error.WriteLine("Program has not the correct file format");
            Environment.Exit(1);
        }

        if (meta.version != Metadata.CURRENT_VERSION)
        {
            Console.Error.WriteLine($"Program is not compatible with the VM file expect '{meta.version}' VM has '{Metadata.CURRENT_VERSION}'");
            Environment.Exit(1);
        }
        return meta;
    }

    public static Section ReadSection(ref BinaryReader reader)
    {
        byte type = reader.ReadByte();
        uint lenght = reader.ReadUInt32();
        byte[] content = reader.ReadBytes((int)lenght);
        Console.WriteLine("Type"+type);
        Console.WriteLine("Length"+lenght);
        switch (type)
        {
            case Section.TYPE_POOL:
            {
                PoolSection poolSection = new PoolSection();
                poolSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    poolSection.pool.Add(new Word(BitConverter.ToUInt64(content, i)), BitConverter.ToUInt16(content, i + 8));
                    i += 10;
                }
                return poolSection;
            }
            case Section.TYPE_PROGRAM:
            {
                ProgramSection programSection = new ProgramSection();
                programSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    byte opcode = content[i++];
                    if (!Opcode.HasOperand(opcode))
                    {
                        programSection.program.Add(new Inst(opcode));
                        continue;
                    }
                    byte size = content[i++];
                    if (size == 0)
                    {
                        programSection.program.Add(new Inst(opcode));
                        continue;
                    }
                    byte[] bytes = new byte[8];
                    for (int j = 0; j < size; j++, i++)
                    {
                        bytes[j] = content[i];
                    }
                    programSection.program.Add(new Inst(opcode, new Word(BitConverter.ToUInt64(bytes))));
                }
                return programSection;
            }
            case Section.TYPE_SYMBOL:
            {
                SymbolSection symbolSection = new SymbolSection();
                symbolSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    ushort size = BitConverter.ToUInt16(content, i);
                    i += 2;
                    string name = "";
                    for (int j = 0; j < size; j++, i++)
                    {
                        name += (char)content[i];
                    }
                    symbolSection.pool.Add(name, BitConverter.ToUInt64(content, i));
                    i += 8;
                }
                return symbolSection;
            }
            case Section.TYPE_MISSING:
            {
                MissingSection missingSection = new MissingSection();
                missingSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    ushort size = BitConverter.ToUInt16(content, i);
                    i += 2;
                    string name = "";
                    for (int j = 0; j < size; j++, i++)
                    {
                        name += (char)content[i];
                    }
                    missingSection.pool.Add(name, BitConverter.ToUInt64(content, i));
                    i += 8;
                }
                return missingSection;
            }
            case Section.TYPE_ADDRESS:
            {
                AddressSection addressSection = new AddressSection();
                addressSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    addressSection.addresses.Add(BitConverter.ToUInt64(content, i));
                    i += 8;
                }
                return addressSection;
            }
            case Section.TYPE_MEMORY:
            {
                MemorySection memorySection = new MemorySection();
                memorySection.lenght = lenght;
                memorySection.data.AddRange(content);
                return memorySection;
            }
            case Section.TYPE_MEMORY_SYMBOL:
            {
                MemorySymbolSection symbolSection = new MemorySymbolSection();
                symbolSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    ushort size = BitConverter.ToUInt16(content, i);
                    i += 2;
                    string name = "";
                    for (int j = 0; j < size; j++, i++)
                    {
                        name += (char)content[i];
                    }
                    symbolSection.pool.Add(name, BitConverter.ToUInt64(content, i));
                    i += 8;
                }
                return symbolSection;
            }
            case Section.TYPE_MEMORY_ADDRESS:
            {
                MemoryAddressSection addressSection = new MemoryAddressSection();
                addressSection.lenght = lenght;
                int i = 0;
                while (i < lenght)
                {
                    addressSection.addresses.Add(BitConverter.ToUInt64(content, i));
                    i += 8;
                }
                return addressSection;
            }
            default:
                return null;
        }
    }
}