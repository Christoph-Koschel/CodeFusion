using System;
using System.IO;
using CodeFusion.VM;

namespace CodeFusion.ASM.Compiling;

public class Loader
{
    public readonly BinaryReader reader;

    public Loader(string path)
    {
        this.reader = new BinaryReader(new FileStream(path, FileMode.Open));
    }

    public Metadata ReadHeader()
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
            poolSize = BitConverter.ToUInt16(metadata, Metadata.POOL_OFFSET),
            programSize = BitConverter.ToUInt64(metadata, Metadata.PROGRAM_OFFSET)
        };
        return meta;
    }

    public RelocatableMetadata ReadRelocatableHeader() {
        byte[] metadata = reader.ReadBytes(RelocatableMetadata.METADATA_SIZE);
        RelocatableMetadata meta = new RelocatableMetadata {
            symbolCount = BitConverter.ToUInt16(metadata, RelocatableMetadata.SYMBOL_OFFSET),
            missingCount = BitConverter.ToUInt16(metadata, RelocatableMetadata.MISSING_OFFSET),
            addressCount = BitConverter.ToUInt16(metadata,RelocatableMetadata.ADDRESS_OFFSET)
        };
        return meta;
    }
}