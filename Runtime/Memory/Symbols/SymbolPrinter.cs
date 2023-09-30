using System;
using System.IO;
using IllusionScript.Runtime.Extension;

namespace IllusionScript.Runtime.Memory.Symbols;

internal static class SymbolPrinter
{
    public static void WriteTo(this Symbol symbol, TextWriter writer)
    {
        switch (symbol.symbolType)
        {
            case SymbolType.GlobalVariable:
                WriteGlobalVariable((GlobalVariableSymbol)symbol, writer);
                break;
            case SymbolType.LocalVariable:
                WriteLocalVariable((LocalVariableSymbol)symbol, writer);
                break;
            case SymbolType.Type:
                WriteType((TypeSymbol)symbol, writer);
                break;
            case SymbolType.Function:
                WriteFunction((FunctionSymbol)symbol, writer);
                break;
            case SymbolType.Parameter:
                WriteParameter((ParameterSymbol)symbol, writer);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void WriteGlobalVariable(GlobalVariableSymbol symbol, TextWriter writer)
    {
        writer.WriteKeyword(symbol.isReadOnly ? "const " : "let ");
        writer.WriteIdentifier(symbol.name);
        writer.WritePunctuation(": ");
        symbol.type.WriteTo(writer);
    }

    private static void WriteLocalVariable(LocalVariableSymbol symbol, TextWriter writer)
    {
        writer.WriteKeyword(symbol.isReadOnly ? "const " : "let ");
        writer.WriteIdentifier(symbol.name);
        writer.WritePunctuation(": ");
        symbol.type.WriteTo(writer);
    }

    private static void WriteType(TypeSymbol symbol, TextWriter writer)
    {
        writer.WriteIdentifier(symbol.name);
    }

    private static void WriteFunction(FunctionSymbol symbol, TextWriter writer)
    {
        writer.WriteKeyword("define ");
        writer.WriteIdentifier(symbol.name);
        writer.WritePunctuation("(");

        bool first = true;

        foreach (ParameterSymbol parameter in symbol.parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                writer.Write(", ");
            }

            parameter.WriteTo(writer);
        }

        writer.WritePunctuation("): ");
        symbol.returnType.WriteTo(writer);
    }

    private static void WriteParameter(ParameterSymbol symbol, TextWriter writer)
    {
        writer.WriteIdentifier(symbol.name);
        writer.WritePunctuation(": ");
        symbol.type.WriteTo(writer);
    }
}