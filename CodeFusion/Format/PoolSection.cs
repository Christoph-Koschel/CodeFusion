using System;
using System.Collections.Generic;
using CodeFusion.VM;

namespace CodeFusion.Format;

public class PoolSection : Section
{
    public Dictionary<Word, ushort> pool = new Dictionary<Word, ushort>();

    public PoolSection()
    {
        lenght = 0;
        type = TYPE_POOL;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.Add(type);

        lenght = 0;
        foreach (KeyValuePair<Word, ushort> _ in pool)
        {
            lenght += 2 + 8;
        }

        bytes.AddRange(BitConverter.GetBytes(lenght));

        foreach (KeyValuePair<Word, ushort> item in pool)
        {
            bytes.AddRange(BitConverter.GetBytes(item.Key.asU64));
            bytes.AddRange(BitConverter.GetBytes(item.Value));
        }

        return bytes.ToArray();
    }
}