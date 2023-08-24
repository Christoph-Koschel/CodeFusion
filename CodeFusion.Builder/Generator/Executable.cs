using System;
using System.IO;

namespace CodeFusion.Builder.Generator;

public partial class Maker {
    public static void MakeExecutable(string file, string outName, Platform platform) {
        MakeFolder("obj");
        MakeFolder("obj/cf");
        CopyFile(file, "obj/cf", true);
        MakeFolder("obj/refs");

        string part;
        if (platform == Platform.WINDOWS) {
            part = "win";
        } else if (platform == Platform.LINUX) {
            part = "linux";
        } else {
            throw new ArgumentOutOfRangeException();    
        }

        CopyFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", part, "image.o"), "obj");

        ExecuteLD("-r", "-b", "binary", Path.Combine("obj", "cf", Path.GetFileName(file)), "-o", "code.o");

        MakeFolder("bin");

        ExecuteGCC("obj/image.o", "obj/cf/code.o", "-o", $"bin/{outName}", "-L", "obj/refs");
    }
}