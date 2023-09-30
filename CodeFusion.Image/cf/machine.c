
#include <malloc.h>
#include <math.h>
#include <memory.h>
#include "machine.h"
#include "opcode.h"
#include "debug.h"


#define BINARY_OP(cf, in, out, op)                                                               \
{                                                                                                \
    if ((cf)->stack_size < 2) {                                                                  \
        return STATUS_STACK_UNDERFLOW;                                                           \
    }                                                                                            \
    (cf)->stack[(cf)->stack_size - 2].as_##out =                                                 \
        (cf)->stack[(cf)->stack_size - 2].as_##in op (cf)->stack[(cf)->stack_size - 1].as_##in;  \
    (cf)->stack_size--;                                                                          \
    return STATUS_OK;                                                                            \
}                                                                                                \

#define UNARY_OP(cf, in, out, op)                                                                \
{                                                                                                \
    if ((cf)->stack_size < 1) {                                                                  \
        return STATUS_STACK_OVERFLOW;                                                            \
    }                                                                                            \
    (cf)->stack[(cf)->stack_size - 1].as_##out = op (cf)->stack[(cf)->stack_size - 1].as_##in;   \
    return STATUS_OK;                                                                            \
}

#define CAST_OP(cf, from, to, cast)                                                              \
{                                                                                                \
    if ((cf)->stack_size < 1) {                                                                  \
        return STATUS_STACK_UNDERFLOW;                                                           \
    }                                                                                            \
    (cf)->stack[(cf)->stack_size - 1].as_##to = cast (cf)->stack[(cf)->stack_size - 1].as_##from;\
    return STATUS_OK;                                                                            \
}

#define CF_PROGRAM_SIZE(cf) (cf->libraries[cf->program_pool].program_size)
#define CF_MEMORY_SIZE(cf) (cf->libraries[cf->program_pool].memory_size)
#define CF_PROGRAM(cf) (cf->libraries[cf->program_pool].program)
#define CF_ADDR_POOL(cf) (cf->libraries[cf->program_pool].address_pool)

CF_Interrupt interrupts[INTERRUPT_CAPACITY] = {0};

static Word read_ptr(void *ptr, Word size) {
    Word result = WORD_U64(0);
    memcpy(&result.as_u64, ptr, size.as_u64);

    return result;
}

static void write_ptr(void *ptr, Word value, Word size) {
    memcpy(ptr, &value.as_u64, size.as_u64);
}

static Word read_ptr_at(void *ptr, Word offset, Word size) {
    return read_ptr((void *) (ptr + offset.as_u64), size);
}

static void write_ptr_at(void *ptr, Word offset, Word value, Word size) {
    write_ptr((void *) (ptr + offset.as_u64), value, size);
}

static Word read_pool_n(CF_Machine *cf, Word offset, Word size) {
    return read_ptr_at(cf->pool_stack[cf->pool_stack_size - 1].as_ptr, offset, size);
}

static void write_pool_n(CF_Machine *cf, Word offset, Word value, Word size) {
    write_ptr_at(cf->pool_stack[cf->pool_stack_size - 1].as_ptr, offset, value, size);
}

Status cf_execute_inst(CF_Machine *cf) {
    if (cf->program_counter >= CF_PROGRAM_SIZE(cf)) {
        return STATUS_ILLEGAL_ACCESS;
    }

    Inst inst = CF_PROGRAM(cf)[cf->program_counter++];

    switch (inst.opcode) {
        case INST_NOP:
            return STATUS_OK;
        case INST_PUSH:
            if (cf->stack_size >= STACK_CAPACITY) {
                return STATUS_STACK_OVERFLOW;
            }
            cf->stack[cf->stack_size++] = inst.operand;
            return STATUS_OK;
        case INST_POP:
            if (cf->stack_size < 1) {
                return STATUS_STACK_UNDERFLOW;
            }
            cf->stack_size--;
            return STATUS_OK;
        case INST_LOAD:
            if (cf->stack_size < 1) {
                return STATUS_STACK_UNDERFLOW;
            }
            cf->stack[cf->stack_size - 1] = read_pool_n(cf, inst.operand, cf->stack[cf->stack_size - 1]);
            return STATUS_OK;
        case INST_STORE:
            if (cf->stack_size < 2) {
                return STATUS_STACK_UNDERFLOW;
            }
            write_pool_n(cf, inst.operand, cf->stack[cf->stack_size - 2], cf->stack[cf->stack_size - 1]);
            cf->stack_size -= 2;
            return STATUS_OK;
        case INST_MALLOC_POOL:
            if (cf->pool_stack_size >= CALLSTACK_CAPACITY) {
                return STATUS_CALL_STACK_OVERFLOW;
            }
            cf->pool_stack[cf->pool_stack_size++].as_ptr = malloc(get_hash_map(CF_ADDR_POOL(cf), inst.operand.as_u64));
            return STATUS_OK;
        case INST_FREE_POOL:
            if (cf->pool_stack_size < 1) {
                return STATUS_CALL_STACK_UNDERFLOW;
            }
            free(cf->pool_stack[--cf->pool_stack_size].as_ptr);
            return STATUS_OK;
        case INST_PUSH_PTR:
            if (cf->pool_stack_size < 1) {
                return STATUS_CALL_STACK_UNDERFLOW;
            }
            if (cf->stack_size >= STACK_CAPACITY) {
                return STATUS_STACK_UNDERFLOW;
            }
            cf->stack[cf->stack_size++].as_ptr = cf->pool_stack[cf->pool_stack_size - 1].as_ptr + inst.operand.as_u64;
            return STATUS_OK;
        case INST_LOAD_PTR:
            if (cf->stack_size < 1) {
                return STATUS_STACK_UNDERFLOW;
            }
            cf->stack[cf->stack_size - 1] = read_ptr(cf->stack[cf->stack_size - 1].as_ptr, inst.operand);
            return STATUS_OK;
        case INST_STORE_PTR:
            if (cf->stack_size < 2) {
                return STATUS_STACK_UNDERFLOW;
            }
            write_ptr(cf->stack[cf->stack_size - 2].as_ptr, cf->stack[cf->stack_size - 1], inst.operand);
            cf->stack_size -= 2;
            return STATUS_OK;
        case INST_DUP:
            if (cf->stack_size < inst.operand.as_u64) {
                return STATUS_STACK_UNDERFLOW;
            }
            if (cf->stack_size > STACK_CAPACITY) {
                return STATUS_STACK_OVERFLOW;
            }
            cf->stack[cf->stack_size++] = cf->stack[cf->stack_size - (1 + inst.operand.as_u64)];
            return STATUS_OK;
        case INST_PUSH_ARRAY:
            if (cf->stack_size < 1) {
                return STATUS_STACK_UNDERFLOW;
            }
            cf->stack[cf->stack_size - 1].as_ptr = malloc(cf->stack[cf->stack_size - 1].as_u64);
            return STATUS_OK;
        case INST_LOAD_ARRAY:
            if (cf->stack_size < 2) {
                return STATUS_STACK_UNDERFLOW;
            }
            cf->stack[cf->stack_size - 2] = read_ptr_at(cf->stack[cf->stack_size - 2].as_ptr,
                                                        cf->stack[cf->stack_size - 1], inst.operand);
            cf->stack_size--;
            return STATUS_OK;
        case INST_STORE_ARRAY:
            if (cf->stack_size < 3) {
                return STATUS_STACK_UNDERFLOW;
            }
            write_ptr_at(cf->stack[cf->stack_size - 3].as_ptr, cf->stack[cf->stack_size - 2],
                         cf->stack[cf->stack_size - 1], inst.operand);
            cf->stack_size -= 3;
            return STATUS_OK;
        case INST_IADD: BINARY_OP(cf, i64, i64, +)
        case INST_FADD: BINARY_OP(cf, f64, f64, +)
        case INST_UADD: BINARY_OP(cf, u64, u64, +)
        case INST_ISUB: BINARY_OP(cf, i64, i64, -)
        case INST_FSUB: BINARY_OP(cf, f64, f64, -)
        case INST_USUB: BINARY_OP(cf, u64, u64, -)
        case INST_IMUL: BINARY_OP(cf, i64, i64, *)
        case INST_FMUL: BINARY_OP(cf, f64, f64, *)
        case INST_UMUL: BINARY_OP(cf, u64, u64, *)
        case INST_IDIV:
            if (cf->stack[cf->stack_size - 1].as_i64 == 0) {
                return STATUS_DIVISION_BY_ZERO;
            }
            BINARY_OP(cf, i64, i64, /)
        case INST_FDIV:
            if (cf->stack[cf->stack_size - 1].as_f64 == 0) {
                return STATUS_DIVISION_BY_ZERO;
            }
            BINARY_OP(cf, f64, f64, /)
        case INST_UDIV:
            if (cf->stack[cf->stack_size - 1].as_u64 == 0) {
                return STATUS_DIVISION_BY_ZERO;
            }
            BINARY_OP(cf, u64, u64, /)
        case INST_IMOD:
            if (cf->stack[cf->stack_size - 1].as_i64 == 0) {
                return STATUS_DIVISION_BY_ZERO;
            }
            BINARY_OP(cf, i64, i64, %)
        case INST_FMOD:
            if (cf->stack[cf->stack_size - 1].as_f64 == 0) {
                return STATUS_DIVISION_BY_ZERO;
            }
            {
                if ((cf)->stack_size < 2) { return STATUS_STACK_UNDERFLOW; }
                (cf)->stack[(cf)->stack_size - 2].as_f64 =
                        remainder((cf)->stack[(cf)->stack_size - 2].as_f64, (cf)->stack[(cf)->stack_size - 1].as_f64);
                (cf)->stack_size--;
                break;
            }
        case INST_UMOD:
            if (cf->stack[cf->stack_size - 1].as_u64 == 0) {
                return STATUS_DIVISION_BY_ZERO;
            }
            BINARY_OP(cf, u64, u64, %)
        case INST_ILESS: BINARY_OP(cf, i64, u64, <)
        case INST_FLESS: BINARY_OP(cf, f64, u64, <)
        case INST_ULESS: BINARY_OP(cf, u64, u64, <)
        case INST_ILESS_EQUAL: BINARY_OP(cf, i64, u64, <=)
        case INST_FLESS_EQUAL: BINARY_OP(cf, f64, u64, <=)
        case INST_ULESS_EQUAL: BINARY_OP(cf, u64, u64, <=)
        case INST_IGREATER: BINARY_OP(cf, i64, u64, >)
        case INST_FGREATER: BINARY_OP(cf, f64, u64, >)
        case INST_UGREATER: BINARY_OP(cf, u64, u64, >)
        case INST_IGREATER_EQUALS: BINARY_OP(cf, i64, u64, >=)
        case INST_FGREATER_EQUALS: BINARY_OP(cf, f64, u64, >=)
        case INST_UGREATER_EQUALS: BINARY_OP(cf, u64, u64, >=)
        case INST_EQ: BINARY_OP(cf, u64, u64, ==)
        case INST_NEQ: BINARY_OP(cf, u64, u64, !=)
        case INST_AND: BINARY_OP(cf, u64, u64, &)
        case INST_OR: BINARY_OP(cf, u64, u64, |)
        case INST_XOR: BINARY_OP(cf, u64, u64, ^)
        case INST_LSHIFT: BINARY_OP(cf, u64, u64, <<)
        case INST_RSHIFT: BINARY_OP(cf, u64, u64, >>)
        case INST_INEG: UNARY_OP(cf, i64, i64, -)
        case INST_FNEG: UNARY_OP(cf, f64, f64, -)
        case INST_UNEG: UNARY_OP(cf, u64, u64, -)
        case INST_NOT: UNARY_OP(cf, u64, u64, !)
        case INST_ONES: UNARY_OP(cf, u64, u64, ~)
        case INST_INT:
            if (interrupts[inst.operand.as_u64] == NULL) {
                return STATUS_ILLEGAL_INTERRUPT;
            }
            PRINT_DEBUG("Interrupt %"PRIu64"\n", inst.operand.as_u64);
            return interrupts[inst.operand.as_u64](cf);
        case INST_JMP:
            if (inst.operand.as_u64 >= CF_PROGRAM_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            cf->program_counter = inst.operand.as_u64;
            return STATUS_OK;
        case INST_JMP_ZERO:
            if (cf->stack_size < 1) {
                return STATUS_STACK_UNDERFLOW;
            }
            if (inst.operand.as_u64 >= CF_PROGRAM_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            if (cf->stack[--cf->stack_size].as_u64 == 0) {
                cf->program_counter = inst.operand.as_u64;
            }
            return STATUS_OK;
        case INST_JMP_NOT_ZERO:
            if (cf->stack_size < 1) {
                return STATUS_STACK_UNDERFLOW;
            }
            if (inst.operand.as_u64 >= CF_PROGRAM_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            if (cf->stack[--cf->stack_size].as_u64 != 0) {
                cf->program_counter = inst.operand.as_u64;
            }
            return STATUS_OK;
        case INST_CALL:
            if (cf->stack_size >= STACK_CAPACITY || cf->stack_size + 1 >= STACK_CAPACITY) {
                return STATUS_STACK_OVERFLOW;
            }
            if (inst.operand.as_u64 >= CF_PROGRAM_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            cf->stack[cf->stack_size++] = WORD_U64(cf->program_pool);
            cf->stack[cf->stack_size++] = WORD_U64(cf->program_counter);
            cf->program_counter = inst.operand.as_u64;
            return STATUS_OK;
        case INST_VCALL:
            if (cf->stack_size < 2) {
                return STATUS_STACK_UNDERFLOW;
            }
            if (cf->stack[cf->stack_size - 2].as_u64 >= cf->library_size) {
                return STATUS_ILLEGAL_LIBRARY_INDEX;
            }

            uint64_t oldLib = cf->program_pool;
            cf->program_pool = cf->stack[cf->stack_size - 2].as_u64;
            if (cf->stack[cf->stack_size - 1].as_u64 >= CF_PROGRAM_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            cf->stack[cf->stack_size - 2] = WORD_U64(oldLib);

            uint64_t newIC = cf->stack[cf->stack_size - 1].as_u64;
            cf->stack[cf->stack_size - 1] = WORD_U64(cf->program_counter);
            cf->program_counter = newIC;
            return STATUS_OK;
        case INST_RET:
            if (cf->stack_size < 2) {
                return STATUS_STACK_UNDERFLOW;
            }

            if (cf->stack[cf->stack_size - 2].as_u64 >= cf->library_size) {
                return STATUS_ILLEGAL_LIBRARY_INDEX;
            }
            if (cf->stack[cf->stack_size - 2].as_u64 != cf->program_pool) {
                cf->program_pool = cf->stack[cf->stack_size - 2].as_u64;
            }

            if (cf->stack[cf->stack_size - 1].as_u64 >= CF_PROGRAM_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            cf->program_counter = cf->stack[cf->stack_size - 1].as_u64;
            cf->stack_size -= 2;
            return STATUS_OK;
        case INST_ITU: CAST_OP(cf, i64, u64, (uint64_t))
        case INST_ITF: CAST_OP(cf, i64, f64, (double))
        case INST_FTI: CAST_OP(cf, f64, i64, (int64_t))
        case INST_FTU: CAST_OP(cf, f64, u64, (uint64_t))
        case INST_UTI: CAST_OP(cf, u64, i64, (int64_t))
        case INST_UTF: CAST_OP(cf, u64, f64, (double))
        case INST_LOAD_MEMORY:
            if (cf->stack_size >= STACK_CAPACITY) {
                return STATUS_STACK_OVERFLOW;
            }
            PRINT_DEBUG("Load memory address %"PRIu64" of size %"PRIu64"\n", inst.operand.as_u64, CF_MEMORY_SIZE(cf));
            if (inst.operand.as_u64 >= CF_MEMORY_SIZE(cf)) {
                return STATUS_ILLEGAL_ACCESS;
            }
            cf->stack[cf->stack_size++] = WORD_PTR(cf->libraries[cf->program_pool].memory + inst.operand.as_u64);
            return STATUS_OK;
    }

    return STATUS_ILLEGAL_OPCODE;
}
