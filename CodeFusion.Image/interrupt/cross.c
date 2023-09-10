#include "../bridge/interrupt.h"
#include "../bridge/dll.h"
#include <stdlib.h>

static int strcmp(const char *a, const char *b) {
    while (*a && *b && *a == *b) {
        a++;
        b++;
    }
    return *a - *b;
}

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

static Status cf_malloc(CF_Machine *cf) {
    if (cf->stack_size < 1) {
        return STATUS_STACK_UNDERFLOW;
    }

    cf->stack[cf->stack_size - 1] = WORD_PTR(malloc(cf->stack[cf->stack_size - 1].as_u64));
    return STATUS_OK;
}

static Status cf_free(CF_Machine *cf) {
    if (cf->stack_size < 1) {
        return STATUS_STACK_UNDERFLOW;
    }

    free(cf->stack[cf->stack_size - 1].as_ptr);
    cf->stack_size--;
    return STATUS_OK;
}

static Status cf_load_library(CF_Machine *cf) {
    printf("Load library\n");
    if (cf->stack_size < 1) {
        return STATUS_STACK_UNDERFLOW;
    }
    if (cf->library_size >= LIBRARY_CAPACITY) {
        return STATUS_LIBRARY_OVERFLOW;
    }

    CF_Library lib = cf_load_dll(cf->stack[cf->stack_size - 1].as_ptr);

    for (int i = 0; i < lib.symbol_size; i++) {
        printf("Symbol %s: %"PRIu64"\n", lib.symbols[i].name, lib.symbols[i].address);
    }

    cf->stack[cf->stack_size - 1].as_u64 = cf->library_size;
    cf->libraries[cf->library_size++] = lib;
    return STATUS_OK;
}

static Status cf_unload_library(CF_Machine *cf) {
    if (cf->stack_size < 1) {
        return STATUS_STACK_UNDERFLOW;
    }

    if (cf->stack[cf->stack_size - 1].as_u64 >= cf->library_size || cf->stack[cf->stack_size - 1].as_u64 == 0) {
        return STATUS_ILLEGAL_LIBRARY_INDEX;
    }

    cf_free_dll(&cf->libraries[cf->stack[cf->stack_size - 1].as_u64]);
    cf->stack_size--;
    return STATUS_OK;
}

static Status cf_retrieve_symbol(CF_Machine *cf) {
    if (cf->stack_size < 2) {
        return STATUS_STACK_UNDERFLOW;
    }

    if (cf->stack[cf->stack_size - 2].as_u64 >= cf->library_size || cf->stack[cf->stack_size - 2].as_u64 == 0) {
        return STATUS_ILLEGAL_LIBRARY_INDEX;
    }

    CF_Library *lib = &cf->libraries[cf->stack[cf->stack_size - 2].as_u64];
    for (size_t i = 0; i < lib->symbol_size; i++) {
        if (strcmp(lib->symbols[i].name, cf->stack[cf->stack_size - 1].as_ptr) == 0) {
            cf->stack[cf->stack_size - 2] = WORD_U64(lib->symbols[i].address);
            cf->stack_size--;
            return STATUS_OK;
        }
    }
    return STATUS_SYMBOL_NOT_FOUND;
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
    interrupts[7] = cf_malloc;
    interrupts[8] = cf_free;
    interrupts[9] = cf_load_library;
    interrupts[10] = cf_unload_library;
    interrupts[11] = cf_retrieve_symbol;
}