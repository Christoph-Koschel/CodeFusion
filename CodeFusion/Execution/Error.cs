namespace CodeFusion.Execution;

public enum Error
{
    OK,
    ILLEGAL_OPCODE,
    ILLEGAL_ACCESS,
    STACK_UNDERFLOW,
    STACK_OVERFLOW,
    CALL_STACK_OVERFLOW,
    CALL_STACK_UNDERFLOW,
    DIVISON_BY_ZERO
}