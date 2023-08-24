namespace CodeFusion.VM;

public static class Opcode
{
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
    /// </summary>
    public const byte LOAD = 3;

    /// <summary>
    /// store &lt;offset><br /><br />
    /// Stores a value and its size on top of the stack with the offset as operand in the current pool
    /// </summary>
    public const byte STORE = 4;

    /// <summary>
    /// mallocpool &lt;address><br /><br />
    /// Allocates memory for the function address, where the size is defined in the pool HashTable
    /// </summary>
    public const byte MALLOC_POOL = 5;

    /// <summary>
    /// freepool<br /><br />
    /// Frees the last pool of the poolstack
    /// </summary>
    public const byte FREE_POOL = 6;

    /// <summary>
    /// pushptr &lt;offset><br /><br />
    /// Pushes a new ptr on top of the stack that's directing to the pool + offset byte
    /// </summary>
    public const byte PUSH_PTR = 7;

    /// <summary>
    /// loadptr &lt;size><br /><br />
    /// Loads the value from the ptr on top of the stack behind a ptr with its given size
    /// </summary>
    public const byte LOAD_PTR = 8;

    /// <summary>
    /// storeptr &lt;size><br /><br />
    /// Stores a value behind a ptr with its given size
    /// </summary>
    public const byte STORE_PTR = 9;

    /// <summary>
    /// dup &lt;offset><br /><br />
    /// Duplicates a value of the stack. Zero duplicates the top value
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
                return true;
            default:
                return false;
        }
    }
}