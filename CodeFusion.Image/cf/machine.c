
#include <malloc.h>
#include "machine.h"
#include "opcode.h"

static Word read_ptr(void *ptr, Word size) {
    uint64_t result = 0;
    char *buffer = (char *) ptr;

    for (size_t i = 0; i < size.as_u64; i++) {
        result |= (uint64_t) (*buffer << i * 8);
        buffer++;
    }

    return WORD_U64(result);
}

static void write_ptr(void *ptr, Word value, Word size) {
    char *buffer = (char *) ptr;
    uint64_t write = value.as_u64;

    for (size_t i = 0; i < size.as_u64; i++) {
        buffer[i] = (char) (write & 0xFF);
        write >>= 8;
    }
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
    if (cf->program_counter >= cf->program_size) {
        return STATUS_ILLEGAL_ACCESS;
    }

    Inst inst = cf->program[cf->program_counter++];
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
            cf->pool_stack[cf->pool_stack_size++].as_ptr = malloc(get_hash_map(cf->address_pool, inst.operand.as_u64));
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
    }

    return STATUS_ILLEGAL_OPCODE;
}
