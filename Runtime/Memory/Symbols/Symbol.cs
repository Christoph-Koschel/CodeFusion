using System.IO;

namespace IllusionScript.Runtime.Memory.Symbols;

public abstract class Symbol
{
    public readonly string name;

    internal Symbol(string name)
    {
        this.name = name;
    }

    public abstract SymbolType symbolType { get; }

    public void WriteTo(TextWriter writer)
    {
        SymbolPrinter.WriteTo(this, writer);
    }

    public override string ToString()
    {
        using (StringWriter writer = new StringWriter())
        {
            WriteTo(writer);
            return writer.ToString();
        }
    }
}