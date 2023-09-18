using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeFusion.Format;

public class MemoryAddressSection : Section
{
    public List<ulong> addresses = new List<ulong>();

    public MemoryAddressSection()
    {
        type = TYPE_MEMORY_ADDRESS;
        lenght = 0;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.Add(type);
        bytes.AddRange(BitConverter.GetBytes((uint)addresses.Count * 8));

        bytes.AddRange(addresses.SelectMany(BitConverter.GetBytes));

        return bytes.ToArray();
    }
}