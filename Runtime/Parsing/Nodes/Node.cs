using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Extension;
using IllusionScript.Runtime.Lexing;

namespace IllusionScript.Runtime.Parsing.Nodes;

public abstract class Node
{
    public readonly SyntaxTree syntaxTree;
    public abstract SyntaxType type { get; }
    public TextLocation location => new TextLocation(syntaxTree.text, span);

    protected Node(SyntaxTree syntaxTree)
    {
        this.syntaxTree = syntaxTree;
    }

    public virtual TextSpan span
    {
        get
        {
            TextSpan first = GetChildren().First().span;
            TextSpan last = GetChildren().Last().span;

            return TextSpan.FromBounds(first.start, last.end);
        }
    }

    public IEnumerable<Node> GetChildren()
    {
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            if (typeof(Node).IsAssignableFrom(field.FieldType))
            {
                Node child = (Node)field.GetValue(this);
                if (child != null)
                {
                    yield return child;
                }
            }
            else if (typeof(IEnumerable<Node>).IsAssignableFrom(field.FieldType))
            {
                IEnumerable<Node> children = (IEnumerable<Node>)field.GetValue(this);
                foreach (Node child in children)
                {
                    if (child != null)
                    {
                        yield return child;
                    }
                }
            }
            else if (typeof(SeparatedSyntaxList).IsAssignableFrom(field.FieldType))
            {
                SeparatedSyntaxList list = (SeparatedSyntaxList)field.GetValue(this);
                foreach (Node listItem in list.GetWithSeparators())
                {
                    yield return listItem;
                }
            }
        }
    }

    public void WriteTo(TextWriter writer)
    {
        PrettyPrint(writer, this);
    }

    private static void PrettyPrint(TextWriter writer, Node node, string indent = "", bool isLast = true)
    {
        bool isConsole = writer == Console.Out;

        string marker = isLast ? "└──" : "├──";

        if (isConsole)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        writer.Write(indent);
        writer.Write(marker);

        if (isConsole)
        {
            Console.ForegroundColor = node is Token ? ConsoleColor.DarkBlue : ConsoleColor.DarkCyan;
        }

        writer.Write(node.type);

        if (isConsole)
        {
            Console.ForegroundColor = node is Token ? ConsoleColor.DarkBlue : ConsoleColor.DarkCyan;
        }

        if (node is Token t && t.value != null)
        {
            writer.Write(" ");
            writer.Write(t.value);
        }

        if (isConsole)
        {
            Console.ResetColor();
        }

        writer.Write("\n");


        indent += isLast ? "   " : "│  ";

        Node lastChild = node.GetChildren().LastOrDefault();

        foreach (Node child in node.GetChildren())
        {
            PrettyPrint(writer, child, indent, child == lastChild);
        }
    }

    public override string ToString()
    {
        using StringWriter writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}