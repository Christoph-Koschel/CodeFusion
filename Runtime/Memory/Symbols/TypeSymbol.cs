using System.Collections.Generic;
using System.Linq;

namespace IllusionScript.Runtime.Memory.Symbols;

public sealed class TypeSymbol : Symbol
{
    public readonly int size;
    private readonly TypeSymbol maxCapacity;
    private readonly IEnumerable<Attributes> attributes;
    public static readonly TypeSymbol i64 = new("i64", 8, null, Attributes.INTEGER);
    public static readonly TypeSymbol f64 = new("f64", 8, null, Attributes.FLOAT);
    public static readonly TypeSymbol u64 = new("u64", 8, null, Attributes.UNSIGNED_INTEGER);
    public static readonly TypeSymbol i32 = new("i32", 8, i64, Attributes.INTEGER);
    public static readonly TypeSymbol f32 = new("f32", 8, f64, Attributes.FLOAT);
    public static readonly TypeSymbol u32 = new("u32", 8, u64, Attributes.UNSIGNED_INTEGER);
    public static readonly TypeSymbol i16 = new("i16", 8, i64, Attributes.INTEGER);
    public static readonly TypeSymbol u16 = new("u16", 8, u64, Attributes.UNSIGNED_INTEGER);
    public static readonly TypeSymbol i8 = new("i8", 8, i64, Attributes.INTEGER);
    public static readonly TypeSymbol u8 = new("u8", 8, u64, Attributes.UNSIGNED_INTEGER);
    public static readonly TypeSymbol boolean = new("boolean", 1, null, Attributes.UNSIGNED_INTEGER);
    public static readonly TypeSymbol @string = new("string", 8, null);
    public static readonly TypeSymbol @void = new("void", 0, null);
    public static readonly TypeSymbol @object = new("object", 8, null);
    public static readonly TypeSymbol error = new("?", 0, null);

    public static readonly TypeSymbol[] primitives =
    {
        i64, f64, u64, i32, f32, u32, i16, u16, i8, u8, boolean, @string, @object
    };

    private TypeSymbol(string name, int size, TypeSymbol maxCapacity, params Attributes[] attributes) : base(name)
    {
        this.size = size;
        this.maxCapacity = maxCapacity;
        this.attributes = attributes;
    }

    public override SymbolType symbolType => SymbolType.Type;

    public bool HasFlag(Attributes attribute) => attributes.Contains(attribute);

    public TypeSymbol ToMaxCapacity() => maxCapacity ?? this;

    public enum Attributes
    {
        INTEGER,
        FLOAT,
        UNSIGNED_INTEGER,
    }
}