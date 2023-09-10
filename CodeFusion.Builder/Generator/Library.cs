using System;
using System.IO;

namespace CodeFusion.Builder.Generator;

public partial class Maker
{
    public static void MakeLibrary(string file, string outName, Platform platform)
    {
        MakeFolder("obj");
        MakeFolder("obj/cf");
        CopyFile(file, "obj/cf", true);
        RenameFile(Path.Combine("obj", "cf", Path.GetFileName(file)), "code.bin");

        string part;
        if (platform == Platform.WINDOWS)
        {
            part = "win";
        }
        else if (platform == Platform.LINUX)
        {
            part = "linux";
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }

        CopyFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", part, "library.o"), "obj", true);
        ExecuteLD("-r", "-b", "binary", "cf/code.bin", "-o", "code.o");

        MakeFolder("bin");


        ExecuteGCC("-o",
            "../bin/" + outName,
            "-s",
            "--shared",
            "./library.o",
            "./code.o");
    }
}