using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Parsing;

namespace IllusionScript.Runtime.Extension;

public static class TextWriterExtensions
{
    private static bool IsConsole(this TextWriter writer)
    {
        if (writer == Console.Out)
            return !Console.IsOutputRedirected;

        if (writer == Console.Error)
            return !Console.IsErrorRedirected &&
                   !Console.IsOutputRedirected;

        if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
            return true;

        return false;
    }

    private static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if (writer.IsConsole())
            Console.ForegroundColor = color;
    }

    private static void ResetColor(this TextWriter writer)
    {
        if (writer.IsConsole())
            Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter writer, SyntaxType type)
    {
        string? text = SyntaxFacts.GetText(type);

        writer.WriteKeyword(text);
    }

    public static void WriteKeyword(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Blue);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteIdentifier(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Gray);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteNumber(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkMagenta);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteString(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Cyan);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteSpace(this TextWriter writer)
    {
        writer.WritePunctuation(" ");
    }

    public static void WritePunctuation(this TextWriter writer, SyntaxType kind)
    {
        string? text = SyntaxFacts.GetText(kind);
        writer.WritePunctuation(text);
    }

    public static void WritePunctuation(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkGray);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics)
    {
        foreach (Diagnostic diagnostic in diagnostics.Where(d => d.location.text == null))
        {
            if (IsConsole(writer))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            writer.WriteLine(diagnostic.message);
            writer.ResetColor();
        }

        foreach (Diagnostic diagnostic in diagnostics.Where(d => d.location.text != null))
        {
            SourceText text = diagnostic.location.text;
            string filename = diagnostic.location.text.filename;
            int startLine = diagnostic.location.startLine + 1;
            int startCharacter = diagnostic.location.startCharacter + 1;
            int endLine = diagnostic.location.endLine + 1;
            int endCharacter = diagnostic.location.endCharacter + 1;
            TextSpan span = diagnostic.location.span;
            int lineIndex = text.GetLineIndex(span.start);
            TextLine line = text.lines[lineIndex];

            if (IsConsole(writer))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }

            writer.Write($"{filename}({startLine}:{startCharacter},{endLine}:{endCharacter}): ");
            writer.WriteLine(diagnostic);
            writer.ResetColor();
            TextSpan prefixSpan = TextSpan.FromBounds(line.start, diagnostic.location.span.start);
            TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.location.span.end, line.end);

            string prefix = text.ToString(prefixSpan);
            string error = text.ToString(diagnostic.location.span);
            string suffix = text.ToString(suffixSpan);

            writer.Write(prefix);

            if (IsConsole(writer))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }

            writer.Write(error);
            writer.ResetColor();

            writer.Write(suffix);

            writer.Write("\n\n");
        }
    }
}