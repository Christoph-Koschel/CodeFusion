namespace CodeFusion.VM;

public struct Metadata
{
    public char[] magic;
    public ushort version;
    public byte flags;
    public ulong entryPoint;
    public ushort poolSize;
    public ulong programSize;
    public uint symbolSize;

    public const ushort CURRENT_VERSION = 1;
    public const int VERSION_OFFSET = 3;
    public const int FLAGS_OFFSET = VERSION_OFFSET + 2;
    public const int ENTRYPOINT_OFFSET = FLAGS_OFFSET + 1;
    public const int POOL_OFFSET = ENTRYPOINT_OFFSET + 8;
    public const int PROGRAM_OFFSET = POOL_OFFSET + 2;
    public const int SYMBOL_OFFSET = PROGRAM_OFFSET + 8;
    public const int METADATA_SIZE = SYMBOL_OFFSET + 4;

    #region FLAGS

    public const byte RELOCATABLE = 0b1;
    public const byte EXECUTABLE = 0b10;
    public const byte CONTAINS_ERRORS = 0b100;
    public const byte LIBRARY = 0b1000;

    #endregion
}