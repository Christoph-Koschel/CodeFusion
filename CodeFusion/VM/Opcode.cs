namespace CodeFusion.VM;

// TODO improve Opcode docuemtation

public static class Opcode
{
    /// <summary>
    /// nop<br /><br />
    /// No Operation
    /// </summary>
    public const byte NOP = 0;

    /// <summary>
    /// push &lt;x><br /><br />
    /// Pushes the constant x on top of the stack
    /// </summary>
    public const byte PUSH = 1;

    /// <summary>
    /// pop<br /><br />
    /// Pops the top value of the stack
    /// </summary>
    public const byte POP = 2;

    /// <summary>
    /// load &lt;offset><br /><br />
    /// Loads the value with the offset as operand and the size on top of the stack from the current pool
    ///
    /// <code>
    ///     push 4 ; Pushes the size
    ///     load 0 ; Load the value stored in 4 Bytes at the pool offset 0
    /// </code>
    /// </summary>
    public const byte LOAD = 3;

    /// <summary>
    /// store &lt;offset><br /><br />
    /// Stores a value and its size on top of the stack with the offset as operand in the current pool
    ///
    /// <code>
    ///     push 40 ; Pushes the value
    ///     push 4 ; Pushes the size (4 Bytes aka. Int)
    ///     store 0 ; Store the value in 4 Bytes at the pool offset 0
    /// </code>
    /// </summary>
    public const byte STORE = 4;

    /// <summary>
    /// mallocpool &lt;address><br /><br />
    /// Allocates memory for the function address, where the size is defined in the pool HashTable
    ///
    /// <code>
    ///     [12] entry:
    ///         mallocpool entry ; Malloc a pool of 12 Bytes
    ///         freepool ; Frees the latest pool
    /// </code>
    /// </summary>
    public const byte MALLOC_POOL = 5;

    /// <summary>
    /// freepool<br /><br />
    /// Frees the last pool of the poolstack
    ///
    /// <code>
    ///     [12] entry:
    ///         mallocpool entry ; Malloc a pool of 12 Bytes
    ///         freepool ; Frees the latest pool
    /// </code>
    /// </summary>
    public const byte FREE_POOL = 6;

    /// <summary>
    /// pushptr &lt;offset><br /><br />
    /// Pushes a new ptr on top of the stack that's directing to the pool + offset byte
    /// 
    /// <code>
    /// ; Create a new PTR
    /// pushptr 0
    /// </code>
    /// </summary>
    public const byte PUSH_PTR = 7;

    /// <summary>
    /// loadptr &lt;size><br /><br />
    /// Loads the value from the ptr on top of the stack behind a ptr with its index with its given size
    /// 
    /// <code>
    ///     ; Create a new PTR
    ///     pushptr 0
    ///     ; Pushes index
    ///     push 0
    ///     ; Load a value
    ///     loadptr 4
    /// </code>
    /// </summary>
    public const byte LOAD_PTR = 8;

    /// <summary>
    /// storeptr &lt;size><br /><br />
    /// Stores a value behind a ptr with its given size
    ///
    /// <code>
    ///     ; Creates a new PTR
    ///     pushptr 0
    ///     ; Pushes the value
    ///     push 40
    ///     ; Store the value
    ///     storeptr 4
    /// </code>
    /// </summary>
    public const byte STORE_PTR = 9;

    /// <summary>
    /// dup &lt;offset><br /><br />
    /// Duplicates a value of the stack. Zero duplicates the top value
    ///  
    /// <code>  
    ///     push 40
    ///     ; Dupplicate the top value
    ///     dup 0
    /// </code>
    /// </summary>
    public const byte DUP = 10;

    /// <summary>
    /// pusharray<br /><br />
    /// Creates a new array with the size in bytes allocated on the heap,
    /// pushes the array ptr on top of the stack
    ///
    /// <code>
    ///     push 8 ; size
    ///     pusharray
    /// </code>
    /// </summary>
    public const byte PUSH_ARRAY = 11;

    /// <summary>
    /// loadarray &lt;size><br /><br />
    /// Loads a value from a array ptr with its index on the stack within its size
    ///
    /// <code>
    ///     ; Create a Array
    ///     push 8
    ///     pusharray
    ///     ; Load a value
    ///     push 0 ; index
    ///     loadarray 4 ; size
    /// </code>
    /// </summary>
    public const byte LOAD_ARRAY = 12;

    /// <summary>
    /// storearray &lt;size><br /><br />
    /// Stores a value from a array ptr with its index and value on the stack within its size
    ///
    /// <code>
    ///     ; Create a Array
    ///     push 8
    ///     pusharray
    ///     ; Store a value
    ///     push 0 ; index
    ///     push 123 ; value
    ///     storearray 4 ; size
    /// </code>
    /// </summary>
    public const byte STORE_ARRAY = 13;

    public const byte IADD = 14;
    public const byte FADD = 15;
    public const byte UADD = 16;

    public const byte ISUB = 17;
    public const byte FSUB = 18;
    public const byte USUB = 19;

    public const byte IMUL = 20;
    public const byte FMUL = 21;
    public const byte UMUL = 22;

    public const byte IDIV = 23;
    public const byte FDIV = 24;
    public const byte UDIV = 25;

    public const byte IMOD = 26;
    public const byte FMOD = 27;
    public const byte UMOD = 28;

    public const byte ILESS = 29;
    public const byte FLESS = 30;
    public const byte ULESS =  31;

    public const byte ILESS_EQUAL = 32;
    public const byte FLESS_EQUAL = 33;
    public const byte ULESS_EQUAL = 34;

    public const byte IGREATER = 35;
    public const byte FGREATER = 36;
    public const byte UGREATER = 37;

    public const byte IGREATER_EQUALS = 38;
    public const byte FGREATER_EQUALS = 39;
    public const byte UGREATER_EQUALS = 40;

    public const byte EQ = 41;
    public const byte NEQ = 42;

    public const byte AND = 43;
    public const byte OR = 44;
    public const byte XOR = 45;
    public const byte LSHIFT = 46;
    public const byte RSHIFT = 47;

    /// <summary>
    /// int &lt;code><br /><br />
    /// Interrupt the machine with a specific function code
    ///
    /// <code>
    ///     push 0
    ///     int 6 ; Interrupt the machine with the exit code 0, what will also exit the program
    /// </code>
    /// </summary>
    public const byte INT = 48;

    public const byte JMP = 49;
    public const byte JMP_ZERO = 50;
    public const byte JMP_NOT_ZERO = 51;

    public const byte CALL = 52;
    public const byte VCALL = 53;
    public const byte RET = 54;

    public const byte LOAD_MEMORY = 55;

    public static bool HasOperand(byte opcode)
    {
        switch (opcode)
        {
            case PUSH:
            case LOAD:
            case STORE:
            case MALLOC_POOL:
            case PUSH_PTR:
            case LOAD_PTR:
            case STORE_PTR:
            case DUP:
            case LOAD_ARRAY:
            case STORE_ARRAY:
            case INT:
            case JMP:
            case JMP_ZERO:
            case JMP_NOT_ZERO:
            case CALL:
            case LOAD_MEMORY:
                return true;
            default:
                return false;
        }
    }
}