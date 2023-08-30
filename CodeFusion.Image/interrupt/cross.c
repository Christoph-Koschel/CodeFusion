#include "../bridge/interrupt.h"
#include <stdlib.h>

static Status cf_get_stdout(CF_Machine *cf) {
    if (cf->stack_size >= STACK_CAPACITY) {
        return STATUS_CALL_STACK_OVERFLOW;
    }

    cf->stack[cf->stack_size++] = WORD_PTR(stdout);
    return STATUS_OK;
}

static Status cf_get_stdin(CF_Machine *cf) {
    if (cf->stack_size >= STACK_CAPACITY) {
        return STATUS_CALL_STACK_OVERFLOW;
    }

    cf->stack[cf->stack_size++] = WORD_PTR(stdin);
    return STATUS_OK;
}

static Status cf_get_stderr(CF_Machine *cf) {
    if (cf->stack_size >= STACK_CAPACITY) {
        return STATUS_CALL_STACK_OVERFLOW;
    }

    cf->stack[cf->stack_size++] = WORD_PTR(stderr);
    return STATUS_OK;
}

static Status cf_open(CF_Machine *cf) {
    if (cf->stack_size < 2) {
        return STATUS_STACK_UNDERFLOW;
    }

    cf->stack[cf->stack_size - 2] = WORD_PTR(
            fopen(cf->stack[cf->stack_size - 2].as_ptr, cf->stack[cf->stack_size - 1].as_ptr));
    cf->stack_size--;
    return STATUS_OK;
}

static Status cf_write(CF_Machine *cf) {
    if (cf->stack_size < 4) {
        return STATUS_STACK_UNDERFLOW;
    }

    fwrite(cf->stack[cf->stack_size - 1].as_ptr, cf->stack[cf->stack_size - 2].as_u64,
           cf->stack[cf->stack_size - 3].as_u64, cf->stack[cf->stack_size - 4].as_ptr);
    cf->stack_size -= 4;
    return STATUS_OK;
}

static Status cf_close(CF_Machine *cf) {
    if (cf->stack_size < 1) {
        return STATUS_STACK_UNDERFLOW;
    }

    fclose(cf->stack[cf->stack_size - 1].as_ptr);
    cf->stack_size--;
    return STATUS_OK;
}

static Status cf_exit(CF_Machine *cf) {
    if (cf->stack_size < 1) {
        return STATUS_STACK_UNDERFLOW;
    }

    exit(cf->stack[cf->stack_size - 1].as_i64);
}

static void init() __attribute__((constructor));

static void init() {
    interrupts[0] = cf_get_stdout;
    interrupts[1] = cf_get_stdin;
    interrupts[2] = cf_get_stderr;
    interrupts[3] = cf_open;
    interrupts[4] = cf_write;
    interrupts[5] = cf_close;
    interrupts[6] = cf_exit;
}