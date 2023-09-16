using System;
using System.IO;
using CodeFusion.VM;

namespace CodeFusion.Dump;

public class Program
{
    public static void Main(string[] args)
    {
        string path = null;
        bool mainHeader = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-h")
            {
                mainHeader = true;
            }
            else
            {
                path = args[i];
            }
        }

        if (path == null)
        {
            Console.Error.WriteLine("Missing file");
            Environment.Exit(1);
        }

        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
        Metadata metadata = Loader.ReadMainHeader(ref reader);
        if (mainHeader)
        {
            Console.WriteLine("Main Header:");
            Console.WriteLine("{0, 15}: {1, -30}", "EntryPoint", metadata.entryPoint);
            Console.WriteLine("{0, 15}: {1, -30}", "VM Version", metadata.version);
            Console.WriteLine("{0, 15}: {1, -30}", "VM Flags", Convert.ToString(metadata.flags, 2));
            Console.WriteLine("{0, 15}: {1, -30}", "Sections", metadata.sectionCount);
            Console.WriteLine();
        }
    }
}