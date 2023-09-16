using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeFusion.Format;

public abstract class PairSection : Section
{
    public Dictionary<string, ulong> pool = new Dictionary<string, ulong>();

    public PairSection()
    {
        lenght = 0;
    }

    public override byte[] ToBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.Add(type);

        lenght = 0;
        foreach (KeyValuePair<string, ulong> item in pool)
        {
            lenght += (uint)(item.Key.Length + 2 + 8);
        }

        bytes.AddRange(BitConverter.GetBytes(lenght));

        foreach (KeyValuePair<string, ulong> item in pool)
        {
            bytes.AddRange(BitConverter.GetBytes((ushort)item.Key.Length));
            bytes.AddRange(item.Key.Select(c => (byte)c));
            bytes.AddRange(BitConverter.GetBytes(item.Value));
        }

        return bytes.ToArray();
    }
}