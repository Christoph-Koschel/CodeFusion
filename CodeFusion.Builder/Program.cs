using System;
using System.ComponentModel;
using System.IO;
using CodeFusion.Builder.Generator;
using CodeFusion.VM;

namespace CodeFusion.Builder;

public class Program
{
    public static void Main(string[] args)
    {
        string file = null;
        Platform platform = Platform.WINDOWS;
        string outputName = "a";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-p")
            {
                string value = args[++i];
                if (value == "win")
                {
                    platform = Platform.WINDOWS;
                }
                else if (value == "linux")
                {
                    platform = Platform.LINUX;
                }
            }
            else
            {
                file = args[i];
            }
        }

        if (file == null)
        {
            Console.Error.WriteLine("No file provided");
            Environment.Exit(1);
        }

        if (!outputName.EndsWith(".exe") && platform == Platform.WINDOWS)
        {
            outputName += ".exe";
        }

        BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open));
        Metadata metadata = Loader.ReadMainHeader(ref reader);
        if ((metadata.flags & Metadata.EXECUTABLE) != Metadata.EXECUTABLE)
        {
            Console.Error.WriteLine("File must be executable");
            Environment.Exit(1);
        }

        if ((metadata.flags & Metadata.CONTAINS_ERRORS) == Metadata.CONTAINS_ERRORS)
        {
            Console.Error.WriteLine("File contains errors");
            Environment.Exit(1);
        }
        // TODO Implement static/dynamic library compilation
        Maker.MakeExecutable(file, outputName, platform);
    }
}