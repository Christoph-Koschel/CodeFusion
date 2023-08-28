#ifndef CF_MACHINE_H
#define CF_MACHINE_H

#include <inttypes.h>
#include <stdio.h>
#include <assert.h>
#include "hashmap.h"

#define STACK_CAPACITY 1024
#define PROGRAM_CAPACITY 1024
#define CALLSTACK_CAPACITY 1024

#define WORD_U64(value) ((Word){.as_u64 = value})
#define WORD_I64(value) ((Word){.as_i64 = value})
#define WORD_F64(value) ((Word){.as_f64 = value})
#define WORD_PTR(value) ((Word){.as_ptr = value})

typedef union {
    uint64_t as_u64;
    int64_t as_i64;
    void *as_ptr;
    double as_f64;
} Word;

typedef struct {
    uint8_t opcode;
    Word operand;
} Inst;

static_assert(sizeof(Word) == 8, "Size of Word must be 64Bit aka. 8Bytes");

typedef struct {
    Word stack[STACK_CAPACITY];
    uint64_t stack_size;

    Inst program[PROGRAM_CAPACITY];
    uint64_t program_size;

    Word pool_stack[CALLSTACK_CAPACITY];
    uint64_t pool_stack_size;

    HashMap *address_pool;

    uint64_t program_counter;

} CF_Machine;

typedef enum {
    STATUS_OK,
    STATUS_ILLEGAL_OPCODE,
    STATUS_ILLEGAL_ACCESS,
    STATUS_STACK_UNDERFLOW,
    STATUS_STACK_OVERFLOW,
    STATUS_CALL_STACK_OVERFLOW,
    STATUS_CALL_STACK_UNDERFLOW,
    STATUS_DIVISON_BY_ZERO
} Status;

Status cf_execute_inst(CF_Machine *cf);

#endif
