using System;
using System.IO;
using CodeFusion.Execution;
using CodeFusion.Test;
using CodeFusion.VM;

namespace CodeFusion;

public static class Program
{
    public static void Main(string[] args)
    {
        if (!File.Exists("./test"))
        {
            ProgramMaker.Make("./test");
            Console.Write(new FileInfo("./test").Length);
            Console.WriteLine("bytes");
        }

        VmCodeFusion cf = new VmCodeFusion();
        Loader.LoadFile(ref cf, "./test");

        for (ulong i = 0; i < cf.programSize; i++)
        {
            Console.WriteLine(Opcode.GetOpcodeName(cf.program[i].opcode));
        }

        Console.WriteLine($"Program Size: {cf.programSize}");

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