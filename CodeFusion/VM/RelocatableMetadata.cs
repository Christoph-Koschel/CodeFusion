namespace CodeFusion.VM;

public struct RelocatableMetadata {
    public ushort symbolCount;
    public ushort missingCount;
    public ushort addressCount;

    public const int SYMBOL_OFFSET = 0;
    public const int MISSING_OFFSET = SYMBOL_OFFSET + 2;
    public const int ADDRESS_OFFSET = MISSING_OFFSET + 2;
    public const int METADATA_SIZE = ADDRESS_OFFSET + 2;
}