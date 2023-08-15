namespace CodeFusion.VM;

public struct Inst
{
    public byte opcode;
    public Word operand;

    public Inst(byte opcode, Word operand)
    {
        this.opcode = opcode;
        this.operand = operand;
    }
    public Inst(byte opcode)
    {
        this.opcode = opcode;
        this.operand = new Word(0);
    }
}