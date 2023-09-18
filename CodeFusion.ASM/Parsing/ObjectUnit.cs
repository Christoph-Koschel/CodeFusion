using System.Linq;
using CodeFusion.Format;

namespace CodeFusion.ASM.Parsing;

public struct ObjectUnit
{
    public BinFile file;
    public string path;

    public ObjectUnit(BinFile file, string path)
    {
        this.file = file;
        this.path = path;
    }

    public ulong programLength
    {
        get
        {
            ulong length = 0;
            foreach (Section section in file.sections)
            {
                if (section is ProgramSection programSection)
                {
                    length += programSection.lenght;
                }
            }
            return length;
        }
    }

    public ulong memoryLength
    {
        get
        {
            ulong length = 0;
            foreach (Section section in file.sections)
            {
                if (section is MemorySection memorySection)
                {
                    length += memorySection.lenght;
                }
            }
            return length;
        }
    }
}