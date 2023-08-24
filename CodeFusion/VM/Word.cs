using System;

namespace CodeFusion.VM;

public unsafe struct Word
{
    private ulong value;

    public Word(ulong value)
    {
        this.value = value;
    }
    public Word(long value)
    {
        this.value = Convert.ToUInt64(value);
    }

    public Word(double value) {
        this.value = BitConverter.ToUInt64(BitConverter.GetBytes(value), 0);
    }

    public Word(void* value)
    {
        this.value = (ulong)value;
    }



    public static Word operator +(Word one, Word other)
    {
        return new Word(one.asU64 + other.asU64);
    }

    public ulong asU64 => value;
    public long asI64 => unchecked((long)value + long.MinValue);
    public double asF64 => BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
    public void* asPtr => (void*)value;

    public static Word zero => new Word(0);
}