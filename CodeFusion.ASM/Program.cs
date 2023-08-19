using CodeFusion.ASM.Parsing;

namespace CodeFusion.ASM;

class Program {
    public static void Main(string[] args) {
        const string TMP_PATH = @"C:\Users\kosch\Desktop\Workbench\CodeFusion\test.cf";
        Parser parser = new Parser(TMP_PATH);
    }
}