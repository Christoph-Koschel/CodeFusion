#include <stdio.h>
#include <stdlib.h>
#include "cf/CodeFusion.h"
#include "cf/debug.h"

CF_Machine cf = {0};

extern char _binary_cf_code_bin_start[];
extern char _binary_cf_code_bin_end[];

static void exit_with(Status status) {
    printf("VM stops with code '%x'\n", status);
    exit(status == STATUS_OK ? 0 : 1);
}

int main(void) {
    PRINT_DEBUG("Start VM Program\n");
    Metadata metadata = {0};
    void *buff = (void *) _binary_cf_code_bin_start;
    PRINT_DEBUG("Binary content start: %p\n", buff);
    cf_load_metadata(&buff, &metadata);
    CF_Library main_program = {0};
    main_program.address_pool = create_hash_map(metadata.pool_size);
    cf_load_pool(&buff, &metadata, main_program.address_pool);
    cf_load_program(&buff, &metadata, &main_program);
    PRINT_DEBUG("Some step");
    cf_load_memory(&buff, &metadata, &main_program);

    if (metadata.entry_point >= main_program.program_size) {
        exit_with(STATUS_ILLEGAL_ENTRY_POINT);
    }
    PRINT_DEBUG("Set entry point\n");
    cf.program_counter = metadata.entry_point;
    PRINT_DEBUG("Load main program into library stack\n");
    cf.libraries[cf.library_size++] = main_program;


    PRINT_DEBUG("Start execution\n");
    Status status;
    do {
        status = cf_execute_inst(&cf);
#ifdef SLOW
        for (size_t i = 0; i < cf.stack_size; i++) {
            printf("%"PRIu64"\n", cf.stack[i].as_u64);
        }
        getchar();
#endif
    } while (status == STATUS_OK);
    exit_with(status);
}

