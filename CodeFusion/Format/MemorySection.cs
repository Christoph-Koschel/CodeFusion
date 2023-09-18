using System;
using System.Collections.Generic;

namespace CodeFusion.Format;

public class MemorySection : Section
{
    public List<byte> data = new List<byte>();

    public MemorySection()
    {
        type = TYPE_MEMORY;
        lenght = 0;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.Add(type);

        lenght = (uint)data.Count;
        bytes.AddRange(BitConverter.GetBytes(lenght));
        bytes.AddRange(data);

        return bytes.ToArray();
    }
}