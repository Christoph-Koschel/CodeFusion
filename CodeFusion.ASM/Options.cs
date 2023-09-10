using System;

namespace CodeFusion.ASM;

public class Options
{
    public static readonly Options INSTANCE = new Options();
    public string[] files = Array.Empty<string>();
    public OutputType outputType = OutputType.EXECUTABLE;
    public bool combine = false;
    public string output = "a.bin";
    public string entryPoint = string.Empty;
}