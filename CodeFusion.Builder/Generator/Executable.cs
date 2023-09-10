using System;
using System.IO;
using System.Runtime;

namespace CodeFusion.Builder.Generator;

public partial class Maker
{
    public static void MakeExecutable(string file, string outName, Platform platform)
    {
        MakeFolder("obj");
        MakeFolder("obj/cf");
        CopyFile(file, "obj/cf", true);
        RenameFile(Path.Combine("obj", "cf", Path.GetFileName(file)), "code.bin");
        MakeFolder("obj/refs");

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

        CopyFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", part, "image.o"), "obj", true);
        CopyFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", part, "table.o"), "obj", true);
        CopyFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", part, "loader.o"), "obj", true);
        ExecuteLD("-r", "-b", "binary", "cf/code.bin", "-o", "code.o");

        MakeFolder("bin");

        ExecuteGCC(  "-o",
            "../bin/" + outName,
            "./image.o",
            "./loader.o",
            "./table.o",
            "./code.o");
    }
}