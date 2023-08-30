using System;
using System.Collections;
using CodeFusion.Execution;

namespace CodeFusion.VM;

public struct VmCodeFusion
{
    public const int STACK_CAPACITY = 1024;
    public const int PROGRAM_CAPACITY = 1024;
    public const int CALLSTACK_CAPACITY = 1000;
    public const int INTERRUPT_CAPACITY = 255;

    public Word[] stack = new Word[STACK_CAPACITY];
    public ulong stackSize = 0;
    public readonly Inst[] program = new Inst[PROGRAM_CAPACITY];
    public ulong programSize = 0;
    public readonly Word[] poolStack = new Word[CALLSTACK_CAPACITY];
    public ulong poolStackSize = 0;

    public readonly Hashtable addressPool = new Hashtable();
    public ulong programCounter = 0;

    public readonly Func<VmCodeFusion, Error>[] interrupts = new Func<VmCodeFusion, Error>[INTERRUPT_CAPACITY];
    public VmCodeFusion()
    {
    }
}