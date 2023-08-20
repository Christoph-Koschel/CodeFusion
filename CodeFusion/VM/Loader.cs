using System;
using System.IO;

namespace CodeFusion.VM;

public class Loader
{
    public static void LoadFile(ref VmCodeFusion cf, string path)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
        byte[] metadata = reader.ReadBytes(Metadata.METADATA_SIZE);
        Metadata meta = new Metadata
        {
            magic = new[]
            {
                (char)metadata[0], (char)metadata[1], (char)metadata[2]
            },
            version = BitConverter.ToUInt16(metadata, Metadata.VERSION_OFFSET),
            poolSize = BitConverter.ToUInt16(metadata, Metadata.POOL_OFFSET),
            programSize = BitConverter.ToUInt64(metadata, Metadata.PROGRAM_OFFSET)
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

        if ((meta.flags & Metadata.EXECUTABLE) != Metadata.EXECUTABLE) {
            Console.Error.WriteLine("Program is not executable");
            Environment.Exit(1);
        }

        cf.programCounter = meta.entryPoint;

        ulong i;
        for (i = 0; i < meta.poolSize; i++)
        {
            ulong address = BitConverter.ToUInt64(reader.ReadBytes(8));
            ushort size = BitConverter.ToUInt16(reader.ReadBytes(2));
            cf.addressPool.Add(address, size);
        }
        for (i = 0; i < meta.programSize; i++)
        {
            Inst inst = new Inst
            {
                opcode = reader.ReadByte()
            };
            if (Opcode.HasOperand(inst.opcode))
            {
                byte size = reader.ReadByte();
                if (size == 0)
                {
                    inst.operand = new Word(0);
                }
                else
                {
                    byte[] bytes = new byte[8];
                    for (int j = 0; j < size; j++)
                    {
                        bytes[j] = reader.ReadByte();
                    }
                    inst.operand = new Word(BitConverter.ToUInt64(bytes));
                }
            }
            cf.program[cf.programSize++] = inst;
        }
    }
}