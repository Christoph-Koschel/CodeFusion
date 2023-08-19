using System;
using System.IO;

namespace CodeFusion.ASM.Lexing;

public struct SourceFile
{
    public readonly string path;
    public readonly string content;
    public readonly string[] lines;

    public SourceFile(string path)
    {
        this.path = path;
        this.content = File.ReadAllText(path);
        this.lines = this.content.Split(Environment.NewLine);
    }

    public int GetLine(Span span)
    {
        int totalLength = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            int length = totalLength + lines[i].Length;
            if (span.start > totalLength && span.end <= length)
            {
                return i + 1;
            }
            totalLength += lines[i].Length + Environment.NewLine.Length;
        }

        return -1;
    }
}
