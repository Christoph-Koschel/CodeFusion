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
        ExecuteLD("-r", "-b", "binary", "cf/code.bin", "-o", "code.o");

        MakeFolder("bin");

        if (platform == Platform.WINDOWS)
        {
            string libCBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libc");
            ExecuteLD(
                "-m",
                "i386pep",
                "-Bdynamic",
                "-o",
                "../bin/" + outName,
                Path.Combine(libCBase, "crt2.o"),
                Path.Combine(libCBase, "crtbegin.o"),
                "-L" + libCBase,
                "./code.o",
                "./image.o",
                "./table.o",
                "-lmingw32",
                "-lgcc",
                "-lgcc_eh",
                "-lmoldname",
                "-lmingwex",
                "-lmsvcrt",
                "-lpthread",
                "-ladvapi32",
                "-lshell32",
                "-luser32",
                "-lkernel32",
                "-liconv",
                "-lmingw32",
                "-lgcc",
                "-lgcc_eh",
                "-lmoldname",
                "-lmingwex",
                "-lmsvcrt",
                Path.Combine(libCBase, "crtend.o")
            );
        }
        else if (platform == Platform.LINUX)
        {
            // TODO Write compilation for Linux
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}