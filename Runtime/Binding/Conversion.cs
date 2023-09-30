using IllusionScript.Runtime.Memory.Symbols;

namespace IllusionScript.Runtime.Binding;

internal sealed class Conversion
{
    public static readonly Conversion None = new(false, false, false);
    public static readonly Conversion Identity = new(true, true, true);
    public static readonly Conversion Implicit = new(true, false, true);
    public static readonly Conversion Explicit = new Conversion(true, false, false);

    public static Conversion Classify(TypeSymbol from, TypeSymbol to)
    {
        if (from == to)
        {
            return Identity;
        }

        if (from.HasFlag(TypeSymbol.Attributes.INTEGER) && to.HasFlag(TypeSymbol.Attributes.INTEGER))
        {
            return Implicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.FLOAT) && to.HasFlag(TypeSymbol.Attributes.FLOAT))
        {
            return Implicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER) && to.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER))
        {
            return Implicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.INTEGER) && to.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER))
        {
            return Implicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER) && to.HasFlag(TypeSymbol.Attributes.INTEGER))
        {
            return Implicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.INTEGER) && to.HasFlag(TypeSymbol.Attributes.FLOAT))
        {
            return Explicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.FLOAT) && to.HasFlag(TypeSymbol.Attributes.INTEGER))
        {
            return Explicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER) && to.HasFlag(TypeSymbol.Attributes.FLOAT))
        {
            return Explicit;
        }

        if (from.HasFlag(TypeSymbol.Attributes.FLOAT) && to.HasFlag(TypeSymbol.Attributes.UNSIGNED_INTEGER))
        {
            return Explicit;
        }

        if (from != TypeSymbol.@void && to == TypeSymbol.@object)
        {
            return Implicit;
        }

        return None;
    }

    public readonly bool exists;
    public readonly bool isIdentity;
    public readonly bool isImplicit;
    public bool isExplicit => exists && !isImplicit;

    private Conversion(bool exists, bool isIdentity, bool isImplicit)
    {
        this.exists = exists;
        this.isIdentity = isIdentity;
        this.isImplicit = isImplicit;
    }
}