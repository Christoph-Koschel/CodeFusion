using System;
using System.Diagnostics;
using System.IO;

namespace CodeFusion.Builder.Generator;

public partial class Maker
{
    private static void MakeFolder(string path)
    {
        if (Directory.Exists(path))
        {
            return;
        }

        Directory.CreateDirectory(path);
    }

    private static void CopyFile(string src, string dstFolder, bool overwrite = false)
    {
        File.Copy(src, Path.Combine(dstFolder, Path.GetFileName(src)), overwrite);
    }

    private static void ExecuteGCC(params string[] arguments)
    {
        if (OperatingSystem.IsWindows())
        {
            ExecuteProgram(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gcc.exe"), arguments);
        }
        ExecuteProgram("gcc", arguments);
    }

    private static void ExecuteLD(params string[] arguments)
    {
        if (OperatingSystem.IsWindows())
        {
            ExecuteProgram(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ld.exe"), arguments);
        }
        ExecuteProgram("ld", arguments);
    }

    private static void ExecuteProgram(string executable, params string[] arguments)
    {
        ProcessStartInfo info = new ProcessStartInfo(executable)
        {
            WorkingDirectory = Directory.GetCurrentDirectory()
        };
        foreach (string argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }
        Process process = Process.Start(info);
        process.WaitForExit();
    }
}