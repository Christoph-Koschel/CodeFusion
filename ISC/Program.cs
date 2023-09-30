using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using IllusionScript.Runtime;
using IllusionScript.Runtime.Diagnostics;
using IllusionScript.Runtime.Extension;
using IllusionScript.Runtime.Parsing;
using Mono.Options;

namespace IllusionScript.ISC;

internal static class Program
{
    private static int Main(string[] args)
    {
        List<string> references = new List<string>();
        List<string> sourcePaths = new List<string>();
        string outputPath = null;
        bool helpRequested = false;
        OptionSet options = new OptionSet()
        {
            "usage: ils <source-paths> [options]",
            {
                "r=", "The {path} of an assembly to reference", v => references.Add(v)
            },
            {
                "o=", "The {path} of the output file", v => outputPath = v
            },
            {
                "<>", v => sourcePaths.Add(v)
            },
            {
                "h|help", v => helpRequested = true
            }
        };

        options.Parse(args);
        if (helpRequested)
        {
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        if (sourcePaths.Count == 0)
        {
            Console.Error.WriteLine("ERROR: Need at least one source file");
            return 1;
        }

        outputPath ??= Directory.GetCurrentDirectory();

        List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
        bool hasErrors = false;
        foreach (string path in sourcePaths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"ERROR: File '{path}' doesn't exists");
                hasErrors = true;
                continue;
            }

            SyntaxTree syntaxTree = SyntaxTree.Load(path);
            if (syntaxTree.diagnostics.Any())
            {
                hasErrors = true;
            }
            syntaxTrees.Add(syntaxTree);
        }

        foreach (string path in references)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"ERROR: File '{path}' doesn't exists");
                hasErrors = true;
            }
        }

        if (hasErrors)
        {
            foreach (SyntaxTree syntaxTree in syntaxTrees)
            {
                Console.Out.WriteDiagnostics(syntaxTree.diagnostics);
            }
            return 1;
        }

        Compilation compilation = Compilation.Create(syntaxTrees.ToArray());
        ImmutableArray<Diagnostic> diagnostics = compilation.Emit(outputPath);

        if (diagnostics.Any())
        {
            Console.Out.WriteDiagnostics(diagnostics);
            return 1;
        }

        return 0;
    }
}