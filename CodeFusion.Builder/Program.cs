using System;
using System.IO;
using CodeFusion.Builder.Generator;
using CodeFusion.VM;

namespace CodeFusion.Builder;

public enum OutputType
{
    EXE,
    LIB
}

public class Program
{
    public static void Main(string[] args)
    {
        string file = null;
        Platform platform = Platform.WINDOWS;
        OutputType outputType = OutputType.EXE;
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
            else if (args[i] == "-t")
            {
                string value = args[++i];
                if (value == "exe")
                {
                    outputType = OutputType.EXE;
                }
                else if (value == "lib")
                {
                    outputType = OutputType.LIB;
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

        switch (outputType)
        {
            case OutputType.EXE:
            {
                if (!outputName.EndsWith(".exe") && platform == Platform.WINDOWS)
                {
                    outputName += ".exe";
                }
                break;
            }
            case OutputType.LIB:
                switch (platform)
                {
                    case Platform.LINUX:
                    {
                        if (!outputName.EndsWith(".so"))
                        {
                            outputName += ".so";
                        }
                        break;
                    }
                    case Platform.WINDOWS:
                    {
                        if (!outputName.EndsWith(".dll"))
                        {
                            outputName += ".dll";
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open));
        Metadata metadata = Loader.ReadMainHeader(ref reader);
        reader.Close();

        if ((metadata.flags & Metadata.CONTAINS_ERRORS) == Metadata.CONTAINS_ERRORS)
        {
            Console.Error.WriteLine("File contains errors");
            Environment.Exit(1);
        }

        if (outputType == OutputType.EXE)
        {
            if ((metadata.flags & Metadata.EXECUTABLE) != Metadata.EXECUTABLE)
            {
                Console.Error.WriteLine("File must be executable");
                Environment.Exit(1);
            }

            Maker.MakeExecutable(file, outputName, platform);
        }
        else if (outputType == OutputType.LIB)
        {
            if ((metadata.flags & Metadata.LIBRARY) != Metadata.LIBRARY)
            {
                Console.Error.WriteLine("File must be a library");
                Environment.Exit(1);
            }

            Maker.MakeLibrary(file, outputName, platform);
        }
    }
}