using System;
using System.Runtime.InteropServices;
using CodeFusion.VM;

namespace CodeFusion.Execution;

public class Runner
{
    public unsafe Error ExecuteInst(ref VmCodeFusion cf)
    {
        if (cf.programCounter >= cf.programSize)
        {
            return Error.ILLEGAL_ACCESS;
        }

        Inst inst = cf.program[cf.programCounter++];
        switch (inst.opcode)
        {
            case Opcode.NOP:
                return Error.OK;
            case Opcode.PUSH:
                if (cf.stackSize >= VmCodeFusion.STACK_CAPACITY)
                {
                    return Error.STACK_OVERFLOW;
                }
                cf.stack[cf.stackSize++] = inst.operand;
                return Error.OK;
            case Opcode.POP:
                if (cf.stackSize < 1)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stackSize--;
                return Error.OK;
            case Opcode.LOAD:
                if (cf.stackSize < 1)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 1] = ReadPoolN(ref cf, inst.operand, cf.stack[cf.stackSize - 1]);
                return Error.OK;
            case Opcode.STORE:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                WritePoolN(ref cf, inst.operand, cf.stack[cf.stackSize - 2], cf.stack[cf.stackSize - 1]);
                cf.stackSize -= 2;
                return Error.OK;
            case Opcode.MALLOC_POOL:
                if (cf.poolStackSize >= VmCodeFusion.CALLSTACK_CAPACITY)
                {
                    return Error.CALL_STACK_OVERFLOW;
                }
                ushort size = (ushort)(cf.addressPool[inst.operand.asU64] ?? 0);
                Word word = new Word(Marshal.AllocHGlobal(size).ToInt64());
                cf.poolStack[cf.poolStackSize++] = word;
                return Error.OK;
            case Opcode.FREE_POOL:
                if (cf.poolStackSize < 1)
                {
                    return Error.CALL_STACK_UNDERFLOW;
                }
                Marshal.FreeHGlobal(new IntPtr(cf.poolStack[--cf.poolStackSize].asPtr));
                return Error.OK;
            case Opcode.PUSH_PTR:
                if (cf.poolStackSize < 1)
                {
                    return Error.CALL_STACK_UNDERFLOW;
                }
                if (cf.stackSize >= VmCodeFusion.STACK_CAPACITY)
                {
                    return Error.STACK_OVERFLOW;
                }
                cf.stack[cf.stackSize++] = cf.poolStack[cf.poolStackSize - 1] + inst.operand;
                return Error.OK;
            case Opcode.LOAD_PTR:
                if (cf.stackSize < 1)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 1] = ReadPtr(cf.stack[cf.stackSize - 1].asPtr, inst.operand);
                return Error.OK;
            case Opcode.STORE_PTR:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                WritePtr(cf.stack[cf.stackSize - 2].asPtr, cf.stack[cf.stackSize - 1], inst.operand);
                cf.stackSize -= 2;
                return Error.OK;
            case Opcode.DUP:
                if (cf.stackSize < inst.operand.asU64)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stackSize > VmCodeFusion.STACK_CAPACITY)
                {
                    return Error.STACK_OVERFLOW;
                }
                Word dup = cf.stack[cf.stackSize - (1 + inst.operand.asU64)];
                cf.stack[cf.stackSize] = dup;
                cf.stackSize++;

                return Error.OK;
            case Opcode.PUSH_ARRAY:
                if (cf.stackSize < 1)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 1] = new Word(Marshal.AllocHGlobal((int)cf.stack[cf.stackSize - 1].asI64).ToPointer());

                return Error.OK;
            case Opcode.LOAD_ARRAY:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = ReadPtr(cf.stack[cf.stackSize - 2].asPtr, cf.stack[cf.stackSize - 1], inst.operand);
                cf.stackSize--;
                return Error.OK;
            case Opcode.STORE_ARRAY:
                if (cf.stackSize < 3)
                {
                    return Error.STACK_UNDERFLOW;
                }
                WritePtr(cf.stack[cf.stackSize - 3].asPtr, cf.stack[cf.stackSize - 2], cf.stack[cf.stackSize - 1], inst.operand);
                cf.stackSize -= 3;
                return Error.OK;
            case Opcode.IADD:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asI64 + cf.stack[cf.stackSize - 1].asI64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.FADD:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asF64 + cf.stack[cf.stackSize - 1].asF64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.UADD:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asU64 + cf.stack[cf.stackSize - 1].asU64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.ISUB:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asI64 - cf.stack[cf.stackSize - 1].asI64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.FSUB:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asF64 - cf.stack[cf.stackSize - 1].asF64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.USUB:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asU64 - cf.stack[cf.stackSize - 1].asU64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.IMUL:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asI64 * cf.stack[cf.stackSize - 1].asI64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.FMUL:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asF64 * cf.stack[cf.stackSize - 1].asF64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.UMUL:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asU64 + cf.stack[cf.stackSize - 1].asU64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.IDIV:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stack[cf.stackSize - 1].asI64 == 0)
                {
                    return Error.DIVISON_BY_ZERO;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asI64 / cf.stack[cf.stackSize - 1].asI64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.FDIV:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stack[cf.stackSize - 1].asF64 == 0)
                {
                    return Error.DIVISON_BY_ZERO;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asF64 / cf.stack[cf.stackSize - 1].asF64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.UDIV:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stack[cf.stackSize - 1].asU64 == 0)
                {
                    return Error.DIVISON_BY_ZERO;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asU64 / cf.stack[cf.stackSize - 1].asU64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.IMOD:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stack[cf.stackSize - 1].asI64 == 0)
                {
                    return Error.DIVISON_BY_ZERO;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asI64 % cf.stack[cf.stackSize - 1].asI64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.FMOD:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stack[cf.stackSize - 1].asF64 == 0)
                {
                    return Error.DIVISON_BY_ZERO;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asF64 % cf.stack[cf.stackSize - 1].asF64);
                cf.stackSize--;
                return Error.OK;
            case Opcode.UMOD:
                if (cf.stackSize < 2)
                {
                    return Error.STACK_UNDERFLOW;
                }
                if (cf.stack[cf.stackSize - 1].asU64 == 0)
                {
                    return Error.DIVISON_BY_ZERO;
                }
                cf.stack[cf.stackSize - 2] = new Word(cf.stack[cf.stackSize - 2].asU64 % cf.stack[cf.stackSize - 1].asU64);
                cf.stackSize--;
                return Error.OK;
        }

        return Error.ILLEGAL_OPCODE;
    }

    public unsafe Word ReadPoolN(ref VmCodeFusion cf, Word offset, Word size)
    {
        return ReadPtr(cf.poolStack[cf.poolStackSize - 1].asPtr, offset, size);
    }

    public unsafe void WritePoolN(ref VmCodeFusion cf, Word offset, Word value, Word size)
    {
        WritePtr(cf.poolStack[cf.poolStackSize - 1].asPtr, offset, value, size);
    }

    public unsafe Word ReadPtr(void* ptr, Word offset, Word size)
    {
        return ReadPtr((void*)((ulong)ptr + offset.asU64), size);
    }

    public unsafe Word ReadPtr(void* ptr, Word size)
    {
        ulong result = 0;
        byte* buffer = (byte*)ptr;

        for (int i = 0; i < (int)size.asU64; i++)
        {
            result |= (ulong)*buffer << i * 8;
            buffer++;
        }

        return new Word(result);
    }

    public unsafe void WritePtr(void* ptr, Word offset, Word value, Word size)
    {
        WritePtr((void*)((ulong)ptr + offset.asU64), value, size);
    }

    public unsafe void WritePtr(void* ptr, Word value, Word size)
    {
        byte* buffer = (byte*)ptr;
        ulong write = value.asU64;

        for (int i = 0; i < (int)size.asU64; i++)
        {
            buffer[i] = (byte)(write & 0xFF);
            write >>= 8;
        }
    }
}