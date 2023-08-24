using System;
using CodeFusion.ASM.Lexing;

namespace CodeFusion.ASM;

public class Report
{
    public static bool sentErros { get; private set; } = false;
    public static bool sentWarning { get; private set; } = false;

    public static void PrintReport(SourceFile source, Token token, string message)
    {
        sentErros = true;
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;

        Console.WriteLine($"{source.path}({source.GetLine(token.span)}) {message}");
        Console.ForegroundColor = color;
    }

    public static void PrintReport(string path, string message)
    {
        sentErros = true;
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;

        Console.WriteLine($"{path} {message}");
        Console.ForegroundColor = color;

    }

    public static void PrintWarning(SourceFile source, Token token, string message)
    {
        sentWarning = true;
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.WriteLine($"{source.path}({source.GetLine(token.span)}) {message}");
        Console.ForegroundColor = color;
    }

    internal static void PrintWarning(string path, string message)
    {
        sentWarning = true;
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.WriteLine($"{path} {message}");
        Console.ForegroundColor = color;
    }
}