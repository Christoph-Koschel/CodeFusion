using System;
using System.IO;
using CodeFusion.Execution;
using CodeFusion.VM;

namespace CodeFusion;

public static class Program
{
    public static void Main(string[] args)
    {
        string path = null;
        
        foreach(string arg in args) {
            /// Prework for future options
            if (false) {

            } else {
                path = arg;
            }
        }

        if (path == null) {
            Console.Error.WriteLine("No program provieded");
            Environment.Exit(1);
        }

        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Program '{path}' don't exists");
            Environment.Exit(1);
        }

        VmCodeFusion cf = new VmCodeFusion();
        Loader.LoadFile(ref cf, path);

        Runner runner = new Runner();
        Error error;
        while (true)
        {
            error = runner.ExecuteInst(ref cf);
            if (error != Error.OK)
            {
                break;
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Stack:");
            for (ulong i = 0; i < cf.stackSize; i++)
            {
                Console.WriteLine($"    [{i}] {cf.stack[i].asU64}");
            }
            Console.ReadKey();
        }

        Console.WriteLine($"VM stops with code '{error}'");
    }
}