#ifndef CF_MACHINE_H
#define CF_MACHINE_H

#include <inttypes.h>
#include <stdio.h>
#include <assert.h>
#include "hashmap.h"

#define STACK_CAPACITY 1024
#define PROGRAM_CAPACITY 1024
#define CALLSTACK_CAPACITY 1024
#define INTERRUPT_CAPACITY 255
#define LIBRARY_CAPACITY 255

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
static_assert(LIBRARY_CAPACITY <= (65535) && LIBRARY_CAPACITY > 1,
              "LIBRARY_CAPACITY must fit in a unsigned short (65535) and must be greater than 1");

typedef struct {
    char *name;
    uint64_t address;
} CF_Symbol;

typedef struct {
    Inst program[PROGRAM_CAPACITY];
    uint64_t program_size;

    HashMap *address_pool;

    CF_Symbol *symbols;
    uint32_t symbol_size;

    void *memory;
    uint64_t memory_size;

    const char *path;
    void *handler;
} CF_Library;

typedef struct {
    Word stack[STACK_CAPACITY];
    uint64_t stack_size;

    Word pool_stack[CALLSTACK_CAPACITY];
    uint64_t pool_stack_size;

    uint64_t program_counter;
    uint16_t program_pool;

    CF_Library libraries[LIBRARY_CAPACITY];
    uint64_t library_size;
} CF_Machine;

typedef enum {
    STATUS_OK,
    STATUS_ILLEGAL_OPCODE,
    STATUS_ILLEGAL_ACCESS,
    STATUS_STACK_UNDERFLOW,
    STATUS_STACK_OVERFLOW,
    STATUS_CALL_STACK_OVERFLOW,
    STATUS_CALL_STACK_UNDERFLOW,
    STATUS_DIVISION_BY_ZERO,
    STATUS_ILLEGAL_INTERRUPT,
    STATUS_ILLEGAL_LIBRARY_INDEX,
    STATUS_LIBRARY_OVERFLOW,
    STATUS_SYMBOL_NOT_FOUND,
} Status;

typedef Status (*CF_Interrupt)(CF_Machine *);

Status cf_execute_inst(CF_Machine *cf);

#endif
