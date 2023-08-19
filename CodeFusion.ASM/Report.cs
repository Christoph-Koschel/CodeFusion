using System;
using CodeFusion.ASM.Lexing;

namespace CodeFusion.ASM;

public class Report
{
    public static bool sentErros { get; private set; } = false;

    public static void PrintReport(SourceFile source, Token token, string message)
    {
        sentErros = true;
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;

        Console.WriteLine($"{source.path}({source.GetLine(token.span)}) {message}");
        Console.ForegroundColor = color;
    }
}