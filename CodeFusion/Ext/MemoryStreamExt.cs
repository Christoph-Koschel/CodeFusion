using System.IO;

namespace CodeFusion.Ext;

public static class MemoryStreamExt
{
    public static void Write(this MemoryStream stream, byte b)
    {
        stream.Write(new []
        {
            b
        });
    }
}