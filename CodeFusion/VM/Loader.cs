using System;
using System.IO;

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
            poolSize = BitConverter.ToUInt16(metadata, Metadata.POOL_OFFSET),
            programSize = BitConverter.ToUInt64(metadata, Metadata.PROGRAM_OFFSET),
            symbolSize = BitConverter.ToUInt32(metadata, Metadata.SYMBOL_OFFSET)
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

    public static RelocatableMetadata ReadRelocatableHeader(ref BinaryReader reader)
    {
        byte[] metadata = reader.ReadBytes(RelocatableMetadata.METADATA_SIZE);
        RelocatableMetadata meta = new RelocatableMetadata
        {
            symbolCount = BitConverter.ToUInt16(metadata, RelocatableMetadata.SYMBOL_OFFSET),
            missingCount = BitConverter.ToUInt16(metadata, RelocatableMetadata.MISSING_OFFSET),
            addressCount = BitConverter.ToUInt16(metadata, RelocatableMetadata.ADDRESS_OFFSET)
        };
        return meta;
    }
}