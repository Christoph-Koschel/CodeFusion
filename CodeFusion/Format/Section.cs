namespace CodeFusion.Format;

public abstract class Section
{
    public byte type;
    public uint lenght;

    public abstract byte[] ToBytes();

    #region TYPES

    public const byte TYPE_PROGRAM = 0;
    public const byte TYPE_POOL = 1;
    public const byte TYPE_SYMBOL = 2;
    public const byte TYPE_MISSING = 3;
    public const byte TYPE_ADDRESS = 4;

    #endregion
}